using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public List<PlayerCombat> playersInside = new List<PlayerCombat>();
    public Animator animator;

    private void OnTriggerEnter(Collider other)
    {
        // Csak a szerver figyeli az ütközést
        if (!IsServer) return;

        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();

        if (pcTemp != null || other.CompareTag("Terrain"))
        {

            if (pcTemp != null)
            {
                playersInside.Add(pcTemp);
            }

            for (int i = 0; i < playersInside.Count; i++)
            {
                Debug.Log(playersInside[i].transform.name);
            }
            animator.Play("Explosion");
        }
    }
}
