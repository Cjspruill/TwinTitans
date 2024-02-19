using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class HealthBarPlayer : NetworkBehaviour
{
    [SerializeField] PlayerHealth playerHealth;
    [SerializeField] Image healthBar;
    [SerializeField] Image redBackground;

    public override void OnNetworkSpawn()
    {
       if(!IsClient) { return; }

        playerHealth.CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        playerHealth.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth)
    {
        healthBar.fillAmount = (float)newHealth / playerHealth.MaxHealth;
        StartCoroutine(DepleteHealth());
    }

    IEnumerator DepleteHealth()
    {
        yield return new WaitForSeconds(.5f);
        float newValue = (float)1 / playerHealth.MaxHealth;
        redBackground.fillAmount -= newValue;
    }
}
