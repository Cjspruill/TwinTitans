using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{

    private static List<PlayerSpawnPoint> spawnPoints = new List<PlayerSpawnPoint>();
    private void OnEnable()
    {
        spawnPoints.Add(this);
    }

    private void OnDisable()
    {
        spawnPoints.Add(this);
    }

    public static Vector3 GetRandomSpawnPos()
    {
        if (spawnPoints.Count == 0)
        {
            return Vector3.zero;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Count)].transform.position;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 1);
    }
}
