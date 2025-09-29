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
        followTransform = GameObject.FindGameObjectWithTag("ShockwaveSpawn").transform;
        rTargetScale = bigScale;
    }

    void Update()
    {
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
}
