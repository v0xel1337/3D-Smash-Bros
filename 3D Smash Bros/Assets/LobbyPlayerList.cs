using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine.SceneManagement;

public class LobbyPlayerList : NetworkBehaviour
{
    public static LobbyPlayerList Instance;
    public GameObject waitingLobby;
    public GameObject main;


    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        LobbyManager.Instance.playerNames.OnListChanged += OnPlayerListChanged;

        if (IsServer)
        {
            // Csak akkor adjuk hozzá a hostot, ha még nincs senki a listában
            if (LobbyManager.Instance.playerNames.Count == 0)
            {
                AddPlayerName(NetworkManager.Singleton.LocalClientId);
            }

            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        if (LobbyManager.Instance.playerNames.Count != 0)
        {
            main.SetActive(false);
            waitingLobby.SetActive(true);
        }    

        RelayManager.Instance.UpdatePlayerListUI(LobbyManager.Instance.playerNames);
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            LobbyManager.Instance.playerNames.OnListChanged -= OnPlayerListChanged;

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
            int playerNumber = LobbyManager.Instance.playerNames.Count + 1;
            string playerName = $"PLAYER {playerNumber}";
            LobbyManager.Instance.playerNames.Add(playerName);
        }
    }

    private void OnPlayerListChanged(NetworkListEvent<FixedString64Bytes> change)
    {
        RelayManager.Instance.UpdatePlayerListUI(LobbyManager.Instance.playerNames);
    }
}