using UnityEngine;

public class OrbitCamera : MonoBehaviour
{
    [SerializeField] private Transform target; // A karaktered
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
    [SerializeField] private float sensitivityX = 4f;
    [SerializeField] private float sensitivityY = 2f;
    [SerializeField] private float minY = -20f;
    [SerializeField] private float maxY = 60f;

    private float yaw = 0f;
    private float pitch = 20f;

    void LateUpdate()
    {
        if (target == null) return;

        // Egér bemenet
        yaw += Input.GetAxis("Mouse X") * sensitivityX;
        pitch -= Input.GetAxis("Mouse Y") * sensitivityY;
        pitch = Mathf.Clamp(pitch, minY, maxY);

        // Forgatás kiszámítása
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPosition = target.position + rotation * offset;

        // Kamera mozgatása és forgatása
        transform.position = desiredPosition;
        transform.LookAt(target.position + Vector3.up * 1.5f); // nézz a karakter "feje" felé
    }
}
