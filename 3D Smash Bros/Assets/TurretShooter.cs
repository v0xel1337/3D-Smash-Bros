using UnityEngine;

public class TurretShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float shootInterval = 2f;
    private float timer = 0f;
	
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= shootInterval)
        {
            Fire();
            timer = 0f;
        }
    }

    void Fire()
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = proj.GetComponent<Rigidbody>();
        rb.linearVelocity = firePoint.forward * 10f;
    }
}
