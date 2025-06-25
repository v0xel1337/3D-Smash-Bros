using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Globalization;
using Unity.Burst.Intrinsics;
using Unity.Multiplayer.Playmode;

public class PlayerHealthUI : MonoBehaviour
{
    public Image healthFillImage;
    public Image[] circles;

    private Movement playerHealth;

    public void SetPlayerHealth(Movement health)
    {
        playerHealth = health;
    }

    public void UpdateCircles(int combo)
    {
        for (int i = 0; i < circles.Length; i++)
        {
            if (i < combo)
                circles[i].gameObject.SetActive(true);
            else
                circles[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        float healthPercent = playerHealth.HEALTH.Value / playerHealth.maxHealth;
        healthFillImage.fillAmount = Mathf.Clamp01(healthPercent);
    }
}