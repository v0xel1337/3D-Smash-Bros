using TMPro;
using UnityEngine;

public class PlayerDamage : MonoBehaviour
{
    public float damagePercent = 0f;
    public Rigidbody rb;
	private float velocityThreshold = 0.5f; // Threshold for stopping motion

    [Header("Knockback Settings")]
    public float knockbackMultiplier = 1.5f;


    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
    }

    public void TakeHit(float damage, Vector3 sourcePosition, float baseKnockback = 0f, float scaling = 0f, float upwardBoost = 1.0f)
	{
	    damagePercent += damage;
	
	    // Knockback force based on base + scaling * %damage
	    float knockbackForce = baseKnockback + (scaling * damagePercent);
	
	    // Direction from attacker to player, flat (XZ only)
	    Vector3 horizontalDir = (transform.position - sourcePosition);
	    horizontalDir.y = 0;
	    horizontalDir.Normalize();
	
	    // Add upward direction
	    Vector3 finalKnockback = horizontalDir + Vector3.up * upwardBoost;
	    finalKnockback.Normalize(); // Keep direction unit-length
	
	    // Apply force
	    rb.AddForce(finalKnockback * knockbackForce * knockbackMultiplier, ForceMode.Impulse);
 		rb.AddForce(finalKnockback * knockbackForce * knockbackMultiplier, ForceMode.Impulse);
    }

    public void ResetDamage()
    {
        damagePercent = 0f;
    }
	
	void FixedUpdate(){
		if (rb.linearVelocity.magnitude < velocityThreshold)
		    {
			    rb.linearVelocity = Vector3.zero;
			}
		}

}
