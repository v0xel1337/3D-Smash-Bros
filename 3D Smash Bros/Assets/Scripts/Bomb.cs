using UnityEngine;

public class Bomb : MonoBehaviour
{
    [SerializeField] private float damage = 25f;
    [SerializeField] private float baseKnockback = 15f;
    [SerializeField] private float upwardBoost = 5f;

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
            Destroy(gameObject);
        }
    }
}
