using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private HashSet<ulong> alivePlayers = new HashSet<ulong>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    void Update()
    {
        Debug.Log(alivePlayers.Count);
    }

    public void RegisterPlayer(ulong clientId)
    {
        if (!IsServer) return;
        alivePlayers.Add(clientId);
    }

    public void UnregisterPlayer(ulong clientId)
    {
        if (!IsServer) return;

        alivePlayers.Remove(clientId);
        Debug.Log("ALIVE LEFT: " + alivePlayers.Count);

        if (alivePlayers.Count == 1)
        {
            foreach (var id in alivePlayers)
            {
                ShowWinUIClientRpc(id);
            }
        }
    }

    [ClientRpc]
    private void ShowWinUIClientRpc(ulong winnerId)
    {
        if (NetworkManager.Singleton.LocalClientId == winnerId)
        {
            Debug.Log("You are the winner!");
            GameUI.Instance.GameplayUI.SetActive(false);
            GameUI.Instance.winObject.SetActive(true);
            GameUI.Instance.FightEnd.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}