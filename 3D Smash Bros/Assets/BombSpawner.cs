using Unity.Netcode;
using UnityEngine;

public class BombSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnAllBombs();
        }
    }

    private void SpawnAllBombs()
    {
        foreach (Transform point in spawnPoints)
        {
            GameObject bomb = Instantiate(bombPrefab, point.position, Quaternion.identity);
            bomb.GetComponent<NetworkObject>().Spawn();
        }
    }
}