using UnityEngine;

public class Projectile : MonoBehaviour
{
	[SerializeField]
	private float multiplier = 1.0f;
	
    void OnCollisionEnter(Collision other)
    {
        PlayerDamage player = other.collider.GetComponent<PlayerDamage>();
        if (player)
        {
            Vector3 knockbackDir = (other.transform.position - transform.position).normalized;
            player.TakeHit(10f, knockbackDir, baseKnockback: 2f * multiplier, scaling: 0.1f, upwardBoost: 2.0f);
			Destroy(gameObject);
        } else if (other.gameObject.tag == "Terrain"){
			Destroy(gameObject);
		}
    }
}
