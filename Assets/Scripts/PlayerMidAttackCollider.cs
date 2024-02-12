using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMidAttackCollider : NetworkBehaviour
{

    [SerializeField] BoxCollider boxCollider;
    [SerializeField] int damage;
    [SerializeField] int power = 1;
    [SerializeField] Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsLocalPlayer)
        {
            boxCollider.enabled = false;
            other.GetComponent<Target>().KnockBack(transform.forward, power);

        }
        else if (IsClient && IsLocalPlayer)
        {
            boxCollider.enabled = false;
            Debug.Log("knockback");
            other.GetComponent<Target>().KnockBackServerRpc(transform.forward, power);
        }
    }
}
