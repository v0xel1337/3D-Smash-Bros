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
    public NetworkVariable<float> HEALTH = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );  // ÁTRAKNI
    Camera _camera;

    public PlayerHealthUI ui; // ÁTRAKNI
    public float maxHealth = 100f; // ÁTRAKNI

    public bool isDead = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    public override void OnNetworkSpawn()
    {
        _camera = GetComponentInChildren<Camera>();

        if (IsOwner)
        {
            percent.Value = 0;
            StartCoroutine(WaitForUIReferences());
        }

        percent.OnValueChanged += OnPercentChanged;
    }
    public void TakeDamage(float amount)
    {
        if (!IsOwner || isDead) return;

        HEALTH.Value -= amount;
        if (HEALTH.Value > maxHealth)
        {
            HEALTH.Value = maxHealth;
        }

        if (HEALTH.Value <= 0)
        {
            Die();
            isDead = true;
        }
    }

    void Die()
    {
        if (!IsOwner) return;

        isDead = true;

        if (IsServer)
        {
            GameManager.Instance.UnregisterPlayer(OwnerClientId);
        }
        else
        {
            UnregisterPlayerServerRpc();
        }

        GameUI.Instance.GameplayUI.SetActive(false);
        GameUI.Instance.loseObject.SetActive(true);
        _camera.gameObject.SetActive(false);
        GameUI.Instance.spectatorCamera.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameUI.Instance.FightEnd.SetActive(true);

        RequestDespawnServerRpc();
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestDespawnServerRpc()
    {
        NetworkObject.Despawn();
    }
    [ServerRpc]
    public void UnregisterPlayerServerRpc()
    {
        GameManager.Instance.UnregisterPlayer(OwnerClientId);
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

        Debug.Log("dfgdfgdfgdfg");
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
