using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Target : NetworkBehaviour
{
    [SerializeField] Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void KnockBack(Vector3 direction, int power)
    {
        rb.AddForce(direction * power, ForceMode.Impulse);
    }
    [ServerRpc(RequireOwnership = false)]
    public void KnockBackServerRpc(Vector3 direction, int power)
    {
        rb.AddForce(direction * power, ForceMode.Impulse);
    }
    public void Popup(Vector3 direction, int power)
    {
        rb.AddForce(direction * power, ForceMode.Impulse);
    }
    [ServerRpc(RequireOwnership = false)]
    public void PopupServerRpc(Vector3 direction, int power)
    {
        rb.AddForce(direction * power, ForceMode.Impulse);
    }
}