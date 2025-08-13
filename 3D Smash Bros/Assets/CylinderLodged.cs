using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CylinderLodged : NetworkBehaviour
{
    public List<PlayerCombat> playersInside = new List<PlayerCombat>();

    private Collider triggerCollider;

    public override void OnNetworkSpawn()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null || !triggerCollider.isTrigger)
        {
            Debug.LogError("Ehhez a scripthez trigger Collider kell!");
            return;
        }

        // Spawn pillanatában keressük meg a bent lévõ játékosokat
        Collider[] hits = Physics.OverlapBox(
            triggerCollider.bounds.center,
            triggerCollider.bounds.extents,
            Quaternion.identity
        );

        foreach (Collider hit in hits)
        {
            PlayerCombat pcTemp = hit.GetComponent<PlayerCombat>();
            if (pcTemp != null && !playersInside.Contains(pcTemp))
            {
                playersInside.Add(pcTemp);
            }
        }

        // Most már van kit sebezni
        CheckPlayersInside();
    }

    void OnTriggerEnter(Collider other)
    {
        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();
        if (pcTemp != null && !playersInside.Contains(pcTemp))
        {
            playersInside.Add(pcTemp);
            // Ha szeretnéd, belépéskor is azonnal sebezhet:
            // CheckPlayersInside();
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
                    enemy.Stun(5f);
                }
            }
        }
    }
}
