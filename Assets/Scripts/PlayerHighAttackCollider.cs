using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerHighAttackCollider : NetworkBehaviour
{

    [SerializeField] BoxCollider boxCollider;
    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] int damage;
    [SerializeField] int power = 1;
    [SerializeField] Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsServer && IsLocalPlayer)
        {
            if (other.GetComponent<EnemyHealth>() != null)
                other.GetComponent<EnemyHealth>().TakeDamage(damage);
            if (other.GetComponent<EnemyMovement>() != null)
                other.GetComponent<EnemyMovement>().Popup(power);
            playerEnergy.IncreaseEnergy(1);
        }
        else if (IsClient && IsLocalPlayer)
        {
            if (other.GetComponent<EnemyHealth>() != null)
                other.GetComponent<EnemyHealth>().TakeDamageServerRpc(damage);
            if (other.GetComponent<EnemyMovement>() != null)
                other.GetComponent<EnemyMovement>().PopupServerRpc(power);
            playerEnergy.IncreaseEnergyServerRpc(1);
        }
    }
}
