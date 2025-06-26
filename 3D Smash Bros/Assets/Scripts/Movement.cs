using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5.0f;

   	[SerializeField] private float jumpForce = 700.0f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded = false;
    Camera _camera;
    [SerializeField] private Transform cameraTransform; // A f�kamera Transformja

    public Animator animator;

    public static int noOfClicks = 0;
    PlayerHealthUI ui;
    public float maxHealth = 100f;


    public Image cooldownImage; // ide h�zod be az UI Image-t
    private bool isAbilityOnCooldown = false;
    private float cooldownTimer = 0f;
    public float abilityCooldownTime = 5f;



    // NetworkVariable szinkroniz�lja a h�l�zaton a health �rt�k�t
    public NetworkVariable<float> HEALTH = new NetworkVariable<float>(
        100f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public void TakeDamage(float amount)
    {
        if (!IsOwner) return;

        HEALTH.Value -= amount;
        if (HEALTH.Value > maxHealth)
        {
            HEALTH.Value = maxHealth;
        }

        if (HEALTH.Value <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(name + " has died to the storm!");
        // Ne haszn�ld Application.Quit multiplayerben!
        Destroy(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        _camera = GetComponentInChildren<Camera>();
        cameraTransform = Camera.main.transform;

        if (!IsOwner)
        {
            _camera.enabled = false;
            return;
        }
        // Keresd meg az UI-t a jelenetb�l (csak a saj�todn�l)
        GameObject uiObj = GameObject.Find("ClickCooldown");
        if (uiObj != null)
        {
            cooldownImage = uiObj.GetComponent<Image>();
        }

        // Keresd meg a UI-t a jelenetben
        ui = FindObjectOfType<PlayerHealthUI>();
        if (ui != null)
        {
            ui.SetPlayerHealth(this);
        }
        else
        {
            Debug.LogWarning("PlayerHealthUI nem tal�lhat� a jelenetben.");
        }
    }
    private string lastPlayedAnimation = "";
    void ActivateAbility()
    {
        isAbilityOnCooldown = true;
        cooldownTimer = 0f;

        if (cooldownImage != null)
            cooldownImage.fillAmount = 0f;

        Debug.Log("Ability aktiv�lva!");
    }
    void OnClick()
    {
        

        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);

        string nextAnimation = "";

        if (!isAbilityOnCooldown)
        {
            ActivateAbility();
            if (noOfClicks == 0) nextAnimation = "Club Attack Lunge";
        }
        
        else if (noOfClicks == 1) nextAnimation = "Club Attack Wide";
        else if (noOfClicks == 2) nextAnimation = "Club Attack Ground Slam";

        // Ne ind�tsuk �jra ugyanazt az anim�ci�t, ha az m�r volt
        if (nextAnimation != "" && lastPlayedAnimation != nextAnimation)
        {
            animator.CrossFade(nextAnimation, 0.15f);
            lastPlayedAnimation = nextAnimation;
        }

        Debug.Log("Klikksz�m: " + noOfClicks);
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

            // Csak v�zszintes komponensek
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
        if (animator.GetBool("inSubStateMachine"))
        {
            Debug.Log("SubState Machine akt�v!");
        }


        if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetTrigger("Punch");
        }

        // Ne engedje �jraind�tani a gurul�st, ha m�g tart
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
            Debug.Log("Q fel lett engedve, roll v�ge.");
        }

        if (!isAbilityOnCooldown)
        {
            noOfClicks = 0;
            lastPlayedAnimation = ""; // anim�ci� reset
            ui.UpdateCircles(noOfClicks);
        }
        if (Input.GetMouseButtonDown(0))
        {
            OnClick();
        }

        // ?? Anim�ci� vez�rl�se
        Debug.Log(moveInput.magnitude);
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
        Vector3 targetPosition = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }



    public void PerformPunchHit(float punchRange)
    {
        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, punchRange))
        {
            Debug.Log("�t�s eltal�lt valamit: " + hit.collider.name);

            // Ha van rajta valami "damageable" script
            var enemy = hit.collider.GetComponent<Movement>();
            if (enemy != null)
            {
                enemy.PlayAnimationOnEnemy();
            }
        }
    }

    public void DamageZoneAreaCheck()
    {
        if (!IsOwner)
            return;

        if (playersInside.Count == 0)
        {
            noOfClicks = 0;
            if (ui != null)
                ui.UpdateCircles(noOfClicks);
            return;
        }

        noOfClicks++;
        if (ui != null)
            ui.UpdateCircles(noOfClicks);

        foreach (Movement enemy in playersInside)
        {
            enemy.PlayAnimationOnEnemy();
            Debug.Log("Sebz�st kapott egy j�t�kos a triggerben: " + enemy.name);
        }

        playersInside.Clear();

    }

    public List<Movement> playersInside = new List<Movement>();

    private void OnTriggerEnter(Collider other)
    {
        if (!IsOwner) return;

        Movement pc = other.GetComponent<Movement>();
        if (pc != null)
        {
            playersInside.Add(pc);
            Debug.Log("J�t�kos bel�pett a mesh triggerbe.");
        }
    }

    public bool IsAnyPlayerInside()
    {
        return playersInside.Count > 0;
    }

    public void PlayAnimationOnEnemy()
    {
        animator.SetTrigger("GetHit");
    }

    /*
    public void TakeDamageKnock() // ANIMATION EVENT
    {
        Vector3 knockDir = -transform.forward; // j�t�kos h�ta m�g�

        // Add vertical component
        knockDir.y = 0.2f;
        knockDir.Normalize();
        rb.AddForce(knockDir * 10f, ForceMode.VelocityChange);
    }
    */
}
