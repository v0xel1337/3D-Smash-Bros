using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class BombSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private List<NetworkObject> spawnedBombs = new List<NetworkObject>();


    public override void OnNetworkSpawn()
    {
        Debug.Log("TESTTTT");
        if (IsServer)
        {
            RespawnBombs();
            Debug.Log("TESTTTT222");
        }
    }

    public void DeleteBombs()
    {
        foreach (var bomb in spawnedBombs)
        {
            if (bomb != null && bomb.IsSpawned)
            {
                bomb.Despawn(true); // true = destroy GameObject is
            }
        }
        spawnedBombs.Clear();
    }

    public void RespawnBombs()
    {
        foreach (Transform point in spawnPoints)
        {
            GameObject bombGO = Instantiate(bombPrefab, point.position, Quaternion.identity);
            var netObj = bombGO.GetComponent<NetworkObject>();
            netObj.Spawn();

            spawnedBombs.Add(netObj);
        }
    }
}