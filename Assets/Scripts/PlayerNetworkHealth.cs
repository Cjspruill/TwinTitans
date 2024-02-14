using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;

public class PlayerNetworkHealth : NetworkBehaviour
{
    [SerializeField] NetworkVariable<int> health = new NetworkVariable<int>();

    [SerializeField] Slider healthBar;
    [SerializeField] RectTransform redBackGround;
    [SerializeField] float backgroundNumber;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] float startingHealth;
    // Start is called before the first frame update
    void Start()
    {
    }

     public override void OnNetworkSpawn()
    {
        startingHealth = health.Value;
        healthBar.maxValue = health.Value;
        healthBar.value = health.Value;
    }

    [ClientRpc]
    public void DepleteHealthClientRpc(int value)
    {
        if(IsServer)
        health.Value -= value;

        if (IsOwner)
        {
        healthBar.value = health.Value;
        StartCoroutine(DepleteHealth(1,1));
        }

        if (health.Value <= 0)
            Debug.Log("Dead");
    }

    IEnumerator DepleteHealth(float y, float z)
    {
        float newValue =  1 / startingHealth;
        Vector3 newScale = new Vector3(backgroundNumber, y, z);
        redBackGround.transform.localScale = newScale;
        yield return new WaitForSeconds(.5f);
        backgroundNumber -= newValue;
        Vector3 finalScale = new Vector3(backgroundNumber, y, z);
        redBackGround.transform.localScale = finalScale;
    }
}
