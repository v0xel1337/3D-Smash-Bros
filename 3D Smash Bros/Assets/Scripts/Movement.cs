using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    

    public static int noOfClicks = 0;


    public Image cooldownImage; // ide húzod be az UI Image-t
    public Image QcooldownImage;
    public Image EcooldownImage;
    public Image cooldownGreenImage;
    private bool isAbilityOnCooldown = false;
    private float cooldownTimer = 0f;
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

    public bool rIsEnabled = false;

    // NetworkVariable szinkronizálja a hálózaton a health értékét




   
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
        pc.ui = GameUI.Instance.healthUI;

        if (pc.ui != null)
            pc.ui.SetPlayerHealth(pc);
    }



    private void Start()
    {
        qCooldownTimer = qCooldownDelay;
    }

    public override void OnNetworkSpawn()
    {
        rb = GetComponent<Rigidbody>();
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
        if (pc.animator.GetBool("inSubStateMachine"))
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
            pc.animator.CrossFade(nextAnimation, 0.15f);
            lastPlayedAnimation = nextAnimation;
        }
    }
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (!pc.animator.GetBool("inSubStateMachine"))
        {
            pc.playersInside.Clear();
        }
        
        if (pc.isStunned)
        {
            if (Time.time >= pc.stunEndTime)
            {
                pc.isStunned = false;
                // Esetleg animáció visszaállítása itt
            }
            else
            {
                return; // ne csináljon semmit, ha stunban van
            }
        }

        AnimatorStateInfo stateInfo = pc.animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer

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



        if (pc.animator.GetBool("inSubStateMachine") && !stateInfo.IsName("Roll"))
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
            pc.animator.SetTrigger("Punch");
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

        if (Input.GetKeyDown(KeyCode.R) && !pc.animator.GetBool("inSubStateMachine"))
        {
            pc.animator.SetTrigger("Lay");
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
        if (Input.GetKeyDown(KeyCode.Q) && isQUsable && !pc.animator.GetBool("inSubStateMachine"))
        {
            pc.animator.SetTrigger("Roll");
            currentSpeed = defaultSpeed * 4;
            isQPressed = true;
        }

        if (Input.GetKey(KeyCode.Q) && isQUsable && isQPressed)
        {
            if (qCooldownTimer > 0)
            {
                qCooldownTimer -= Time.deltaTime;
                pc.animator.SetBool("IsRolling", true);
            }
            else
            {
                qTimerAfterUse = 0;
                isQUsable = false;
                isQPressed = false;
                pc.animator.SetBool("IsRolling", false);
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
            pc.animator.SetBool("IsRolling", false);
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
            pc.ui.UpdateCircles(noOfClicks);
        }
        if (Input.GetMouseButtonDown(0))
        {
            OnClick();
        }

        // ?? Animáció vezérlése
        bool isWalking = moveInput.magnitude > 0.1f;
        bool isRunning = isWalking && Input.GetKey(KeyCode.LeftShift);

        pc.animator.SetBool("IsWalking", isWalking);
        pc.animator.SetBool("IsRunning", isRunning);

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
        foreach (PlayerCombat enemy in pc.playersInside)
        {
            if (enemy != null)
            {
                enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);

            }
        }
    }

    [ServerRpc]
    public void SetREnabledServerRpc(bool value)
    {
        SetREnabledClientRpc(value);
    }

    [ClientRpc]
    void SetREnabledClientRpc(bool value)
    {
        rIsEnabled = value;
    }

    public void LayDamage()
    {
        SetREnabledServerRpc(true);
        foreach (PlayerCombat enemy in pc.playersInside)
        {
            if (enemy != null)
            {
                enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
            }
        }
    }

    public void LayExit()
    {
        rIsEnabled = false;
    }

    public void PerformPunchHit(float punchRange)
    {
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, punchRange))
        {
            var enemy = hit.collider.GetComponent<Movement>();
            if (enemy != null)
            {
                if (enemy.rIsEnabled)
                {
                    enemy.pc.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);

                }
                else
                {
                    enemy.pc.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
                }
            }
        }
    }
    public PlayerCombat pc;
    public void DamageZoneAreaCheck(string actionID)
    {
        if (pc.playersInside.Count == 0)
        {
            noOfClicks = 0;
            isGreenOnCooldown = false;
            greenCooldownTimer = 0f;

            if (cooldownGreenImage != null)
                cooldownGreenImage.fillAmount = 1f;
            if (pc.ui != null)
                pc.ui.UpdateCircles(noOfClicks);
            return;
        }
        
        if (noOfClicks <= 2)
        {
            isGreenOnCooldown = true;
            greenCooldownTimer = 0f;
            noOfClicks++;
            foreach (PlayerCombat enemy in pc.playersInside)
            {
                enemy.PlayGetHitAnimationServerRpc(10, 12, transform.position, OwnerClientId);
            }
            if (noOfClicks > 2)
            {
                isGreenOnCooldown = false;
                greenCooldownTimer = 1f;
                noOfClicks = 0;
                cooldownGreenImage.fillAmount = 1f;
            }

        }

        if (pc.ui != null)
            pc.ui.UpdateCircles(noOfClicks);
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



}
