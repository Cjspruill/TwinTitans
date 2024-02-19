using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
public class HealthBarEnemy : NetworkBehaviour
{
    [SerializeField] EnemyHealth enemyHealth;
    [SerializeField] Image healthBar;
    [SerializeField] Image redBackGround;

    public override void OnNetworkSpawn()
    {
        if (!IsClient) { return; }

        enemyHealth.CurrentHealth.OnValueChanged += HandleHealthChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        enemyHealth.CurrentHealth.OnValueChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int oldHealth, int newHealth)
    {
        healthBar.fillAmount = (float)newHealth / enemyHealth.MaxHealth;
        StartCoroutine(DepleteHealth());
    }

    IEnumerator DepleteHealth()
    {
        yield return new WaitForSeconds(.5f);
        float newValue = (float)1 / enemyHealth.MaxHealth;
        redBackGround.fillAmount -= newValue;
    }


}