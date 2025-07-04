using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    public TextMeshProUGUI knockbackText;
    public Image clickCooldownImage;
    public Image clickGreenCooldownImage;
    public PlayerHealthUI healthUI;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}