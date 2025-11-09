using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Slide : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;
    public float gravity = -9.81f;

    [Header("Slide")]
    public float slideBoostMultiplier = 1.3f; // multiplica la velocidad al iniciar el slide
    public float slideDuration = 0.9f;
    public float slideHeight = 0.8f;
    public float normalHeight = 2f;
    public AnimationCurve slideCurve;

    private CharacterController controller;
    private Vector3 moveDir;
    private Vector3 slideVelocity;
    private float verticalVelocity;
    private bool isSliding = false;
    private float slideTimer;
    private float originalCenterY;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalCenterY = controller.center.y;

        if (slideCurve == null || slideCurve.length == 0)
            slideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    }

    void Update()
    {
        if (!isSliding)
            HandleMovement();
        else
            HandleSlide();
    }

    void HandleMovement()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        Vector3 inputDir = (transform.right * inputX + transform.forward * inputZ).normalized;

        moveDir = inputDir * moveSpeed;

        controller.Move(moveDir * Time.deltaTime);

        // Gravedad
        if (controller.isGrounded)
            verticalVelocity = -1f;
        else
            verticalVelocity += gravity * Time.deltaTime;

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // Iniciar slide
        if (Input.GetKeyDown(KeyCode.LeftControl) && inputDir.magnitude > 0.1f && controller.isGrounded)
            StartSlide();
    }

    void StartSlide()
    {
        isSliding = true;
        slideTimer = slideDuration;

        // Mantener la dirección actual y añadir boost
        Vector3 horizontalVelocity = new Vector3(moveDir.x, 0, moveDir.z);
        slideVelocity = horizontalVelocity * slideBoostMultiplier;

        // Ajustar la altura del jugador
        controller.height = slideHeight;
        controller.center = new Vector3(controller.center.x, slideHeight / 2, controller.center.z);
    }

    void HandleSlide()
    {
        slideTimer -= Time.deltaTime;

        // Curva de desaceleración (1 → 0)
        float curveValue = slideCurve.Evaluate(1 - (slideTimer / slideDuration));
        float currentSpeedMultiplier = Mathf.Lerp(1f, 0.3f, curveValue);

        Vector3 currentSlideMove = slideVelocity * currentSpeedMultiplier * Time.deltaTime;

        controller.Move(currentSlideMove);

        // Aplicar gravedad
        if (!controller.isGrounded)
            verticalVelocity += gravity * Time.deltaTime;

        controller.Move(Vector3.up * verticalVelocity * Time.deltaTime);

        // Termina slide si pasa el tiempo o se suelta Ctrl
        if (slideTimer <= 0 || Input.GetKeyUp(KeyCode.LeftControl))
            StopSlide();
    }

    void StopSlide()
    {
        isSliding = false;

        controller.height = normalHeight;
        controller.center = new Vector3(controller.center.x, originalCenterY, controller.center.z);

        // Recupera parte de la velocidad del slide al terminar (momentum)
        moveDir = new Vector3(slideVelocity.x, 0, slideVelocity.z) * 0.5f;
    }
}
