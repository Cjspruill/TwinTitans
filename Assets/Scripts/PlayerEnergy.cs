using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerEnergy : NetworkBehaviour
{
    [field: SerializeField] public int MaxEnergy { get; private set; } = 100;
    [SerializeField] public NetworkVariable<int> CurrentEnergy = new NetworkVariable<int>();

    [SerializeField] private float energyDecayTimer;
    [SerializeField] private float energyDecayTime;
    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        CurrentEnergy.Value = 0;
    }

    private void Update()
    {
        if (!IsServer) return;

        energyDecayTimer -= Time.deltaTime;

        if(energyDecayTimer <= 0)
        {
            CurrentEnergy.Value--;
            energyDecayTimer = energyDecayTime;
        }
    }

    public void DecreaseEnergy(int energyValue)
    {
        ModifyEnergy(-energyValue);
    }
    public void IncreaseEnergy(int energyValue)
    {
        ModifyEnergy(energyValue);
    }

    private void ModifyEnergy(int value)
    {
        int newEnergy = CurrentEnergy.Value + value;
        CurrentEnergy.Value = Mathf.Clamp(newEnergy, 0, MaxEnergy);
    }
    [ServerRpc(RequireOwnership =false)]
    public void IncreaseEnergyServerRpc(int energyValue)
    {
        ModifyEnergy(energyValue);
    }
}
