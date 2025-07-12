using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ReturnToLobby : NetworkBehaviour
{
    public void ToLobby()
    {
        if (IsHost)
            return;

        NetworkManager.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }
}
