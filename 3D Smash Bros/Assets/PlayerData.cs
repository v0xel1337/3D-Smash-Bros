using Unity.Netcode;

public class PlayerData : NetworkBehaviour
{
    public NetworkVariable<int> CharacterIndex = new NetworkVariable<int>(
        0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // Ezt a lok�lis j�t�kos �rja be, amikor bel�p
            CharacterIndex.Value = CharacterSelector.SelectedCharacter;
        }
    }
}
