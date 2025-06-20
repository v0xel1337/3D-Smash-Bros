using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Movement : NetworkBehaviour
{
    [SerializeField]
    private float speed = 5.0f;

    [SerializeField]
    private float speedH = 2.0f;
    [SerializeField]
    private float speedV = 2.0f;

   	[SerializeField] private float jumpForce = 700.0f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 1.1f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded = false;
    Camera _camera;
    public Animator animator;

    [SerializeField]
    private float rotationSpeed = 300.0f; // új: forgási sebesség
    public override void OnNetworkSpawn()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb = GetComponent<Rigidbody>();
        _camera = GetComponentInChildren<Camera>();

        if (!IsOwner)
        {
            _camera.enabled = false;
           
        }
    }
	
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

		if(rb.linearVelocity.magnitude < 2f){
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

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Punch") && stateInfo.normalizedTime < 1.0f)
        {
            // Ne csináljon semmit
            moveInput = Vector3.zero;
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
                moveInput += new Vector3(transform.forward.x, 0.0f, transform.forward.z);
            if (Input.GetKey(KeyCode.S))
                moveInput -= new Vector3(transform.forward.x, 0.0f, transform.forward.z);

            // Forgás balra/jobbra (A / D)
            if (Input.GetKey(KeyCode.A))
                transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
            if (Input.GetKey(KeyCode.D))
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }



        if (Input.GetKeyDown(KeyCode.V))
        {
            if (!(stateInfo.IsName("Punch") && stateInfo.normalizedTime < 1.0f))
            {
                animator.SetTrigger("Punch");
            }
        }

        // Ne engedje újraindítani a gurulást, ha még tart
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!(stateInfo.IsName("Roll") && stateInfo.normalizedTime < 1.0f))
            {
                animator.SetTrigger("Roll");
            }
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
    }

    void FixedUpdate()
    {
        // Apply movement using Rigidbody
        Vector3 targetPosition = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }

    public void PerformPunchHit()
    {
        float punchRange = 2.0f; // Milyen messzire ér az ütés
        float punchDamage = 10f;

        if (Physics.Raycast(transform.position + Vector3.up, transform.forward, out RaycastHit hit, punchRange))
        {
            Debug.Log("Ütés eltalált valamit: " + hit.collider.name);

            // Ha van rajta valami "damageable" script
            var enemy = hit.collider.GetComponent<Movement>();
            if (enemy != null)
            {
                enemy.TakeDamage(punchDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        animator.SetTrigger("GetHit");
        Vector3 knockDir = -transform.forward; // játékos háta mögé

        // Add vertical component
        knockDir.y = 0.5f;
        knockDir.Normalize();
        rb.AddForce(knockDir * 10f, ForceMode.VelocityChange);
    }

    private void TakeDamageRepeat()
    {
        TakeDamage(1); // így az alapértelmezett 1f érték fog futni
    }

    public void Start() // TEST
    {
        //InvokeRepeating("TakeDamageRepeat", 3f, 3f);
    }
}
