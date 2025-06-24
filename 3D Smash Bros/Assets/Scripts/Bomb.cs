using Unity.Netcode;
using UnityEngine;

public class Bomb : NetworkBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float baseKnockback = 15f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Get direction from bomb to player
            Vector3 knockDir = (collision.transform.position - transform.position).normalized;

            // Add vertical component
            knockDir.y = 0.5f;
            knockDir.Normalize();

            // Apply knockback and damage
            PlayerCombat pc = collision.gameObject.GetComponent<PlayerCombat>();
            if (pc != null)
            {
                pc.TakeDamage(damage, knockDir * baseKnockback);
            }

            // Optionally destroy the bomb after hitting
            NetworkObject.Despawn();
        }
    }
}
