using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
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

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;

    public Transform orientation;

    [SerializeField] private PlayerInput playerInput;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool crouchHeld; // SE mantiene mientras esté pulsado

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 originalSize;
    Vector3 flatVelocity;

    Rigidbody rb;

    /* ----------------------------------------------------------
                     REGISTRO INPUT PROPER
    ---------------------------------------------------------- */

    void OnEnable()
    {
        var map = playerInput.currentActionMap;

        map["Move"].performed += OnMovePerformed;
        map["Move"].canceled += OnMoveCanceled;

        map["Jump"].started += OnJumpStarted;
        map["Jump"].canceled += OnJumpCanceled;

        map["Crouch"].started += OnCrouchStarted;
        map["Crouch"].canceled += OnCrouchCanceled;
    }

    void OnDisable()
    {
        var map = playerInput.currentActionMap;

        map["Move"].performed -= OnMovePerformed;
        map["Move"].canceled -= OnMoveCanceled;

        map["Jump"].started -= OnJumpStarted;
        map["Jump"].canceled -= OnJumpCanceled;

        map["Crouch"].started -= OnCrouchStarted;
        map["Crouch"].canceled -= OnCrouchCanceled;
    }

    /* ----------------------------------------------------------
                     CALLBACKS (FUNCIONAN)
    ---------------------------------------------------------- */

    private void OnMovePerformed(InputAction.CallbackContext ctx)
        => moveInput = ctx.ReadValue<Vector2>();

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
        => moveInput = Vector2.zero;

    private void OnJumpStarted(InputAction.CallbackContext ctx)
        => jumpPressed = true;

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
        => jumpPressed = false;

    private void OnCrouchStarted(InputAction.CallbackContext ctx)
        => crouchHeld = true;

    private void OnCrouchCanceled(InputAction.CallbackContext ctx)
        => crouchHeld = false;


    /* ----------------------------------------------------------
                     UNITY LOOP
    ---------------------------------------------------------- */

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        originalSize = transform.localScale;
    }

    void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down,
            playerHeight * 0.5f + 0.2f, whatIsGround);

        HandleInput();

        rb.linearDamping = (grounded && !isSliding) ? groundDrag : 0;
    }

    private void FixedUpdate()
    {
        MovePlayer();
        SpeedControl();
    }

    private void HandleInput()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
        Debug.Log(grounded);
        // ----------- SALTO -------------------
        if (jumpPressed && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // ----------- SLIDE / CROUCH ----------
        if (crouchHeld && !isCrouching && !isSliding && grounded)
        {
            float speed = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z).magnitude;

            if (speed < moveSpeed * 0.60f)
                Crouch();
            else
                Slide();
        }

        if (!crouchHeld)
        {
            if (isCrouching) CrouchEnd();
            if (isSliding) SlideEnd();
        }
    }


    /* ----------------------------------------------------------
                     MOVIMIENTO ORIGINAL
    ---------------------------------------------------------- */

    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        moveDirection.Normalize();

        if (OnSlope())
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 10f, ForceMode.Force);

        if (grounded)
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * 10f, ForceMode.Force);
        else
            rb.AddForce(new Vector3(moveDirection.x, 0, moveDirection.z) * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        flatVelocity = rb.angularVelocity;
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        if (flatVel.magnitude > maxSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * maxSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        Debug.Log("Jumping");
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump() => readyToJump = true;


    /* ----------------------------------------------------------
                     CROUCH / SLIDE
    ---------------------------------------------------------- */

    private void Crouch()
    {
        transform.localScale = new Vector3(originalSize.x, originalSize.y * 0.6f, originalSize.z);
        playerHeight *= 0.6f;
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        maxSpeed /= 2;
        moveSpeed /= 2;
        isCrouching = true;
    }

    private void CrouchEnd()
    {
        transform.localScale = originalSize;
        playerHeight *= (100f / 60f);
        maxSpeed *= 2;
        moveSpeed *= 2;
        isCrouching = false;
    }

    private void Slide()
    {
        isSliding = true;
        transform.localScale = new Vector3(originalSize.x, originalSize.y * 0.6f, originalSize.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        moveSpeed *= 0.2f;
        maxSpeed *= 10f;
        rb.linearDamping = slideDeceleration;
        rb.AddForce(moveDirection * slideSpeed, ForceMode.Impulse);
    }

    private void SlideEnd()
    {
        transform.localScale = originalSize;

        if (grounded)
        {
            rb.angularVelocity *= 0.3f;
            rb.linearDamping = groundDrag;
        }
        else rb.linearDamping = 0;

        moveSpeed *= 5f;
        maxSpeed *= 0.1f;
        isSliding = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit,
            playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() =>
        Vector3.ProjectOnPlane(moveDirection, slopeHit.normal);
}
