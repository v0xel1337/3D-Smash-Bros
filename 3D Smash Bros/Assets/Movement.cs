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
    private bool isPunching = false;

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

        if (Input.GetKey(KeyCode.W))
            moveInput += new Vector3(transform.forward.x, 0.0f, transform.forward.z);
        if (Input.GetKey(KeyCode.S))
            moveInput -= new Vector3(transform.forward.x, 0.0f, transform.forward.z);

        // Forgás balra/jobbra (A / D)
        if (Input.GetKey(KeyCode.A))
            transform.Rotate(Vector3.up, -rotationSpeed * Time.deltaTime);
        if (Input.GetKey(KeyCode.D))
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.V) && !isPunching)
        {
            animator.SetTrigger("Punch");
            isPunching = true;
        }

        if (isPunching)
        {
            moveInput = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.Q) && !isPunching)
        {
            animator.SetTrigger("Roll");
            isPunching = true;
        }

        if (isPunching && Input.GetKey(KeyCode.Q))
        {
            animator.SetBool("IsRolling", true);
        }
        else
        {
            animator.SetBool("IsRolling", false);
            isPunching = false;
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

    public void EndPunch()
    {
        isPunching = false;
    }

    void FixedUpdate()
    {
        // Apply movement using Rigidbody
        Vector3 targetPosition = rb.position + moveInput * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }
}
