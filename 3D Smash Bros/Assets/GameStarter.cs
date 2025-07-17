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
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            var existingPlayer = client.PlayerObject;

            // Ha van már PlayerObject, despawnoljuk és töröljük
            if (existingPlayer != null)
            {
                existingPlayer.Despawn();
                Destroy(existingPlayer.gameObject);
            }

            // Új játékos objektum példányosítása és spawnolása
            Vector3 spawnPos = GetSpawnPosition(clientId);
            GameObject player = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }


    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return new Vector3(53f + clientId * 2f, 0, 61f);
    }
}