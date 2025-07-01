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

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance;

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
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        hostButton.onClick.AddListener(CreateRelay);
        joinButton.onClick.AddListener(() => JoinRelay(joinInput.text));
    }

    async void CreateRelay()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
        string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
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
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartClient();

        ShowWaitingLobby(joinCode);
    }

    void ShowWaitingLobby(string joinCode)
    {
        Debug.Log("");
        waitingLobbyPanel.SetActive(true);
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
    }
}