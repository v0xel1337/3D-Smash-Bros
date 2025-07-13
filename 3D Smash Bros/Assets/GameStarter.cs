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
            // Ellenõrizzük, hogy már van-e player objektum
            if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject == null)
            {
                Vector3 spawnPos = GetSpawnPosition(clientId);
                GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
            }
        }
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return new Vector3(clientId * 2f, 0, 0);
    }
}