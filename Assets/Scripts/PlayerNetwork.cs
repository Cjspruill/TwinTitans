using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;

public class PlayerNetwork : NetworkBehaviour
{
    public Transform camera; // Reference to the main camera transform

    public float speed = 6f; // Movement speed
    public float turnSmoothTime = 0.1f; // Smoothing time for turning
    float turnSmoothVelocity; // Velocity used for smoothing turning

    [SerializeField] CharacterController characterController;

    [SerializeField] private BoxCollider lowAttackCollider;
    [SerializeField] private BoxCollider medAttackCollider;
    [SerializeField] private BoxCollider highAttackCollider;

    [SerializeField] GameObject leftArm;
    [SerializeField] GameObject rightArm;
    [SerializeField] GameObject rightLeg;

    [SerializeField] bool lockedOn;
    [SerializeField] Target[] targets;
    [SerializeField]public GameObject closestTarget;
    [SerializeField] int targetIndex;

    [SerializeField] AudioListener audioListener;
    [SerializeField] PlayerInputActions playerControls;
    [SerializeField] InputAction move;
    [SerializeField] InputAction attack;
    [SerializeField] InputAction grab;
    [SerializeField]public InputAction toggleTarget;
    [SerializeField] InputAction switchTarget;

    [SerializeField] float attackTimer;
    [SerializeField] bool comboActive;
    [SerializeField] float comboTime;
    [SerializeField] int comboIndex;

    [SerializeField] EnemyMovement[] enemies;
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
        move.Enable();
        attack.Enable();
        grab.Enable();
        toggleTarget.Enable();
        switchTarget.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
        attack.Disable();
        grab.Disable();
        toggleTarget.Disable();
        switchTarget.Disable();
    }
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {       
            audioListener.enabled = true;           
        }
        else if (!IsOwner)
        {
            camera.gameObject.SetActive(false);
        }
    }

    private void Start()
    { 
        highAttackCollider.enabled = false;
        medAttackCollider.enabled = false;
        lowAttackCollider.enabled = false;
        characterController = GetComponent<CharacterController>();

        enemies = FindObjectsOfType<EnemyMovement>();

        for (int i = 0; i < enemies.Length; i++)
        {
            enemies[i].AddTargetObject(gameObject);
            enemies[i].SetLockOn(true);
        }
        
    }

    void Update()
    {
        if (IsOwner)
        {
            MovePlayer();
            AllowAttacks();
        }
    }

    IEnumerator EnableCollider(BoxCollider boxCollider)
    {
        boxCollider.enabled = true;

        yield return new WaitForSeconds(.15f);

        boxCollider.enabled = false;
    }

    void MovePlayer()
    {
        // Get input for horizontal and vertical movement
        float horizontal = move.ReadValue<Vector2>().x; //Input.GetAxisRaw("Horizontal");
        float vertical = move.ReadValue<Vector2>().y; //Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        // Rotate movement direction based on camera
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + camera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            // transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            moveDir += Physics.gravity;
            characterController.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        
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
        }

        if (attack.WasPressedThisFrame())
        {
            comboActive = true;

            if (attackTimer >= comboTime || comboIndex >= 3)
            {
                attackTimer = 0;
                comboIndex = 0;
                comboActive = false;
            }

            if (attackTimer <= comboTime)
                comboIndex++;

            if (comboIndex == 1)
            {
                StartCoroutine(EnableLimb(leftArm));
            }
            if (comboIndex == 2)
            {
                StartCoroutine(EnableLimb(rightArm));
            }
            if (comboIndex == 3)
            {
                StartCoroutine(EnableLimb(rightLeg));
            }
        }

        if (move.WasPressedThisFrame() && attack.WasPressedThisFrame())
        {
            StartCoroutine(EnableCollider(highAttackCollider));
        }
        else if (move.WasPressedThisFrame() && attack.WasPressedThisFrame())
        {
            StartCoroutine(EnableCollider(lowAttackCollider));
        }
        else if (attack.WasPressedThisFrame())
        {
            StartCoroutine(EnableCollider(medAttackCollider));
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
                // Get the direction to the target
                Vector3 directionToTarget = closestTarget.transform.position - transform.position;

                // Lock the rotation around the X-axis
                directionToTarget.y = 0f;

                // Make the object look at the target
                transform.rotation = Quaternion.LookRotation(directionToTarget);
            }

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