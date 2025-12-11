using System;
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private PlayerSoundManager playerSoundManager;
    [SerializeField] private PlayerCharacter playerCharacter;
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;

    [Header("Movement")]
    public float moveSpeed;
    public float maxSpeed;
    public float groundDrag;
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;

    [Header("Slide")]
    public float slideSpeed;
    public float slideDeceleration;

    bool readyToJump = true;
    bool isCrouching = false;
    bool isSliding = false;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask Ground;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;
    private Vector2 moveInput;
    private bool jumpPressed;

    Vector3 moveDirection;
    Vector3 originalSize;
    Vector3 flatVelocity;

    Rigidbody rb;


    void OnEnable()
    {
        if (playerInput == null) { Debug.LogError("PlayerInput no asignado en PlayerMovement"); return; }

        var map = playerInput.currentActionMap;

        // Enlazamos eventos. Aseg�rate que en tu Input Actions se llamen "Move", "Jump" y "Crouch"
        map["Move"].performed += OnMovePerformed;
        map["Move"].canceled += OnMoveCanceled;

        map["Jump"].started += OnJumpStarted;
        map["Jump"].canceled += OnJumpCanceled;

        map["Crouch"].started += OnCrouchStarted;
        map["Crouch"].canceled += OnCrouchCanceled;
    }

    void OnDisable()
    {
        if (playerInput == null) return;
        var map = playerInput.currentActionMap;

        map["Move"].performed -= OnMovePerformed;
        map["Move"].canceled -= OnMoveCanceled;

        map["Jump"].started -= OnJumpStarted;
        map["Jump"].canceled -= OnJumpCanceled;

        map["Crouch"].started -= OnCrouchStarted;
        map["Crouch"].canceled -= OnCrouchCanceled;
    }

    private void OnCrouchStarted(InputAction.CallbackContext ctx)
    {
        if (!isCrouching && !isSliding && grounded)
        {
            
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            
            if (flatVel.magnitude < moveSpeed * 0.60f)
            {
                Crouch();
            }
            else
            {
                Slide();
            }
        }
    }

    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
    {
        if (isCrouching) CrouchEnd();
        else if (isSliding) SlideEnd();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;

    private void OnJumpStarted(InputAction.CallbackContext ctx) => jumpPressed = true;
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => jumpPressed = false;

    void Start()
    {
        //Necesitamos el rigidbody para aplicar las fuerzas, y el tama�o para agacharnos

   		rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        originalSize = GetComponent<Transform>().localScale;

        
    }

    void Update()
    {
        //Comprueba que debajo del jugador hay suelo, para verificar si podemos saltar y a�adir rozamiento
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, Ground);

        if(playerCharacter.estaVivo && playerCharacter.canMove)
        {
        MyInput();    
        }
        

        if (grounded && !isSliding)
        {
            rb.linearDamping = groundDrag;
        }
        else if (!grounded)
        {
            rb.linearDamping = 0;
        }

    }

    //FixedUpdate se llama cada intervalos iguales, asi no se llaman mas o menos veces los m�todos dependiendo de los FPS del jugador
    private void FixedUpdate()
    {
        if(playerCharacter.estaVivo && playerCharacter.canMove)
        {
        MovePlayer();    
        }
        
        SpeedControl();


    }

    private void MyInput()
    {
        horizontalInput = moveInput.x; // Las teclas asociadas est�n en:
		verticalInput = moveInput.y;   // Edit\Project Settings\Input (seg�n el codigo ejemplo del PDF)

        //Salto
		if (jumpPressed && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown); //habria que hacerlo con corutina?
        }

        
        
    }

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();
        if (OnSlope())
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);
        }
        if (grounded)
        {
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * 10f *playerCharacter.velocidadBase, ForceMode.Force);
        }
        else
        {
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
        flatVelocity = new Vector3(rb.angularVelocity.x, rb.angularVelocity.y, rb.angularVelocity.z);
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
        playerHeight = playerHeight * 60f / 100f;
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        maxSpeed = maxSpeed / 2;
        moveSpeed = moveSpeed / 2;
        isCrouching = true;
    }

    private void CrouchEnd()
    {
        transform.localScale = new Vector3(originalSize.x, originalSize.y , originalSize.z);
        playerHeight = (playerHeight * 100f / 60f);
        maxSpeed = maxSpeed * 2;
        moveSpeed = moveSpeed * 2;
        isCrouching = false;
    }

    private void Slide()
    {
        isSliding = true;
        transform.localScale = new Vector3(originalSize.x, originalSize.y * 60/100, originalSize.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        moveSpeed = moveSpeed * 0.2f;
        maxSpeed = maxSpeed * 10f;
        rb.linearDamping = slideDeceleration;
        rb.AddForce(moveDirection * slideSpeed, ForceMode.Impulse);
        playerSoundManager.playSlideSound();
    }

    private void SlideEnd()
    {
        transform.localScale = originalSize;
        if (grounded)
        {
            rb.angularVelocity = new Vector3(flatVelocity.x * 0.3f, flatVelocity.y, flatVelocity.z * 0.3f);
            rb.linearDamping = groundDrag;
        }
        else
        {
            rb.linearDamping = 0;
        }
        moveSpeed = moveSpeed * 5f;
        maxSpeed = maxSpeed * 0.1f;
        isSliding = false;
    }

    private bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
    }
}
