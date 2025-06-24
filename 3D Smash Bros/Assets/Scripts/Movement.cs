using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

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
    [SerializeField] private Transform cameraTransform; // A fõkamera Transformja

    public Animator animator;

    public float cooldownTime = 2f;
    private float nextFireTime = 0f;
    public static int noOfClicks = 0;
    float lastClickedTime = 0;
    float maxComboDelay = 1;

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
           
        }
    }

    void OnClick()
    {
        lastClickedTime = Time.time;
        noOfClicks++;
        
        if (noOfClicks == 1)
        {
            animator.SetTrigger("ClubLunge");
        }
        noOfClicks = Mathf.Clamp(noOfClicks, 0, 3);

        if (noOfClicks >= 2 )
        {
            animator.SetTrigger("ClubWide");
        }
        if (noOfClicks >= 3 )
        {
            animator.SetTrigger("ClubGroundSlam");
        }
        Debug.Log(noOfClicks);
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0); // 0 = base layer


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
        if (animator.GetBool("inSubStateMachine"))
        {
            Debug.Log("SubState Machine aktív!");
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
            Debug.Log("Q fel lett engedve, roll vége.");
        }

        if (Time.time - lastClickedTime > maxComboDelay)
        {
            noOfClicks = 0;
        }
        if (Time.time > nextFireTime)
        {
            if (Input.GetMouseButtonDown(0))
            {
                OnClick();
            }
        }

        // ?? Animáció vezérlése
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
            Debug.Log("Ütés eltalált valamit: " + hit.collider.name);

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
        if (playersInside.Count == 0)
        {
            noOfClicks = 0;
            return;
        }
        foreach (Movement enemy in playersInside)
        {
            // Például: sebezd meg õket
            enemy.PlayAnimationOnEnemy(); // feltéve, hogy van ilyen metódus a PlayerCombat scriptben

            Debug.Log("Sebzést kapott egy játékos a triggerben: " + enemy.name);
        }
    }

    private HashSet<Movement> playersInside = new HashSet<Movement>();

    private void OnTriggerEnter(Collider other)
    {
        Movement pc = other.GetComponent<Movement>();
        if (pc != null)
        {
            playersInside.Add(pc);
            Debug.Log("Játékos belépett a mesh triggerbe.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Movement pc = other.GetComponent<Movement>();
        if (pc != null)
        {
            playersInside.Remove(pc);
            Debug.Log("Játékos kilépett a mesh triggerbõl.");
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
        Vector3 knockDir = -transform.forward; // játékos háta mögé

        // Add vertical component
        knockDir.y = 0.2f;
        knockDir.Normalize();
        rb.AddForce(knockDir * 10f, ForceMode.VelocityChange);
    }
    */
}
