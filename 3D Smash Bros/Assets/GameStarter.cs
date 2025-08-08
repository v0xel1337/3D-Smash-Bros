using Unity.Netcode;
using UnityEngine;

public class GameStarter : NetworkBehaviour
{
    [SerializeField] private GameObject[] characterPrefabs; // 0 = Character1, 1 = Character2

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var client = NetworkManager.Singleton.ConnectedClients[clientId];
            var existingPlayer = client.PlayerObject;

            int charIndex = 0;

            if (existingPlayer != null && existingPlayer.TryGetComponent(out PlayerData playerData))
            {
                charIndex = playerData.CharacterIndex.Value; // minden játékos sajátját használja
            }

            if (existingPlayer != null)
            {
                existingPlayer.Despawn();
                Destroy(existingPlayer.gameObject);
            }

            Vector3 spawnPos = GetSpawnPosition(clientId);
            GameObject player = Instantiate(characterPrefabs[charIndex], spawnPos, Quaternion.identity);
            player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
        }
    }


    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return new Vector3(53f + clientId * 2f, 0, 61f);
    }
}