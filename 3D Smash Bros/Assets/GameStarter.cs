using Unity.Netcode;
using UnityEngine;

public class GameStarter : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Vector3 spawnPos = GetSpawnPosition(clientId);
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        // Egyszer� poz�ci� logika
        return new Vector3(clientId * 2f, 0, 0);
    }
}