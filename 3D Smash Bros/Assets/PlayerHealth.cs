using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
	public float CurrentHealth => currentHealth;
	
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
		if (currentHealth > 100){
			currentHealth = 100;
		}
		
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(name + " has died to the storm!");
		Application.Quit();
        Destroy(gameObject); // Or handle respawn/death here
    }
}
