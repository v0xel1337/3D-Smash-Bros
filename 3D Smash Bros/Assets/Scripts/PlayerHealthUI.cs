using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System.Globalization;
using Unity.Burst.Intrinsics;
using Unity.Multiplayer.Playmode;

public class PlayerHealthUI : MonoBehaviour
{
    private Movement playerHealth;

    public void SetPlayerHealth(Movement health)
    {
        playerHealth = health;
    }

    public void UpdateCircles(int combo)
    {
        for (int i = 0; i < GameUI.Instance.circles.Length; i++)
        {
            if (i < combo)
                GameUI.Instance.circles[i].gameObject.SetActive(true);
            else
                GameUI.Instance.circles[i].gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (playerHealth == null) return;

        float healthPercent = playerHealth.HEALTH.Value / playerHealth.maxHealth;
        GameUI.Instance.healthFillImage.fillAmount = Mathf.Clamp01(healthPercent);
    }
}