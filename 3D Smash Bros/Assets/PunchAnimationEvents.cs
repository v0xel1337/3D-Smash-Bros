using UnityEngine;

public class PunchAnimationEvents : MonoBehaviour
{
    public Movement movementScript;

    public void EndPunch()
    {
        movementScript.EndPunch();
    }

    public void GiveDamage()
    {
        movementScript.PerformPunchHit();
    }
}