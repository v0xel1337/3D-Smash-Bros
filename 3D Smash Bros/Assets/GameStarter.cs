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

            // Ha van m�r PlayerObject, despawnoljuk �s t�r�lj�k
            if (existingPlayer != null)
            {
                existingPlayer.Despawn();
                Destroy(existingPlayer.gameObject);
            }

            // �j j�t�kos objektum p�ld�nyos�t�sa �s spawnol�sa
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