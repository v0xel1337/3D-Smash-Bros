using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class StunSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject stunPrefab;
    [SerializeField] private Vector2 spawnAreaMin; // pl. (-5, -5)
    [SerializeField] private Vector2 spawnAreaMax; // pl. (5, 5)
    [SerializeField] private float spawnHeight = 11f; // Y pozíció

    [SerializeField] private List<NetworkObject> spawnedStuns = new List<NetworkObject>();
    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            RespawnBombs();
            StartCoroutine(ReplaceOldestBombRoutine());

        }
    }

    public void DeleteBombs()
    {
        foreach (var bomb in spawnedStuns)
        {
            if (bomb != null && bomb.IsSpawned)
            {
                bomb.Despawn(true);
            }
        }
        spawnedStuns.Clear();
    }

    public void RespawnBombs()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomPos = GetRandomPositionInArea();
            GameObject bombGO = Instantiate(stunPrefab, randomPos, Quaternion.identity);
            var netObj = bombGO.GetComponent<NetworkObject>();
            netObj.Spawn();
            spawnedStuns.Add(netObj);
        }
    }

    private IEnumerator ReplaceOldestBombRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(10f);

            // Ha van legalább egy stun, akkor törli a legrégebbit
            if (spawnedStuns.Count > 0)
            {
                NetworkObject oldest = spawnedStuns[0];
                if (oldest != null && oldest.IsSpawned)
                {
                    oldest.Despawn(true);
                }
                spawnedStuns.RemoveAt(0);
            }

            // Ezután mindig spawnol egy újat
            Vector3 randomPos = GetRandomPositionInArea();
            GameObject newBomb = Instantiate(stunPrefab, randomPos, Quaternion.identity);
            var netObj = newBomb.GetComponent<NetworkObject>();
            netObj.Spawn();
            spawnedStuns.Add(netObj);
        }
    }

    private Vector3 GetRandomPositionInArea()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float z = Random.Range(spawnAreaMin.y, spawnAreaMax.y); // a Vector2 y = Z itt
        return new Vector3(x, spawnHeight, z);
    }
}
