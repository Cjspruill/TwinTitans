using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEvents : MonoBehaviour
{

    [SerializeField] BoxCollider lowAttackCollider;
    [SerializeField] BoxCollider medAttackCollider;
    [SerializeField] BoxCollider highAttackCollider;

    public void EnableCollider(string collider)
    {
        switch (collider)
        {
            case "LowAttackCollider":
                lowAttackCollider.enabled = true;
                break;
            case "MedAttackCollider":
                medAttackCollider.enabled = true;
                break;
            case "HighAttackCollider":
                highAttackCollider.enabled = true;
                break;
            default:
                break;
        }
    }

    public void DisableColliders()
    {
        lowAttackCollider.enabled = false;
        medAttackCollider.enabled = false;
        highAttackCollider.enabled = false;
    }
}
