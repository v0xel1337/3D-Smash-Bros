using UnityEngine;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    public static int SelectedCharacter = 0; // 0 = Character1, 1 = Character2

    [SerializeField] private Button char1Button;
    [SerializeField] private Button char2Button;

    void Start()
    {
        char1Button.onClick.AddListener(() => SelectCharacter(0));
        char2Button.onClick.AddListener(() => SelectCharacter(1));
    }

    void SelectCharacter(int characterIndex)
    {
        SelectedCharacter = characterIndex;
    }
}
