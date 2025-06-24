using UnityEngine;

public class PunchAnimationEvents : MonoBehaviour
{
    public Movement movementScript;
    public PlayerCombat pc;
    void GiveDamageEvent(string actionID)
    {
        switch (actionID)
        {
            case "Punch":
                movementScript.PerformPunchHit(2);
                break;
            case "Lunge":
                movementScript.DamageZoneAreaCheck();
                break;
            case "Wide":
                movementScript.DamageZoneAreaCheck();
                break;
            case "Slam":
                movementScript.DamageZoneAreaCheck();
                break;
        }
    }

    void TakeDamageEvent(string actionID)
    {
        switch (actionID)
        {
            case "Punch":
                pc.TakeDamage(10, -transform.forward.normalized * 12);
                break;
            case "HitWeak":
                pc.TakeDamage(10, -transform.forward.normalized * 12);
                break;
        }
    }

}