using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Unity.Collections;

public class PlayerNetwork : NetworkBehaviour
{
    public Transform camera; // Reference to the main camera transform

    public float speed = 6f; // Movement speed
    [SerializeField] float gravityScale = 9.8f;
    [SerializeField] float speedModifier = 2f;
    [SerializeField] float jumpPower = 30f;
    [SerializeField] float ySpeed;
    [SerializeField] bool isRunning;
    [SerializeField] float groundCheckDistance = 0.2f;
    [SerializeField] LayerMask groundLayer;
    public float turnSmoothTime = 0.1f; // Smoothing time for turning
    float turnSmoothVelocity; // Velocity used for smoothing turning

    [SerializeField] CharacterController characterController;
    [SerializeField] Animator animator;
    [SerializeField] Vector3 moveDir;

    [SerializeField] private BoxCollider lowAttackCollider;
    [SerializeField] private BoxCollider medAttackCollider;
    [SerializeField] private BoxCollider highAttackCollider;

    [SerializeField] GameObject leftArm;
    [SerializeField] GameObject rightArm;
    [SerializeField] GameObject rightLeg;

    [SerializeField] bool lockedOn;
    [SerializeField] Target[] targets;
    [SerializeField] public GameObject closestTarget;
    [SerializeField] int targetIndex;
    [SerializeField] float lockOnSpeed = 1f;


    [SerializeField] AudioListener audioListener;
    [SerializeField] PlayerInputActions playerControls;
    [SerializeField] InputAction move;
    [SerializeField] InputAction attack;
    [SerializeField] InputAction grab;
    [SerializeField]public InputAction toggleTarget;
    [SerializeField] InputAction switchTarget;
    [SerializeField] InputAction leftEvade;
    [SerializeField] InputAction rightEvade;
    [SerializeField] InputAction forwardEvade;
    [SerializeField] InputAction backEvade;
    [SerializeField] InputAction run;
    [SerializeField] InputAction jump;

    [SerializeField] float attackTimer;
    [SerializeField] bool comboActive;
    [SerializeField] float comboTime;
    [SerializeField] int comboIndex;

    [SerializeField] float evadeDistance;

    [SerializeField] EnemyMovement[] enemies;

    public NetworkVariable<FixedString32Bytes> PlayerName = new NetworkVariable<FixedString32Bytes>();

    [SerializeField] private float leftEvadeTimer;
    [SerializeField] private int evadeIndex=0;
    [SerializeField] private float rightEvadeTimer;
    [SerializeField] private float forwardEvadeTimer;
    [SerializeField] private float backEvadeTimer;
    [SerializeField] private float evadeTime;

    [SerializeField] private bool isEnteringLeftEvade;
    [SerializeField] private bool isEnteringRightEvade;
    [SerializeField] private bool isEnteringForwardEvade;
    [SerializeField] private bool isEnteringBackEvade;

    [SerializeField] bool isReady;

    [SerializeField] float horizontalInput;
    [SerializeField] float verticalInput;
    [SerializeField] bool isJumping;
    [SerializeField] bool isGrounded;


