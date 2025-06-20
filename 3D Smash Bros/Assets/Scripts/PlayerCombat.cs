using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombat : NetworkBehaviour
{
    public NetworkVariable<float> percent = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    private Rigidbody rb;
    private TextMeshProUGUI percentText;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            percentText = GameObject.Find("Knockback Multiplier Text")?.GetComponent<TextMeshProUGUI>();

            if (percentText == null)
            {
                Debug.LogWarning("PercentText UI elem nem található!");
            }
            else
            {
                UpdatePercentUI();
            }
        }

        percent.OnValueChanged += OnPercentChanged;
    }

    public void TakeDamage(float amount, Vector3 knockback)
    {
        if (!IsOwner) return;

        percent.Value += amount;

        float totalKnockback = knockback.magnitude + (percent.Value * 0.1f);
        Vector3 finalForce = knockback.normalized * totalKnockback;

        rb.linearVelocity = Vector3.zero; // 'linearVelocity' nem létezik Unity-ben, 'velocity' a helyes
        rb.AddForce(finalForce, ForceMode.VelocityChange);

        UpdatePercentUI();
    }

    private void UpdatePercentUI()
    {
        if (percentText != null)
        {
            percentText.text = Mathf.RoundToInt(percent.Value) + "%";
        }
    }

    private void OnPercentChanged(float oldValue, float newValue)
    {
        UpdatePercentUI();
    }
}
