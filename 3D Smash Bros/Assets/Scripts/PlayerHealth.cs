using Unity.Netcode;
using UnityEngine;

public class PlayerHealth : NetworkBehaviour
{
    /*
    public float maxHealth = 100f;

    // NetworkVariable szinkroniz�lja a h�l�zaton a health �rt�k�t
    public NetworkVariable<float> HEALTH = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var ui = FindObjectOfType<PlayerHealthUI>();
            if (ui != null)
            {
                ui.SetPlayerHealth(this);
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsOwner) return;

        HEALTH.Value -= amount;
        if (HEALTH.Value > maxHealth)
        {
            HEALTH.Value = maxHealth;
        }

        if (HEALTH.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(name + " has died to the storm!");
        // Ne haszn�ld Application.Quit multiplayerben!
        Destroy(gameObject);
    }
    */
}