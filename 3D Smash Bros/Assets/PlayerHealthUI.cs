using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;      // Drag reference to PlayerHealth
    public Image healthFillImage;          // Drag HealthBarFill here

    void Update()
    {
        float healthPercent = playerHealth.CurrentHealth / playerHealth.maxHealth;
        healthFillImage.fillAmount = Mathf.Clamp01(healthPercent);
    }
}