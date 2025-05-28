using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public float percent = 0f;
    private Rigidbody rb;
    [SerializeField] private TextMeshProUGUI percentText; // H?zd be az Inspectorban

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        UpdatePercentUI();
    }

    public void TakeDamage(float amount, Vector3 knockback)
    {
        percent += amount;

        // Scale knockback by damage percentage
        float totalKnockback = knockback.magnitude + (percent * 0.1f);
        Vector3 finalForce = knockback.normalized * totalKnockback;

        // Apply knockback
        rb.linearVelocity = Vector3.zero; // Reset for consistency
        rb.AddForce(finalForce, ForceMode.VelocityChange);

        UpdatePercentUI();
    }

    private void UpdatePercentUI()
    {
        Debug.Log(percent);
        percentText.text = Mathf.RoundToInt(percent) + "%";
    }
}
