using UnityEngine;

public class PunchAnimationEvents1 : MonoBehaviour
{
    public Movement1 movementScript;
    public PlayerCombat pc;
    void GiveDamageEvent(string actionID)
    {
        switch (actionID)
        {
            case "Punch":
                movementScript.PerformPunchHit(2);
                break;
            case "CylinderLodged":
                movementScript.CylinderLodged();
                break;
        }
    }

}