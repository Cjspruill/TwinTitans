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
    [SerializeField]  GameObject currentTarget;
    [SerializeField] float targetTimer;
    [SerializeField] float targetTime;
    [SerializeField] float minTargetTime;
    [SerializeField] float maxTargetTime;

    [SerializeField] NetworkVariable<int> targetIndex = new NetworkVariable<int>();
    [SerializeField] bool lockedOn;
    [SerializeField] float speed = 2;

    [SerializeField] NetworkVariable<int> health = new NetworkVariable<int>();
    [SerializeField] Slider healthBar;
    [SerializeField] RectTransform redBackGround;
    [SerializeField] float backgroundNumber;
    [SerializeField] float startingHealth;

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
    [SerializeField] NetworkVariable <int> comboIndex = new NetworkVariable<int>();



    [SerializeField] GameObject leftArm;
    [SerializeField] GameObject rightArm;
    [SerializeField] GameObject rightLeg;
    // Start is called before the first frame update
    void Start()
    {
        characterController = GetComponent<CharacterController>();
        attackCollider.enabled = false;

        startingHealth = health.Value;
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

        if(targets.Count != 1)
        {
            targetTimer += Time.deltaTime;
            if (targetTimer>= targetTime)
            {
                ChangeTarget();
            targetTimer = 0;
            targetTime = Random.Range(minTargetTime, maxTargetTime);

            }
        }

        if (inCombat)
        {
            if (comboActive)
                fullComboTimer += Time.deltaTime;
            if (canAttack)
                attackTimer += Time.deltaTime;

            if (fullComboTimer >= comboTime || comboIndex.Value >= 3)
            {
                fullComboTimer = 0;
                comboIndex.Value = 0;
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

                if (fullComboTimer >= comboTime || comboIndex.Value >= 3)
                {
                    fullComboTimer = 0;
                    comboIndex.Value= 0;
                    comboActive = false;
                }

                if (fullComboTimer <= comboTime)
                    comboIndex.Value++;

                AttackClientRpc();

                if (comboIndex.Value == 1)
                {
                    EnableLeftArmClientRpc();
                }
                if (comboIndex.Value == 2)
                {
                    EnableRightArmClientRpc();
                }
                if (comboIndex.Value == 3)
                {
                    EnableRightLegClientRpc();
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
        if (!IsServer) return;

        targetIndex.Value++;
       
        if (targetIndex.Value >= targets.Count)
            targetIndex.Value = 0;
        
        currentTarget = targets[targetIndex.Value];
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

    public void UpdateHealthBar()
    {
        healthBar.value = health.Value;
        StartCoroutine(DepleteHealth(1, 1));
        if (IsServer && health.Value <= 0)
        {
            NetworkObject.Despawn();
        }
        else if (IsClient && health.Value <= 0)
        {
            DespawnServerRpc();
        }
    }

    IEnumerator DepleteHealth(float y, float z)
    {
        float newValue = 1 / startingHealth;
        Vector3 newScale = new Vector3(backgroundNumber, y, z);
        redBackGround.transform.localScale = newScale;
        yield return new WaitForSeconds(.5f);
        backgroundNumber -= newValue;
        Vector3 finalScale = new Vector3(backgroundNumber, y, z);
        redBackGround.transform.localScale = finalScale;
    }

    [ServerRpc(RequireOwnership =false)]
    void DespawnServerRpc()
    {
            NetworkObject.Despawn();
    }

    [ServerRpc(RequireOwnership = false)]
    public void DepleteHealthServerRpc(int value)
    {
        if (IsServer)
        health.Value -= value;

        if (health.Value <= 0)
        {
            TurnOffObject();
        }

        currentMovement = Movement.Idle;
    }

    [ClientRpc]
    public void DepleteHealthClientRpc(int value)
    {
        if (IsServer)
        health.Value -= value;


        if (health.Value <= 0)
        {
            TurnOffObject();
        }

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

    [ClientRpc]
    void EnableLeftArmClientRpc()
    {
        leftArm.SetActive(true);
        Invoke("DisableLimbs", .15f);
    }
    [ClientRpc]
    void EnableRightArmClientRpc()
    {
        rightArm.SetActive(true);
        Invoke("DisableLimbs", .15f);
    }
    [ClientRpc]
    void EnableRightLegClientRpc()
    {
        rightLeg.SetActive(true);
        Invoke("DisableLimbs", .15f);
    }

    void DisableLimbs()
    {
        leftArm.SetActive(false);
        rightArm.SetActive(false);
        rightLeg.SetActive(false);
    }

    [ClientRpc]
    void AttackClientRpc()
    {
        canAttack = false;
        attackCollider.enabled = true;
        Invoke("DisableAttackColliders", .15f);
    }

    void DisableAttackColliders()
    {
        attackCollider.enabled = false;
        attack = false;
        canAttack = true;
    }
    public void TurnOffObject()
    {
        gameObject.SetActive(false);
    }
}
