﻿using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5.0f;

   	[SerializeField] private float jumpForce = 7;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded = false;
    Camera _camera;
    [SerializeField] private Transform cameraTransform; // A főkamera Transformja

    public Animator animator;

    public static int noOfClicks = 0;
    PlayerHealthUI ui;
    public float maxHealth = 100f;


    public Image cooldownImage; // ide húzod be az UI Image-t
    public Image cooldownGreenImage;
    private bool isAbilityOnCooldown = false;
    private float cooldownTimer = 0f;
    public float abilityCooldownTime = 1f;
    private float greenCooldownTimer = 0f;
    private float greenCooldownTime = 5f;
    private bool isGreenOnCooldown = false;


    // NetworkVariable szinkronizálja a hálózaton a health értékét
    public NetworkVariable<float> HEALTH = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );
    public bool isDead = false;
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
    [ServerRpc]
    public void UnregisterPlayerServerRpc()
    {
        GameManager.Instance.UnregisterPlayer(OwnerClientId);
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
    private IEnumerator WaitForGameUI()
    {
        // Várd meg, amíg a GameUI.Instance nem null
        while (GameUI.Instance == null)
        {
            yield return null;
        }

        cooldownImage = GameUI.Instance.clickCooldownImage;
        cooldownGreenImage = GameUI.Instance.clickGreenCooldownImage;
        ui = GameUI.Instance.healthUI;

        if (ui != null)
            ui.SetPlayerHealth(this);
    }

    [ServerRpc(RequireOwnership = false)]

    private void RequestDespawnServerRpc()
    {
        NetworkObject.Despawn();
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        pc = GetComponent<PlayerCombat>();
        _camera = GetComponentInChildren<Camera>();
        cameraTransform = _camera.transform;

        if (IsServer)
        {
            GameManager.Instance.RegisterPlayer(OwnerClientId);
        }

        if (!IsOwner)
        {
            _camera.enabled = false;
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        StartCoroutine(WaitForGameUI());
    }

    private string lastPlayedAnimation = "";
    void ActivateAbility()
    {
        isAbilityOnCooldown = true;
        
        cooldownTimer = 0f;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

    }
    void OnClick()
    {
        if (animator.GetBool("inSubStateMachine"))
            return;
        string nextAnimation = "";
        if (!isAbilityOnCooldown && !isGreenOnCooldown)
        {
            ActivateAbility();
            if (noOfClicks == 0)
            {
                nextAnimation = "Club Attack Lunge";
            }
        }
        else if (noOfClicks == 1)
        {
            nextAnimation = "Club Attack Wide";
            FaceCameraDirection(); // Rotate before attacking
        }
        else if (noOfClicks == 2)
        {
            nextAnimation = "Club Attack Ground Slam";
            FaceCameraDirection(); // Rotate before attacking
        }

        // Avoid replaying the same animation
        if (nextAnimation != "" && lastPlayedAnimation != nextAnimation)
        {
            animator.CrossFade(nextAnimation, 0.15f);
            lastPlayedAnimation = nextAnimation;
        }
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer

        if (isAbilityOnCooldown)
        {
            cooldownTimer += Time.deltaTime;
            float fill = cooldownTimer / abilityCooldownTime;
            if (cooldownImage != null)
                cooldownImage.fillAmount = fill;

            if (cooldownTimer >= abilityCooldownTime)
            {
                isAbilityOnCooldown = false;
                cooldownTimer = 0f;

                if (cooldownImage != null)
                    cooldownImage.fillAmount = 1f;
            }
        }

        if (isGreenOnCooldown)
        {
            greenCooldownTimer += Time.deltaTime;
            float fill = greenCooldownTimer / greenCooldownTime;
            if (cooldownGreenImage != null)
                cooldownGreenImage.fillAmount = 1f - fill;

            if (greenCooldownTimer >= greenCooldownTime)
            {
                isGreenOnCooldown = false;
                greenCooldownTimer = 0f;

                if (cooldownGreenImage != null)
                    cooldownGreenImage.fillAmount = 1f;
            }
        }

        if (rb.linearVelocity.magnitude < 2f){
			rb.linearVelocity = Vector3.zero;
		}
		
		if(Input.GetKeyDown(KeyCode.Escape)){
			Application.Quit();	
		}
		

        // Capture movement input relative to current orientation
        moveInput = Vector3.zero;



        if (Input.GetKey(KeyCode.LeftShift) /*&& notSlowed*/){
			speed = 10.0f;
		} else {
			speed = 5.0f;
		}


        if (animator.GetBool("inSubStateMachine") && !stateInfo.IsName("Roll"))
        {
            moveInput = Vector3.zero;
        }
        else
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            // Csak vízszintes komponensek
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            moveInput = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) moveInput += forward;
            if (Input.GetKey(KeyCode.S)) moveInput -= forward;
            if (Input.GetKey(KeyCode.D)) moveInput += right;
            if (Input.GetKey(KeyCode.A)) moveInput -= right;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetTrigger("Punch");
        }

        // Ne engedje újraindítani a gurulást, ha még tart
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("Roll");
        }

        if (Input.GetKey(KeyCode.Q))
        {
            animator.SetBool("IsRolling", true);
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            animator.SetBool("IsRolling", false);
        }

        if (!isGreenOnCooldown)
        {
            noOfClicks = 0;
            lastPlayedAnimation = ""; // animáció reset
            ui.UpdateCircles(noOfClicks);
        }
        if (Input.GetMouseButtonDown(0))
        {
            OnClick();
        }

        // ?? Animáció vezérlése
        bool isWalking = moveInput.magnitude > 0.1f;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);

        animator.SetBool("IsWalking", isWalking);
        animator.SetBool("IsRunning", isRunning);

        // Grounded check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundMask);

        // Jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // reset vertical velocity
            rb.AddForce(Vector3.up * 70, ForceMode.Impulse);
        } else if (!isGrounded){
		   	rb.AddForce(Vector3.up * -0.1f * 15, ForceMode.Impulse);
		}
		
        moveInput = moveInput.normalized;

        if (moveInput.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveInput);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }
    void FixedUpdate()
    {
        // Apply movement using Rigidbody
        Vector3 targetPosition = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }



    public void PerformPunchHit(float punchRange)
    {
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, punchRange))
        {
            var enemy = hit.collider.GetComponent<Movement>();
            if (enemy != null)
            {
                enemy.PlayAnimationOnEnemy(10, 12, transform.position);
            }
        }
    }
    public PlayerCombat pc;
    public void DamageZoneAreaCheck(string actionID)
    {
        if (playersInside.Count == 0)
        {
            noOfClicks = 0;
            isGreenOnCooldown = false;
            greenCooldownTimer = 0f;

            if (cooldownGreenImage != null)
                cooldownGreenImage.fillAmount = 1f;
            if (ui != null)
                ui.UpdateCircles(noOfClicks);

            playersInside.Clear();
            return;
        }
        
        if (noOfClicks < 2)
        {
            isGreenOnCooldown = true;
            greenCooldownTimer = 0f;
            noOfClicks++;
            foreach (Movement enemy in playersInside)
            {
                enemy.PlayAnimationOnEnemy(10, 12, transform.position);
            }

        }
        else
        {
            isGreenOnCooldown = false;
            greenCooldownTimer = 1f;
            noOfClicks = 0;
            cooldownGreenImage.fillAmount = 1f;
        }

        playersInside.Clear();

        if (ui != null)
            ui.UpdateCircles(noOfClicks);
    }

    public List<Movement> playersInside = new List<Movement>();

    private void OnTriggerEnter(Collider other)
    {

        Movement pcTemp = other.GetComponent<Movement>();
        if (pcTemp != null)
        {
            playersInside.Add(pcTemp);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Movement pcTemp = other.GetComponent<Movement>();
        if (pcTemp != null && playersInside.Contains(pcTemp))
        {
            playersInside.Remove(pcTemp);
        }
    }

    public bool IsAnyPlayerInside()
    {
        return playersInside.Count > 0;
    }

    public void PlayAnimationOnEnemy(float amount, float knockbackForce, Vector3 attackerPosition)
    {
        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        pc.TakeDamage(amount, knockbackDirection * knockbackForce);
        // Ha a szerver (host) futtatja ezt, de nem ő a target, akkor küldjön ClientRpc-t a kliensnek
        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        PlayGetHitAnimationClientRpc(rpcParams);
    }

    void FaceCameraDirection()
    {
        Vector3 cameraForward = cameraTransform.forward;
        cameraForward.y = 0f; // Keep only horizontal rotation
        if (cameraForward.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
            transform.rotation = targetRotation;
        }
    }


    /*
    public void TakeDamageKnock() // ANIMATION EVENT
    {
        Vector3 knockDir = -transform.forward; // játékos háta mögé

        // Add vertical component
        knockDir.y = 0.2f;
        knockDir.Normalize();
        rb.AddForce(knockDir * 10f, ForceMode.VelocityChange);
    }
    */

    [ClientRpc]
    void PlayGetHitAnimationClientRpc(ClientRpcParams rpcParams = default)
    {
        if (!IsOwner) return; // csak a célzott kliens játssza le
        animator.SetTrigger("GetHit");
    }
}
