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
            Vector3 knockDir = (collision.transform.position - transform.position).normalized;
            knockDir.y = 0.5f;
            knockDir.Normalize();

            PlayerCombat pc = collision.gameObject.GetComponent<PlayerCombat>();
            if (pc != null)
            {
                pc.TakeDamage(damage, knockDir * baseKnockback);
            }

            RequestDespawnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]

    private void RequestDespawnServerRpc()
    {
        NetworkObject.Despawn();
    }
}
