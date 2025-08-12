using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class Stun : NetworkBehaviour
{
    public List<PlayerCombat> playersInside = new List<PlayerCombat>();
    public Animator animator;

    void OnTriggerEnter(Collider other)
    {
        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();
        if (pcTemp != null)
        {
            playersInside.Add(pcTemp);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();
        if (pcTemp != null && playersInside.Contains(pcTemp))
        {
            playersInside.Remove(pcTemp);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void PlayStunAnimationServerRpc()
    {
        // Szerveren is lejátszhatod, ha szükséges:
        animator.Play("Stun");

        // Majd küldd el minden kliensnek
        PlayStunAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayStunAnimationClientRpc()
    {
        // Ne játszd le duplán a szerveren
        if (!IsServer)
        {
            animator.Play("Stun");
        }
    } 

    public void CheckPlayersInside()
    {
        if (playersInside.Count == 0)
        {
            Debug.Log("Nincs játékos a triggeren belül.");
        }
        else
        {
            Debug.Log($"Játékosok a triggerben ({playersInside.Count} db):");

            foreach (PlayerCombat enemy in playersInside)
            {
                if (enemy != null)
                {
                    enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
                    enemy.Stun(5f);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]

    private void RequestDespawnServerRpc()
    {
        NetworkObject.Despawn();
    }
}
