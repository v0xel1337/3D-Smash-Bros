using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    public static GameUI Instance;

    public TextMeshProUGUI knockbackText;
    public Image clickCooldownImage;
    public Image clickGreenCooldownImage;
    public GameObject FightEnd;
    public GameObject GameplayUI;
    public PlayerHealthUI healthUI;
    public GameObject spectatorCamera;
    public Image healthFillImage;
    public Image[] circles;

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