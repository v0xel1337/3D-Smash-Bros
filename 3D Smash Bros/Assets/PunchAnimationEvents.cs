using UnityEngine;

public class PunchAnimationEvents : MonoBehaviour
{
    public Movement movementScript;

    public void EndPunch()
    {
        if (movementScript != null)
        {
            movementScript.EndPunch();
        }
    }
}