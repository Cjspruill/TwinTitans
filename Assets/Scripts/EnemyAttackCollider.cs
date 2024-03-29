using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnemyAttackCollider : NetworkBehaviour
{
    [SerializeField] int damage;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer)
        {
            if (other.GetComponent<PlayerHealth>() != null)
                other.GetComponent<PlayerHealth>().TakeDamage(damage);

        }
    }
}
