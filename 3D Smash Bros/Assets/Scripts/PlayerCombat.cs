using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Netcode;

public class PlayerCombat : NetworkBehaviour
{
    public NetworkVariable<float> percent = new NetworkVariable<float>(
        0f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner // ✔️ Ez engedi a szervernek módosítani
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
            percent.Value = 0;
            StartCoroutine(WaitForUIReferences());
        }

        percent.OnValueChanged += OnPercentChanged;
    }

    private IEnumerator WaitForUIReferences()
    {
        while (GameUI.Instance == null)
        {
            yield return null;
        }

        percentText = GameUI.Instance.knockbackText;

        UpdatePercentUI();
    }

    public void TakeDamage(float amount, Vector3 knockback)
    {
        if (!IsOwner) return;

        Debug.Log(percent.Value);
        Debug.Log(amount);
        percent.Value += amount;

        float totalKnockback = knockback.magnitude + (percent.Value * 0.1f);
        Vector3 finalForce = knockback.normalized * totalKnockback;

        rb.linearVelocity = Vector3.zero;
        rb.AddForce(finalForce, ForceMode.VelocityChange);

        UpdatePercentUI(); // ha akarod, ClientRpc-ként is lehet később UI frissítés
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
