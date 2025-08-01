﻿using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [SerializeField]
    private float defaultSpeed = 5.0f;
    private float currentSpeed = 5.0f;

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
    public Image QcooldownImage;
    public Image EcooldownImage;
    public Image cooldownGreenImage;
    private bool isAbilityOnCooldown = false;
    private bool isQAbilityOnCooldown = false;
    private float cooldownTimer = 0f;
    private float cooldownQTimer = 0f;
    public float abilityCooldownTime = 1f;
    private float greenCooldownTimer = 0f;
    private float greenCooldownTime = 5f;
    private bool isGreenOnCooldown = false;

    public float qDelayAfterUse = 1f;
    private float qTimerAfterUse = 0f;
    public float qCooldownDelay = 10f;
    private float qCooldownTimer = 0f;
    private bool isQUsable = true;
    private bool isQPressed = true;

    public float eCooldownDelay = 4f;
    private float eCooldownTimer = 0f;
    private bool isEUsable = true;

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
        QcooldownImage = GameUI.Instance.QCooldownImage;
        EcooldownImage = GameUI.Instance.ECooldownImage;
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

    private void Start()
    {
        qCooldownTimer = qCooldownDelay;
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
    private float qAbilityInterval = 1f;
    private float qAbilityTimer = 0f;
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (isStunned)
        {
            if (Time.time >= stunEndTime)
            {
                isStunned = false;
                // Esetleg animáció visszaállítása itt
            }
            else
            {
                return; // ne csináljon semmit, ha stunban van
            }
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

        if (Input.GetKeyDown(KeyCode.E) && isEUsable)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                Debug.Log("Hit: " + hit.collider.transform.name);

                if (hit.collider.CompareTag("Stun"))
                {
                    hit.collider.transform.parent.GetComponent<Stun>().PlayStunAnimationServerRpc();
                    Debug.Log("Stun Circle clicked!");
                    eCooldownTimer = 0f;
                    isEUsable = false;
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && !animator.GetBool("inSubStateMachine"))
        {
            animator.SetTrigger("Lay");
        }

        // E cooldown logika
        if (!isEUsable)
        {
            eCooldownTimer += Time.deltaTime;
            EcooldownImage.fillAmount = eCooldownTimer / eCooldownDelay;

            if (eCooldownTimer >= eCooldownDelay)
            {
                isEUsable = true;
                Debug.Log("E ability is ready again!");
            }
        }

        // Ne engedje újraindítani a gurulást, ha még tart
        if (Input.GetKeyDown(KeyCode.Q) && isQUsable && !animator.GetBool("inSubStateMachine"))
        {
            animator.SetTrigger("Roll");
            currentSpeed = defaultSpeed * 4;
            isQPressed = true;
        }

        if (Input.GetKey(KeyCode.Q) && isQUsable && isQPressed)
        {
            if (qCooldownTimer > 0)
            {
                qCooldownTimer -= Time.deltaTime;
                animator.SetBool("IsRolling", true);
            }
            else
            {
                qTimerAfterUse = 0;
                isQUsable = false;
                isQPressed = false;
                animator.SetBool("IsRolling", false);
            }
        }
        else
        {
            if(qCooldownTimer <= qCooldownDelay)
                qCooldownTimer += Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                currentSpeed = 10.0f;
            }
            else
            {
                currentSpeed = defaultSpeed;
            }
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            animator.SetBool("IsRolling", false);
            isQPressed = false;
        }
        QcooldownImage.fillAmount = qCooldownTimer / qCooldownDelay;

        if (!isQUsable)
        {
            qTimerAfterUse += Time.deltaTime;
            if (qTimerAfterUse >= qDelayAfterUse)
            {
                isQUsable = true;
                qTimerAfterUse = 0f;
            }
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
            rb.AddForce(Vector3.up * jumpForce * 10, ForceMode.Impulse);
        } else if (!isGrounded){
		   	rb.AddForce(Vector3.up * -0.1f * jumpForce, ForceMode.Impulse);
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
        Vector3 targetPosition = rb.position + moveInput * currentSpeed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    public void RollDamage()
    {
        foreach (Movement enemy in playersInside)
        {
            if (enemy != null)
            {
                enemy.PlayAnimationOnEnemy(10, 12, transform.position);
                enemy.PlayGetHitAnimation();
                //Debug.Log("TEST: " + enemy.transform.name);
                qAbilityTimer = 0f; // reset timer
            }
        }
    }

    public void LayDamage()
    {
        foreach (Movement enemy in playersInside)
        {
            if (enemy != null)
            {
                enemy.PlayAnimationOnEnemy(10, 12, transform.position);
                enemy.PlayGetHitAnimation();
                //Debug.Log("TEST: " + enemy.transform.name);
            }
        }
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
    public bool isStunned = false;
    private float stunEndTime = 0f;
    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        // Esetleg játssz le egy stun animációt is
        Debug.Log($"{gameObject.name} le lett stunolva {duration} másodpercre.");
    }

    public void PlayAnimationOnEnemy(float amount, float knockbackForce, Vector3 attackerPosition)
    {
        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { OwnerClientId }
            }
        };

        PlayGetHitAnimationClientRpc(rpcParams);

        Vector3 knockbackDirection = (transform.position - attackerPosition).normalized;
        pc.TakeDamage(amount, knockbackDirection * knockbackForce);
        // Ha a szerver (host) futtatja ezt, de nem ő a target, akkor küldjön ClientRpc-t a kliensnek

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

    public void PlayGetHitAnimation()
    {
        animator.SetTrigger("GetHit");
    }
}
