using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class EnemyMovement : NetworkBehaviour
{
    [SerializeField] enum Movement
    {
        Forward,
        Back,
        Left,
        Right,
        Idle
    }
    [SerializeField] Movement currentMovement;
    [SerializeField] int currentMovementIndex;
    [SerializeField] float moveTimer;
    [SerializeField] float moveTime;
    [SerializeField] float minMoveTime;
    [SerializeField] float maxMoveTime;
    [SerializeField] float minDistance;
    [SerializeField] float maxDistance;
    [SerializeField] CharacterController characterController;

    [SerializeField] List<GameObject> targets;
    [SerializeField] GameObject currentTarget;
    [SerializeField] int targetIndex;
    [SerializeField] bool lockedOn;
    [SerializeField] float speed = 2;

    [SerializeField] NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] Slider healthBar;

    [SerializeField] BoxCollider attackCollider;
    [SerializeField] bool inCombat;
    [SerializeField] bool canAttack;
    [SerializeField] bool canMove;
    [SerializeField] bool attack;
    [SerializeField] float fullComboTimer;
    [SerializeField] float attackTimer;
    [SerializeField] float attackTime;
    [SerializeField] float minAttackTime;
    [SerializeField] float maxAttackTime;
    [SerializeField] bool comboActive;
    [SerializeField] float comboTime;
    [SerializeField] int comboIndex;

    [SerializeField] GameObject leftArm;
    [SerializeField] GameObject rightArm;
    [SerializeField] GameObject rightLeg;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        attackCollider.enabled = false;
        healthBar.maxValue = health.Value;
        healthBar.value = health.Value;
    }

    

    // Update is called once per frame
    void Update()
    {
        if (IsServer)
        {
            if (currentTarget == null)
                return;

            MoveEnemy();
        }
    }

    void MoveEnemy()
    {
        if(canMove)
        moveTimer += Time.deltaTime;


        if (inCombat)
        {
            if (comboActive)
                fullComboTimer += Time.deltaTime;
            if (canAttack)
                attackTimer += Time.deltaTime;

            if (fullComboTimer >= comboTime || comboIndex >= 3)
            {
                fullComboTimer = 0;
                comboIndex = 0;
                comboActive = false;
            }

            if (attackTimer >= attackTime)
            {
                attack = true;
                attackTimer = 0;
                attackTime = Random.Range(minAttackTime, maxAttackTime);
            }

            if (attack)
            {
                attack = false;
                comboActive = true;

                if (fullComboTimer >= comboTime || comboIndex >= 3)
                {
                    fullComboTimer = 0;
                    comboIndex = 0;
                    comboActive = false;
                }

                if (fullComboTimer <= comboTime)
                    comboIndex++;

                StartCoroutine(Attack());

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
        }
        else
        {

        }

        if(moveTimer > moveTime)
        {
            moveTimer = 0;
            moveTime = Random.Range(minMoveTime, maxMoveTime);

            currentMovementIndex = Random.Range(0, 5);
            switch (currentMovementIndex)
            {
                case 0:
                    currentMovement = Movement.Forward;
                    break;
                case 1:
                    currentMovement = Movement.Back;
                    break;
                case 2:
                    currentMovement = Movement.Left;
                    break;
                case 3:
                    currentMovement = Movement.Right;
                    break;
                case 4:
                    currentMovement = Movement.Idle;
                    break;
            }
        }

        switch (currentMovement)
        {
            case Movement.Forward:
                MoveForward();
                break;
            case Movement.Back:
                MoveBack();
                break;
            case Movement.Left:
                MoveLeft();
                break;
            case Movement.Right:
                MoveRight();
                break;
            case Movement.Idle:
                Idle();
                break;
            default:
                break;
        }
    

        if (lockedOn)
        {
            // Get the direction to the target
            Vector3 directionToTarget = currentTarget.transform.position - transform.position;

            // Lock the rotation around the X-axis
            directionToTarget.y = 0f;

            // Make the object look at the target
            transform.rotation = Quaternion.LookRotation(directionToTarget);
        }
    }

    public void SetLockOn(bool value)
    {
        if (currentTarget == null)
            ChangeTarget();

        lockedOn = value;
    }

    void ChangeTarget()
    {
        targetIndex++;
       
        if (targetIndex >= targets.Count)
            targetIndex = 0;
        
        currentTarget = targets[targetIndex];
    }

    public void AddTargetObject(GameObject target)
    {
        targets.Add(target);
    }

    void MoveLeft()
    {
        canAttack = false;
        inCombat = false;
        var direction = -transform.right;
        direction += Physics.gravity;
        characterController.Move(direction * speed * Time.deltaTime);
    }
    void MoveRight()
    {
        inCombat = false;
        canAttack = false;
        var direction = transform.right;
        direction += Physics.gravity;
        characterController.Move(direction * speed * Time.deltaTime);
    }
    void MoveBack()
    {
        inCombat = false;
        canAttack = false;
        var direction = -transform.forward;
        direction += Physics.gravity;
        characterController.Move(direction * speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, currentTarget.transform.position) > maxDistance)
        {
            currentMovement = Movement.Forward;
        }
    }
    void MoveForward()
    {
        inCombat = false;
        canAttack = false;
        var direction = transform.forward;
        direction += Physics.gravity;
        characterController.Move(direction * speed * Time.deltaTime);

        if(Vector3.Distance(transform.position,currentTarget.transform.position) < minDistance)
        {
            inCombat = true;
            canAttack = true;
            canMove = true;
        }
    }

    void Idle()
    {
        if (Vector3.Distance(transform.position, currentTarget.transform.position) < minDistance)
        {
            inCombat = true;
            canAttack = true;
            canMove = false;
        }
        else
        {
            inCombat = false;
            canAttack = false;
            canMove = true;
            currentMovement = Movement.Forward;
        }
      
    }

    public void DepleteHealth(int value)
    {
        health.Value -= value;

        healthBar.value = health.Value;

        if (health.Value <= 0)
            OnNetworkDespawn();      
        
        currentMovement = Movement.Idle;
    }


    [ServerRpc(RequireOwnership = false)]
    public void DepleteHealthServerRpc(int value)
    {
        health.Value -= value;

        healthBar.value = health.Value;

        if (health.Value <= 0)
            OnNetworkDespawnServerRpc();
        
        currentMovement = Movement.Idle;
    }

    IEnumerator Attack()
    {
        canAttack = false;
        attackCollider.enabled = true;
        yield return new WaitForSeconds(.15f);
        attackCollider.enabled = false;
        attack = false;
        canAttack = true;
    }

    IEnumerator EnableLimb(GameObject limb)
    {
        limb.SetActive(true);
        yield return new WaitForSeconds(.15f);
        limb.SetActive(false);
    }

    public override void OnNetworkDespawn()
    {
        gameObject.SetActive(false);
    }
    [ServerRpc(RequireOwnership =false)]
    public void OnNetworkDespawnServerRpc()
    {
        gameObject.SetActive(false);
    }
}
