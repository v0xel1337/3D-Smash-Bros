using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ability : MonoBehaviour
{
    public Image cooldownImage;
    public float cooldownTime = 5f;
    public KeyCode keyToPress = KeyCode.E;

    private bool isOnCooldown = false;
    private float cooldownTimer = 0f;

    void Update()
    {
        if (Input.GetKeyDown(keyToPress) && !isOnCooldown)
        {
            UseAbility();
        }

        if (isOnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            cooldownImage.fillAmount = cooldownTimer / cooldownTime;

            if (cooldownTimer >= cooldownTime)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
                cooldownImage.fillAmount = 1f;
            }
        }
    }

    void UseAbility()
    {
        isOnCooldown = true;
        cooldownTimer = 0f;
        cooldownImage.fillAmount = 0f;
    }

}
