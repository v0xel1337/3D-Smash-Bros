using Unity.Netcode;
using UnityEngine;

public class Shockwave : NetworkBehaviour
{
    public float rSpawnTimer;
    private Vector3 rTargetScale = new Vector3(100f, 100f, 70f);

    void Update()
    {
        this.transform.localScale = Vector3.Lerp(this.transform.localScale, rTargetScale, Time.deltaTime * 0.5f);
        if (Movement1.rIsReversed)
            rTargetScale = new Vector3(30f, 30f, 20f);
        else
            rTargetScale = new Vector3(100f, 100f, 70f);
    }


}
