using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public List<PlayerCombat> playersInside = new List<PlayerCombat>();
    public Animator animator;
    public PlayerCombat bulletShooter;


    private void OnTriggerEnter(Collider other)
    {
        // Csak a szerver figyeli az ütközést
        //if (!IsServer) return;

        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();
        if (pcTemp != null || other.CompareTag("Terrain"))
        {
            if (!playersInside.Contains(pcTemp) && pcTemp != bulletShooter)
            {
                playersInside.Add(pcTemp);
            }
            animator.Play("Explosion");
        }
    }

    public void CheckPlayersInside()
    {
        if (playersInside.Count > 0)
        {
            Debug.Log($"Játékosok a triggerben ({playersInside.Count} db):");

            foreach (PlayerCombat enemy in playersInside)
            {
                if (enemy != null)
                {
                    enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
                }
            }
        }
    }

    public void DespawnObject()
    {
        CheckPlayersInside();
        NetworkObject.Despawn(true);
    }
}
