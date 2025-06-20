using UnityEngine;

public class PunchAnimationEvents : MonoBehaviour
{
    public Movement movementScript;

    public void GiveDamage()
    {
        movementScript.PerformPunchHit();
    }
}