using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("Movement")]
    public float moveSpeed;
    public float maxSpeed;
	
    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump = true;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;


	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
   		rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {

        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        //Debug.Log("Speed " + rb.linearVelocity.magnitude);

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();

        //Debug.Log("Speed is " + new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude);
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // Las teclas asociadas están en:
		verticalInput = Input.GetAxisRaw("Vertical");   // Edit\Project Settings\Input (según el codigo ejemplo del PDF)

		if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); //habria que hacerlo con corutina?
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (grounded)
        {
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * 10f, ForceMode.Force);
        }
        else
        {
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            //Debug.Log("Speed is " + new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

}
