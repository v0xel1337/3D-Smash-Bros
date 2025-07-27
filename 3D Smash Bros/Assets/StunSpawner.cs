using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StunSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject stunPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private List<NetworkObject> spawnedStuns = new List<NetworkObject>();
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RespawnBombs();
        }
    }

    public void DeleteBombs()
    {
        foreach (var bomb in spawnedStuns)
        {
            if (bomb != null && bomb.IsSpawned)
            {
                bomb.Despawn(true); // true = destroy GameObject is
            }
        }
        spawnedStuns.Clear();
    }

    public void RespawnBombs()
    {
        foreach (Transform point in spawnPoints)
        {
            GameObject bombGO = Instantiate(stunPrefab, point.position, Quaternion.identity);
            var netObj = bombGO.GetComponent<NetworkObject>();
            netObj.Spawn();

            spawnedStuns.Add(netObj);
        }
    }
}
