using System;
using UnityEditor.ShaderGraph.Internal;
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

    bool isCrouching = false;
    bool isSliding = false;

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
    Vector3 originalSize;

    Rigidbody rb;


	
	void Start()
    {
        //Necesitamos el rigidbody para aplicar las fuerzas, y el tamaño para agacharnos

   		rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        originalSize = GetComponent<Transform>().localScale;
    }

    void Update()
    {
        //Comprueba que debajo del jugador hay suelo, para verificar si podemos saltar y añadir rozamiento
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();

        if (grounded)
        {
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
    }

    //FixedUpdate se llama cada intervalos iguales, asi no se llaman mas o menos veces los métodos dependiendo de los FPS del jugador
    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();

    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal"); // Las teclas asociadas están en:
		verticalInput = Input.GetAxisRaw("Vertical");   // Edit\Project Settings\Input (según el codigo ejemplo del PDF)

        //Salto
		if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); //habria que hacerlo con corutina?
        }

        //Agacharse / Deslizarse
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isCrouching && !isSliding && grounded)
        {
            if(new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z).magnitude < moveSpeed*0.60f)
            {
                Crouch();
            }
            else
            {
                Slide();
            }
        }

        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            if (isCrouching)
            {
                CrouchEnd();
            }
            else if (isSliding)
            {
                SlideEnd();
            }
        }
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();
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

    private void Crouch()
    {
        transform.localScale = new Vector3(originalSize.x, originalSize.y * 60 / 100, originalSize.z);
        maxSpeed = maxSpeed / 2;
        moveSpeed = moveSpeed / 2;
        isCrouching = true;
    }

    private void CrouchEnd()
    {
        transform.localScale = new Vector3(originalSize.x, originalSize.y , originalSize.z);
        maxSpeed = maxSpeed * 2;
        moveSpeed = moveSpeed * 2;
        isCrouching = false;
    }

    private void Slide()
    {

    }

    private void SlideEnd()
    {

    }
}
