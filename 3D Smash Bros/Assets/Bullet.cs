using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Bullet : NetworkBehaviour
{
    public List<PlayerCombat> playersInside = new List<PlayerCombat>();
    public GameObject ExplosionCollider;

    void OnTriggerEnter(Collider other)
    {
        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();
        
        if (pcTemp || other.gameObject.tag == "Terrain")
        {
            ExplosionCollider.SetActive(true);
            if (pcTemp != null)
            {
                playersInside.Add(pcTemp);
            }
            for (int i = 0; i < playersInside.Count; i++)
            {
                Debug.Log(playersInside[i].transform.name);
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
