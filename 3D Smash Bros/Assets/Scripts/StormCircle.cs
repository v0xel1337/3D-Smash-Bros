using UnityEngine;

public class StormCircle : MonoBehaviour
{
    [SerializeField] private float shrinkRate = 0.5f;     // Units per second
    [SerializeField] private float minRadius = 5f;
    [SerializeField] private float damagePerSecond = 10f;

    private float currentRadius;

    void Start()
    {
        currentRadius = transform.localScale.x / 2f; // Assuming uniform X/Z scale
    }

    void Update()
    {
        // Shrink the circle over time
        if (currentRadius > minRadius)
        {
            currentRadius -= shrinkRate * Time.deltaTime;
            currentRadius = Mathf.Max(currentRadius, minRadius);
            transform.localScale = new Vector3(currentRadius * 2, transform.localScale.y, currentRadius * 2);
        }

        // Damage players outside the circle
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            float dist = Vector3.Distance(new Vector3(player.transform.position.x, 0, player.transform.position.z),
                                          new Vector3(transform.position.x, 0, transform.position.z));

            if (dist > currentRadius)
            {
                PlayerCombat hp = player.GetComponent<PlayerCombat>();
                if (hp != null)
                {
                    hp.TakeDamage(damagePerSecond * Time.deltaTime);
                }
            } else {
                PlayerCombat hp = player.GetComponent<PlayerCombat>();
                if (hp != null)
                {
                    hp.TakeDamage(-1 * (damagePerSecond * Time.deltaTime));
                }
			}
        }
    }
}
