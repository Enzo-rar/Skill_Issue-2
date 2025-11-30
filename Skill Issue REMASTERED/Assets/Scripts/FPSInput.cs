using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class FPSInput : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float maxSpeed = 10f;
    public float groundDrag = 6f;
    public float jumpForce = 12f;
    public float jumpCooldown = 0.25f;
    public float airMultiplier = 0.4f;

    [Header("Slide")]
    public float slideSpeed = 12f;
    public float slideDeceleration = 2f; // Importante para que el slide no sea infinito

    bool readyToJump = true;
    bool isCrouching = false;
    bool isSliding = false;

    [Header("Ground Check")]
    public float playerHeight = 2f;
    public LayerMask whatIsGround;
    bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle = 40f;
    private RaycastHit slopeHit;

    public Transform orientation;

    [SerializeField] private PlayerInput playerInput;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool crouchHeld;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;
    Vector3 originalSize;

    Rigidbody rb;

    /* ----------------------------------------------------------
                     REGISTRO INPUT
    ---------------------------------------------------------- */

    void OnEnable()
    {
        var map = playerInput.currentActionMap;
        if(map == null) return; // Seguridad

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
        if (map == null) return;

        map["Move"].performed -= OnMovePerformed;
        map["Move"].canceled -= OnMoveCanceled;

        map["Jump"].started -= OnJumpStarted;
        map["Jump"].canceled -= OnJumpCanceled;

        map["Crouch"].started -= OnCrouchStarted;
        map["Crouch"].canceled -= OnCrouchCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext ctx) => moveInput = Vector2.zero;
    private void OnJumpStarted(InputAction.CallbackContext ctx) => jumpPressed = true;
    private void OnJumpCanceled(InputAction.CallbackContext ctx) => jumpPressed = false;
    private void OnCrouchStarted(InputAction.CallbackContext ctx) => crouchHeld = true;
    private void OnCrouchCanceled(InputAction.CallbackContext ctx) => crouchHeld = false;

    /* ----------------------------------------------------------
                     UNITY LOOP
    ---------------------------------------------------------- */

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        
        // 1. Interpolación activada para suavidad visual
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        originalSize = transform.localScale;

       
        // Creamos un material sin fricción en tiempo de ejecución.
        // Esto evita que el jugador se "trabe" al rozar paredes, eliminando el jitter lateral.
        CapsuleCollider col = GetComponent<CapsuleCollider>();
        if (col != null)
        {
            PhysicsMaterial slipperyMat = new PhysicsMaterial("ZeroFriction");
            slipperyMat.dynamicFriction = 0f;
            slipperyMat.staticFriction = 0f;
            slipperyMat.frictionCombine = PhysicsMaterialCombine.Minimum;
            slipperyMat.bounceCombine = PhysicsMaterialCombine.Minimum;
            col.material = slipperyMat;
        }
        
    }

    void Update()
    {
        // Ground Check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        HandleInput();
        SpeedControl();

        // Gestión de Drag (Rozamiento)
        if (grounded)
        {
            if (isSliding)
                rb.linearDamping = slideDeceleration; // Usamos el drag específico del slide
            else
                rb.linearDamping = groundDrag; // Drag normal de caminar
        }
        else
        {
            rb.linearDamping = 0; // En el aire no hay fricción
        }
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void HandleInput()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;

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
            // Solo hacemos Slide si nos movemos lo suficientemente rápido
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            if (flatVel.magnitude > moveSpeed * 0.5f) 
                Slide();
            else 
                Crouch();
        }

        if (!crouchHeld)
        {
            if (isCrouching) CrouchEnd();
            if (isSliding) SlideEnd();
        }
    }

    private void MovePlayer()
    {
        // Si estamos deslizando, NO aplicamos fuerza de movimiento normal para dejar que la inercia actúe
        // Opcional: permitir un control muy reducido durante el slide
        if (isSliding) return;

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // Pendientes
        if (OnSlope() && !jumpPressed)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if (rb.linearVelocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        // Suelo
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // Aire
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }


        rb.useGravity = !OnSlope();

        // --- WALL FRICTION FIX: evita frenarse contra muros ---
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            // Detecta si hay muro adelante
            if (Physics.Raycast(transform.position, rb.linearVelocity.normalized, out RaycastHit wallHit, 0.6f))
            {
                Vector3 wallNormal = wallHit.normal;
                Debug.Log("Wall detected with normal: " + wallNormal);
                // Proyecta la velocidad para quitar la parte que empuja contra el muro
                Vector3 vel = rb.linearVelocity;
                float dot = Vector3.Dot(vel, wallNormal);

                if (dot < 0) // Si estás empujando contra la pared
                {
                    vel -= wallNormal * dot; // Quita solo el frenado lateral
                    rb.linearVelocity = vel;
                }
            }
        }

    }

    private void SpeedControl()
    {
        // --- SOLUCIÓN FLUIDEZ DASH ---
        // Si estamos haciendo Slide, ignoramos el límite de velocidad normal
        // para permitir el impulso inicial del dash.
        if (isSliding) return; 

        // Limitamos velocidad en pendientes
        if (OnSlope() && !jumpPressed)
        {
            if (rb.linearVelocity.magnitude > maxSpeed)
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
        // Limitamos velocidad en suelo/aire
        else
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Solo limitamos si superamos la velocidad máxima
            if (flatVel.magnitude > maxSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * maxSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }
    }

    private void Jump()
    {
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
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        isCrouching = true;
    }

    private void CrouchEnd()
    {
        transform.localScale = originalSize;
        isCrouching = false;
    }

    private void Slide()
    {
        isSliding = true;
        transform.localScale = new Vector3(originalSize.x, originalSize.y * 0.6f, originalSize.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        
        // Empuje inicial del Slide (Dash)
        // Usamos ForceMode.VelocityChange para ignorar la masa y dar un "golpe" seco instantáneo
        // O Impulse, pero VelocityChange suele sentirse más "snappy" para dashes.
        // Si prefieres impulse, cambia a ForceMode.Impulse
        rb.AddForce(moveDirection.normalized * slideSpeed, ForceMode.VelocityChange);
    }

    private void SlideEnd()
    {
        transform.localScale = originalSize;
        isSliding = false;
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDirection() => Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
}