using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobby : NetworkBehaviour
{
    public void ToLobby()
    {
        if (IsHost)
            return;
        FindObjectOfType<BombSpawner>().DeleteBombs();
        FindObjectOfType<StunSpawner>().DeleteBombs();

        NetworkManager.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
