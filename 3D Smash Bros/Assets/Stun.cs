using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Stun : NetworkBehaviour
{
    private List<GameObject> playersInside = new List<GameObject>();
    public Animator animator;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playersInside.Contains(other.gameObject))
        {
            playersInside.Add(other.gameObject);
            Debug.Log($"{other.name} belépett a triggerbe.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playersInside.Contains(other.gameObject))
        {
            playersInside.Remove(other.gameObject);
            Debug.Log($"{other.name} kilépett a triggerbõl.");
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

    [ServerRpc(RequireOwnership = false)]
    public void CheckPlayersInsideServerRpc()
    {
        if (playersInside.Count == 0)
        {
            Debug.Log("Nincs játékos a triggeren belül.");
        }
        else
        {
            Debug.Log($"Játékosok a triggerben ({playersInside.Count} db):");
            foreach (GameObject player in playersInside)
            {
                Debug.Log($"- {player.name}");
            }
        }
    }
}
