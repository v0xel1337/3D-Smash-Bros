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


    private float yaw = 0.0f;
    private float pitch = 0.0f;

    private Rigidbody rb;
    private Vector3 moveInput;
    private bool isGrounded = false;
    Camera _camera;
    public Animator animator;
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
		
        // Mouse rotation
        yaw += speedH * Input.GetAxis("Mouse X");
        pitch -= speedV * Input.GetAxis("Mouse Y");

        pitch = Mathf.Clamp(pitch, -30f, 30f);

        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        // Capture movement input relative to current orientation
        moveInput = Vector3.zero;



        if (Input.GetKey(KeyCode.LeftShift) /*&& notSlowed*/){
			speed = 10.0f;
		} else {
			speed = 5.0f;
		}
		
        if (Input.GetKey(KeyCode.W))
            moveInput += new Vector3( transform.forward.x, 0.0f, transform.forward.z);
        if (Input.GetKey(KeyCode.S))
            moveInput -= new Vector3( transform.forward.x, 0.0f, transform.forward.z);
        if (Input.GetKey(KeyCode.D))
            moveInput += transform.right;
        if (Input.GetKey(KeyCode.A))
            moveInput -= transform.right;

        if (Input.GetKeyDown(KeyCode.V))
        {
            animator.SetTrigger("Punch");
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            animator.SetTrigger("Roll");
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
}
