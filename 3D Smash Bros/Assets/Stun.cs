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
            Debug.Log($"{other.name} bel�pett a triggerbe.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && playersInside.Contains(other.gameObject))
        {
            playersInside.Remove(other.gameObject);
            Debug.Log($"{other.name} kil�pett a triggerb�l.");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayStunAnimationServerRpc()
    {
        // Szerveren is lej�tszhatod, ha sz�ks�ges:
        animator.Play("Stun");

        // Majd k�ldd el minden kliensnek
        PlayStunAnimationClientRpc();
    }

    [ClientRpc]
    private void PlayStunAnimationClientRpc()
    {
        // Ne j�tszd le dupl�n a szerveren
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
            Debug.Log("Nincs j�t�kos a triggeren bel�l.");
        }
        else
        {
            Debug.Log($"J�t�kosok a triggerben ({playersInside.Count} db):");
            foreach (GameObject player in playersInside)
            {
                Debug.Log($"- {player.name}");
            }
        }
    }
}
