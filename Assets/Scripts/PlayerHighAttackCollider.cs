using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerHighAttackCollider : NetworkBehaviour
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
            other.GetComponent<Target>().Popup(transform.up, power);

        }
        else if (IsClient && IsLocalPlayer)
        {
            boxCollider.enabled = false;
            other.GetComponent<Target>().PopupServerRpc(transform.up, power);
        }
    }
}
