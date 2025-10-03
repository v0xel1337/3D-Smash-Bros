using Unity.Netcode;
using UnityEngine;

public class Shockwave : NetworkBehaviour
{
    private Vector3 rTargetScale;
    private Vector3 bigScale = new Vector3(1000f, 1000f, 70f);
    private Vector3 smallScale = new Vector3(15f, 15f, 15f);

    public Transform followTransform;

    public float scaleSpeed = 30f; // mennyit változzon másodpercenként
    public Movement1 movement;

    public override void OnNetworkSpawn()
    {
        rTargetScale = bigScale;
    }

    void Update()
    {
        if (!IsOwner)
            return;
        if (movement.rIsReversed)
        {
            rTargetScale = smallScale;
        }
        else
        {
            rTargetScale = bigScale;
        }
        // skálázás lineárisan
        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            rTargetScale,
            scaleSpeed * Time.deltaTime
        );

        // ha elérte a célt
        if (Vector3.Distance(transform.localScale, rTargetScale) < 0.01f)
        {
            NetworkObject.Despawn(transform);
        }

        // pozíció követés
        transform.position = followTransform.position;
    }

    public void OnTriggerEnter(Collider other)
    {
        PlayerCombat pcTemp = other.GetComponent<PlayerCombat>();
        if (pcTemp != null)
        {
            pcTemp.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
        }
    }
}