    private void Awake()
    {
        playerControls = new PlayerInputActions();
    }
    private void OnEnable()
    {
        move = playerControls.Player.Move;
        attack = playerControls.Player.Attack;
        grab = playerControls.Player.Grab;
        toggleTarget = playerControls.Player.ToggleTarget;
        switchTarget = playerControls.Player.SwitchTarget;
        leftEvade = playerControls.Player.LeftEvade;
        rightEvade = playerControls.Player.RightEvade;
        forwardEvade = playerControls.Player.ForwardEvade;
        backEvade = playerControls.Player.BackEvade;
        run = playerControls.Player.Run;
        jump = playerControls.Player.Jump;
        move.Enable();
        attack.Enable();
        grab.Enable();
        toggleTarget.Enable();
        switchTarget.Enable();
        leftEvade.Enable();
        rightEvade.Enable();
        forwardEvade.Enable();
        backEvade.Enable();
        run.Enable();
        jump.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
        attack.Disable();
        grab.Disable();
        toggleTarget.Disable();
        switchTarget.Disable();
        leftEvade.Disable();
        rightEvade.Disable();
        forwardEvade.Disable();
        backEvade.Disable();
        run.Disable();
        jump.Disable();
    }
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            UserData userData = HostSingleton.Instance.GameManger.NetworkServer.GetUserDataByClientID(OwnerClientId);
            PlayerName.Value = userData.userName;
        }

        if (IsOwner)
        {
            audioListener.enabled = true;

            highAttackCollider.enabled = false;
            medAttackCollider.enabled = false;
            lowAttackCollider.enabled = false;
            characterController = GetComponent<CharacterController>();


   
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

        }
        else if (!IsOwner)
        {
            camera.gameObject.SetActive(false);
        }


        Invoke("RunSetup", 1f);  
    }

    void RunSetup()
    {
        isReady = true;
        enemies = FindObjectsOfType<EnemyMovement>();

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].AddTargetObject(gameObject);
            enemies[i].SetLockOn(true);
        }


        enemies = FindObjectsOfType<EnemyMovement>();

        for (int i = 0; i < enemies.Length; i++)
        {
            //    enemies[i].UpdateHealthBar();
        }
    }

    void Update()
    {
        if (!isReady) return;

        if (IsLocalPlayer)
        {
      

            //Left Evade
            if (isEnteringLeftEvade)
                leftEvadeTimer += Time.deltaTime;

            if (leftEvadeTimer >= evadeTime)
            {
                isEnteringLeftEvade = false;
                leftEvadeTimer = 0;
                evadeIndex = 0;
            }

            //Right Evade
            if (isEnteringRightEvade)
                rightEvadeTimer += Time.deltaTime;

            if(rightEvadeTimer >= evadeTime)
            {
                isEnteringRightEvade = false;
                rightEvadeTimer = 0;
                evadeIndex = 0;
            }

            //Forward Evade
            if (isEnteringForwardEvade)
                forwardEvadeTimer += Time.deltaTime;

            if(forwardEvadeTimer >= evadeTime)
            {
                isEnteringForwardEvade = false;
                forwardEvadeTimer = 0;
                evadeIndex = 0;
            }

            //Back Evade
            if (isEnteringBackEvade)
                backEvadeTimer += Time.deltaTime;

            if(backEvadeTimer >= evadeTime)
            {
                isEnteringBackEvade = false;
                backEvadeTimer = 0;
                evadeIndex = 0;
            }
            AllowAttacks();
            AllowEvades();

            UpdateInput();
        }
    }

    void UpdateInput()
    {
        horizontalInput = move.ReadValue<Vector2>().x;
        verticalInput = move.ReadValue<Vector2>().y;

        moveDir = new Vector3(horizontalInput, 0f, verticalInput) * speed;
        moveDir = transform.TransformDirection(moveDir);

        if (jump.WasPressedThisFrame() && isGrounded)
        {
            ySpeed = jumpPower;
            isGrounded = false;
            isJumping = true;
            animator.SetBool("IsJumping", true);
            animator.SetBool("IsFalling", true);
            animator.SetBool("IsGrounded", false);
        }

        if (run.inProgress)
        {
            isRunning = true;
        }
        else if (run.WasReleasedThisFrame())
        {
            isRunning = false;
        }

        if (isRunning)
        {
            animator.SetFloat("Speed", verticalInput * 2, .15f, Time.deltaTime);
            animator.SetFloat("Direction", horizontalInput * 2, .15f, Time.deltaTime);
        }
        else if (!isRunning)
        {
            animator.SetFloat("Speed", verticalInput, .15f, Time.deltaTime);
            animator.SetFloat("Direction", horizontalInput, .15f, Time.deltaTime);
        }



    }

    private void FixedUpdate()
    {        
        if (!isReady) return;

        if (IsLocalPlayer)
        {
            MovePlayer();
        }
    }

    private void OnDrawGizmos()
    {
            Gizmos.DrawSphere(transform.position + Vector3.up * 0.1f, groundCheckDistance);
        
    }
    void MovePlayer()
    {
        isGrounded = Physics.CheckSphere(transform.position + Vector3.up * 0.1f, groundCheckDistance, groundLayer);
       
        if (isGrounded)
        {
            animator.SetBool("IsGrounded", true);
            animator.SetBool("IsJumping", false);
            isJumping = false;         
            animator.SetBool("IsFalling", false);

            ySpeed = 0;
        }
        else
        {

            characterController.stepOffset = 0;
            animator.SetBool("IsGrounded", false);
           

            if (isJumping && moveDir.y < 0 || moveDir.y < -2)
            {
                animator.SetBool("IsFalling", true);
            }

            ySpeed -= gravityScale * Time.deltaTime;
        }

        moveDir.y = ySpeed;
        characterController.Move(moveDir * Time.deltaTime);

    }


    void AllowAttacks()
    {
        if(comboActive)
            attackTimer += Time.deltaTime;

        if (attackTimer >= comboTime || comboIndex >= 3)
        {
            attackTimer = 0;
            comboIndex = 0;
            comboActive = false;
            animator.SetInteger("ComboIndex", comboIndex);
        }

        if (move.ReadValue<Vector2>().y > .1f && attack.WasPressedThisFrame())
        {
            animator.SetInteger("AttackIndex", 1);

            comboActive = true;

            if (attackTimer >= comboTime || comboIndex >= 3)
            {
                attackTimer = 0;
                comboIndex = 0;
                comboActive = false;
            }

            if (attackTimer <= comboTime)
            {
                animator.SetTrigger("Attack");
                animator.SetInteger("ComboIndex", comboIndex);
                comboIndex++;
            }
        }
        else if (move.ReadValue<Vector2>().y < -.1f && attack.WasPressedThisFrame())
        {
            comboActive = true;

            animator.SetInteger("AttackIndex", 2);
            if (attackTimer >= comboTime || comboIndex >= 3)
            {
                attackTimer = 0;
                comboIndex = 0;
                comboActive = false;
            }

            if (attackTimer <= comboTime)
            {
                animator.SetTrigger("Attack");
                animator.SetInteger("ComboIndex", comboIndex);
                comboIndex++;
            }
        }
        else if (attack.WasPressedThisFrame())
        {

            animator.SetInteger("AttackIndex", 0);

            comboActive = true;

            if (attackTimer >= comboTime || comboIndex >= 3)
            {
                attackTimer = 0;
                comboIndex = 0;
                comboActive = false;
            }

            if (attackTimer <= comboTime)
            {
                animator.SetTrigger("Attack");
                animator.SetInteger("ComboIndex", comboIndex);
                comboIndex++;
            }
        }


        if (toggleTarget.WasPressedThisFrame())
        {
            if (!lockedOn)
            {
                targets = FindObjectsOfType<Target>();
                closestTarget = FindClosestTarget();
                lockedOn = true;
            }
            else
            {
                lockedOn = false;
            }
        }
        if (switchTarget.WasPressedThisFrame())
        {
            if (targets != null)
            {
                targetIndex++;

                if (targetIndex >= targets.Length)
                    targetIndex = 0;
               
                closestTarget = targets[targetIndex].gameObject;
            }
        }

        if (lockedOn)
        {
            if (targets != null)
            {
                // The step size is equal to speed times frame time.
                float singleStep = lockOnSpeed * Time.deltaTime;
                Quaternion rotTarget = Quaternion.LookRotation(closestTarget.transform.position - transform.position);
                // Make the object look at the target
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotTarget, singleStep);
            }

        }
    }

    void AllowEvades()
    {
        if (forwardEvade.WasPressedThisFrame())
        {
            //If isentering other evades is true turn them off.
            if(isEnteringLeftEvade)
            {
                isEnteringLeftEvade = false;
                leftEvadeTimer = 0;
                evadeIndex = 0;
            }
            if(isEnteringRightEvade)
            {
                isEnteringRightEvade = false;
                rightEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringBackEvade)
            {
                isEnteringBackEvade = false;
                backEvadeTimer = 0;
                evadeIndex = 0;
            }




            if (!isEnteringForwardEvade && forwardEvadeTimer < evadeTime)
            {
                isEnteringForwardEvade = true;
                evadeIndex++;
            }
            else if (isEnteringForwardEvade)
            {
                evadeIndex++;
            }

            if (forwardEvadeTimer < evadeTime && evadeIndex == 2)
            {
                evadeIndex = 0;
                isEnteringForwardEvade = false;
                forwardEvadeTimer = 0;
                EvadeForward();
            }
        }

        else if (backEvade.WasPressedThisFrame())
        {
            //If isentering other evades is true turn them off.
            if (isEnteringLeftEvade)
            {
                isEnteringLeftEvade = false;
                leftEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringRightEvade)
            {
                isEnteringRightEvade = false;
                rightEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringForwardEvade)
            {
                isEnteringForwardEvade = false;
                forwardEvadeTimer = 0;
                evadeIndex = 0;
            }







            if (!isEnteringBackEvade && backEvadeTimer < evadeTime)
            {
                isEnteringBackEvade = true;
                evadeIndex++;
            }
            else if (isEnteringBackEvade)
            {
                evadeIndex++;
            }

            if (backEvadeTimer < evadeTime && evadeIndex == 2)
            {
                evadeIndex = 0;
                isEnteringBackEvade = false;
                backEvadeTimer = 0;

                EvadeBack();
            }
        }
        else if (leftEvade.WasPressedThisFrame())
        {
            //If isentering other evades is true turn them off.
            if (isEnteringForwardEvade)
            {
                isEnteringForwardEvade = false;
                forwardEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringBackEvade)
            {
                isEnteringBackEvade = false;
                backEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringRightEvade)
            {
                isEnteringRightEvade = false;
                rightEvadeTimer = 0;
                evadeIndex = 0;
            }






            if (!isEnteringLeftEvade && leftEvadeTimer < evadeTime)
            {
                isEnteringLeftEvade = true;
                evadeIndex++;
            }
            else if (isEnteringLeftEvade)
            {
                evadeIndex++;
            }

            if (leftEvadeTimer < evadeTime && evadeIndex == 2)
            {
                evadeIndex = 0;
                isEnteringLeftEvade = false;
                leftEvadeTimer = 0;
                Debug.Log("Left evade");
                EvadeLeft();
            }
        }
        else if (rightEvade.WasPressedThisFrame())
        {
            //If isentering other evades is true turn them off.
            if (isEnteringLeftEvade)
            {
                isEnteringLeftEvade = false;
                leftEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringForwardEvade)
            {
                isEnteringForwardEvade = false;
                forwardEvadeTimer = 0;
                evadeIndex = 0;
            }
            if (isEnteringBackEvade)
            {
                isEnteringBackEvade = false;
                backEvadeTimer = 0;
                evadeIndex = 0;
            }







            if (!isEnteringRightEvade && rightEvadeTimer < evadeTime)
            {
                isEnteringRightEvade = true;
                evadeIndex++;
            }
            else if (isEnteringRightEvade)
            {
                evadeIndex++;
            }

            if (rightEvadeTimer < evadeTime && evadeIndex == 2)
            {
                evadeIndex = 0;
                isEnteringRightEvade = false;
                rightEvadeTimer = 0;
                Debug.Log("Right evade");
                EvadeRight();
            }
        }
    }

    void EvadeForward()
    {
        float vertical = move.ReadValue<Vector2>().y; //Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(0f, 0f, vertical).normalized;

        // Rotate movement direction based on camera
        if (direction.magnitude >= 0.1f)
        {
            Debug.Log("Evaded forward"); 
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
           // moveDir += Physics.gravity;
            characterController.Move(moveDir.normalized * evadeDistance * speed * Time.deltaTime);
        }
    }

    void EvadeBack()
    {
        float vertical = move.ReadValue<Vector2>().y; //Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(0f, 0f, vertical).normalized;

        // Rotate movement direction based on camera
        if (direction.magnitude >= 0.1f)
        {
            Debug.Log("Evaded Back");
   
            Vector3 moveDir = -transform.forward;
           // moveDir += Physics.gravity;
            characterController.Move(moveDir.normalized * evadeDistance * speed * Time.deltaTime);
        }
    }
    void EvadeLeft()
    {
        float horizontal = move.ReadValue<Vector2>().x; //Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, 0f).normalized;

        // Rotate movement direction based on camera
        if (direction.magnitude >= 0.1f)
        {
            Debug.Log("Evaded Left");

            Vector3 moveDir = -transform.right;
          //  moveDir += Physics.gravity;
            characterController.Move(moveDir.normalized * evadeDistance * speed * Time.deltaTime);
        }
    }

    void EvadeRight()
    {
        float horizontal = move.ReadValue<Vector2>().x; //Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f,0f).normalized;

        // Rotate movement direction based on camera
        if (direction.magnitude >= 0.1f)
        {
            Debug.Log("Evaded Right");

            Vector3 moveDir = transform.right;
          //  moveDir += Physics.gravity;
            characterController.Move(moveDir.normalized * evadeDistance * speed * Time.deltaTime);
        }
    }

    IEnumerator EnableLimb(GameObject limb)
    {
        limb.SetActive(true);
        yield return new WaitForSeconds(.15f);
        limb.SetActive(false);
    }

   public GameObject FindClosestTarget()
    {
        float distance = 100f;
        GameObject closestObject = null;
        for (int i = 0; i < targets.Length; i++)
        {
            float objectDistance = Vector3.Distance(transform.position, targets[i].transform.position);

            if (objectDistance < distance)
            {
                closestObject = targets[i].gameObject;
                distance = objectDistance;
            }
        }

        return closestObject;
    }

    public void SetLockedOn(bool value)
    {
        lockedOn = value;
    }
}