using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class Slide : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    public float slideBoostMultiplier = 1.5f;
    public float slideDuration = 1.0f;
    public float slideCooldown = 0.5f;
    public float sensitivityHor = 9.0f;

    [Header("Altura del jugador")]
    public float normalHeight = 2f;
    public float slideHeight = 1f;
    public Transform playerModel; // Modelo o cámara del jugador
    public float modelSmoothSpeed = 10f; // Velocidad de Lerp para suavizar

    [Header("Física")]
    public float slideFriction = 0.3f;
    public float gravityForce = 10f;
    public LayerMask groundMask;
    public float groundCheckDistance = 0.2f;

    private Rigidbody rb;
    private CapsuleCollider capsule;
    private Vector3 inputDir;
    private bool isSliding = false;
    private bool isOnCooldown = false;
    private float slideTimer;
    private float cooldownTimer;
    private float originalHeight;
    private Vector3 modelOriginalLocalPos;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        capsule = GetComponent<CapsuleCollider>();
        originalHeight = capsule.height;

        if (playerModel) modelOriginalLocalPos = playerModel.localPosition;
    }

    void Update()
    {
        PlayerRotation();
        HandleInput();
        HandleCooldown();
        SmoothModelHeight();
    }

    void FixedUpdate()
    {
        ApplyGravity();

        if (isSliding)
            ApplySlide();
        else
            ApplyMovement();
    }

    void PlayerRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * sensitivityHor;
        Quaternion deltaRotation = Quaternion.Euler(0f, mouseX, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    void HandleInput()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        inputDir = (transform.forward * z + transform.right * x).normalized;

        Debug.Log("KeyCode.LeftControl: " + Input.GetKeyDown(KeyCode.LeftControl));
        // Iniciar slide
        if (Input.GetKeyDown(KeyCode.LeftControl) && !isSliding && !isOnCooldown && IsGrounded() && inputDir.magnitude > 0.1f)
            StartSlide();
            Debug.Log("Slide started");
        // Terminar slide al soltar Ctrl
        if (Input.GetKeyUp(KeyCode.LeftControl) && isSliding)
            StopSlide();
    }

    void ApplyMovement()
    {
        if (inputDir.magnitude > 0)
        {
            Vector3 move = inputDir * moveSpeed;
            rb.linearVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        }
        else
        {
            // Mantener un poco de momentum
            rb.linearVelocity = new Vector3(rb.linearVelocity.x * 0.9f, rb.linearVelocity.y, rb.linearVelocity.z * 0.9f);
        }
    }

    void StartSlide()
    {
        isSliding = true;
        isOnCooldown = true;
        cooldownTimer = slideCooldown;
        slideTimer = slideDuration;

        // Ajustar collider
        capsule.height = slideHeight;
        capsule.center = new Vector3(0, (slideHeight - normalHeight) / 2f, 0);

        // Momentum inicial
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        if (horizontalVel.magnitude < moveSpeed)
            horizontalVel = inputDir * moveSpeed;

        rb.linearVelocity = horizontalVel * slideBoostMultiplier;
    }

    void ApplySlide()
    {
        slideTimer -= Time.fixedDeltaTime;

        // Fricción progresiva
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        horizontalVel *= (1f - slideFriction * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);

        // Termina slide
        if (slideTimer <= 0f || !IsGrounded())
            StopSlide();
    }

    void StopSlide()
    {
        isSliding = false;
        capsule.height = normalHeight;
        capsule.center = Vector3.zero;
    }

    void HandleCooldown()
    {
        if (isOnCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                isOnCooldown = false;
        }
    }

    void ApplyGravity()
    {
        if (!IsGrounded())
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
    }

    bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        return Physics.Raycast(origin, Vector3.down, groundCheckDistance + 0.1f, groundMask);
    }

    void SmoothModelHeight()
    {
        if (!playerModel) return;

        Vector3 targetLocalPos = modelOriginalLocalPos;

        if (isSliding)
            targetLocalPos.y = modelOriginalLocalPos.y - (normalHeight - slideHeight) / 2f;

        // Suavizado con Lerp
        playerModel.localPosition = Vector3.Lerp(playerModel.localPosition, targetLocalPos, Time.deltaTime * modelSmoothSpeed);
    }
}
