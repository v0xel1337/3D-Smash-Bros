using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

public class LobbyPlayerList : NetworkBehaviour
{
    public static LobbyPlayerList Instance;

    public NetworkList<FixedString64Bytes> playerNames = new NetworkList<FixedString64Bytes>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        playerNames.OnListChanged += OnPlayerListChanged;

        if (IsServer)
        {
            AddPlayerName(NetworkManager.Singleton.LocalClientId);
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            playerNames.OnListChanged -= OnPlayerListChanged;

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            // Don't add host again
            if (clientId != NetworkManager.Singleton.LocalClientId)
            {
                AddPlayerName(clientId);
            }
        }
    }

    public void AddPlayerName(ulong clientId)
    {
        if (IsServer)
        {
            int playerNumber = playerNames.Count + 1;
            string playerName = $"PLAYER {playerNumber}";
            playerNames.Add(playerName);
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<FixedString64Bytes> change)
    {
        RelayManager.Instance.UpdatePlayerListUI(playerNames);
    }
}