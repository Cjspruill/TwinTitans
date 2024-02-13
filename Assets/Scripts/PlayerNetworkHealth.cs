using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class PlayerNetworkHealth : NetworkBehaviour
{
    [SerializeField] NetworkVariable<int> health;
    [SerializeField] Slider healthBar;
    // Start is called before the first frame update
    void Start()
    {
    }

     public override void OnNetworkSpawn()
    {
        healthBar.maxValue = health.Value ;
        healthBar.value = health.Value;
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void DepleteHealth(int value)
    {
        health.Value -= value;

        healthBar.value = health.Value;

        if (health.Value <= 0)
            Debug.Log("Dead");

    }

    //[ServerRpc(RequireOwnership =false)]
    //public void DepleteHealthServerRpc(int value)
    //{
    //    health -= value;

    //    healthBar.value = health;

    //    if (health <= 0)
    //        Debug.Log("Dead");

    //}
}
