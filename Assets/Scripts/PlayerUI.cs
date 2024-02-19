using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine.UI;

public class PlayerUI : NetworkBehaviour
{
    [SerializeField] private PlayerNetwork player;
    [SerializeField] private TMP_Text playerNameText;

    [SerializeField] private PlayerEnergy playerEnergy;
    [SerializeField] Image energyBar;

    // Start is called before the first frame update
    private void Start()
    {
        HandlePlayerNameChanged(string.Empty, player.PlayerName.Value);

        player.PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        playerNameText.text = newName.ToString();
    }

    private new void OnDestroy()
    {
        player.PlayerName.OnValueChanged -= HandlePlayerNameChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsClient) { return; }

        playerEnergy.CurrentEnergy.OnValueChanged += HandleEnergyChanged;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsClient) { return; }

        playerEnergy.CurrentEnergy.OnValueChanged -= HandleEnergyChanged;
    }

    private void HandleEnergyChanged(int oldEnergy, int newEnergy)
    {
        energyBar.fillAmount = (float)newEnergy / playerEnergy.MaxEnergy;
    }
}
