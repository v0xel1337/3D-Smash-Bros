using TMPro;
using UnityEngine.UI;
using Unity.Collections;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;
    public static string JoinCode { get; private set; }

    [SerializeField] GameObject startGameButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button joinButton;
    [SerializeField] TMP_InputField joinInput;
    [SerializeField] TextMeshProUGUI codeText;

    [Header("Lobby UI")]
    [SerializeField] GameObject waitingLobbyPanel;
    [SerializeField] Transform playerListContainer;
    [SerializeField] TextMeshProUGUI playerNamePrefab;

    void Awake() => Instance = this;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.SetActive(true);
        }

        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(joinInput.text));
    }

    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        JoinCode = joinCode;
        codeText.text = "Code: " + joinCode;

        var relayServerData = new RelayServerData(allocation, "dtls");
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        ShowWaitingLobby(joinCode);
    }

    async void JoinRelay(string joinCode)
    {
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        var relayServerData = new RelayServerData(joinAllocation, "dtls");
        JoinCode = joinCode;
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        ShowWaitingLobby(joinCode);
    }

    void ShowWaitingLobby(string joinCode)
    {
        waitingLobbyPanel.SetActive(true);
        if (NetworkManager.Singleton.IsHost)
        {
            startGameButton.SetActive(true);
        }
        codeText.text = "Code: " + joinCode;
    }

    public void UpdatePlayerListUI(NetworkList<FixedString64Bytes> names)
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (var name in names)
        {
            var playerNameText = Instantiate(playerNamePrefab, playerListContainer);
            playerNameText.text = name.ToString();
        }
        Debug.Log(JoinCode);
        codeText.text = "Code: " + JoinCode;

    }

    public void StartGame()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainGameScene", LoadSceneMode.Single);
        }
    }
}