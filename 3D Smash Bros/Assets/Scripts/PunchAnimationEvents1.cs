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
            case "Lunge":
                movementScript.DamageZoneAreaCheck("Lunge");
                break;
            case "Wide":
                movementScript.DamageZoneAreaCheck("Wide");
                break;
            case "Slam":
                movementScript.DamageZoneAreaCheck("Slam");
                break;
            case "Roll":
                movementScript.RollDamage();
                break;
            case "Lay":
                movementScript.LayDamage();
                break;
        }
    }

}