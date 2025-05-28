using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Globalization;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthFillImage;

    private PlayerHealth playerHealth;

    public void SetPlayerHealth(PlayerHealth health)
    {
        playerHealth = health;
    }

    void Update()
    {
        if (playerHealth == null) return;

        float healthPercent = playerHealth.HEALTH.Value / playerHealth.maxHealth;
        healthFillImage.fillAmount = Mathf.Clamp01(healthPercent);
    }
}