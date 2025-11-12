using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class FPSInput : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 6.0f;
    [Header("Salto y gravedad")]
    public float jumpHeight = 2.5f;
    public float gravity = -9.8f;
    public float fallMultiplier = 1.5f;
    public float jumpCutMultiplier = 0.5f;

    private CharacterController controller;
    private Slide slide;

    private Vector3 velocity;
    private bool isGrounded;

    [Header("Camera")]
    public Transform cameraTransform;
    public float lookSensitivity = 100f;
    private float xRotation = 0f;

    // Variables de input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool jumpPressed;
    private bool slidePressed;

    private PlayerInput playerInput;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        slide = GetComponent<Slide>();
        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        var map = playerInput.currentActionMap;

        map["Move"].performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        map["Move"].canceled += ctx => moveInput = Vector2.zero;

        map["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        map["Look"].canceled += ctx => lookInput = Vector2.zero;

        map["Jump"].started += ctx => jumpPressed = true;
        map["Jump"].canceled += ctx => jumpPressed = false;

        map["Slide"].started += ctx => slidePressed = true;
        map["Slide"].canceled += ctx => slidePressed = false;
    }

    void OnDisable()
    {
        var map = playerInput.currentActionMap;

        map["Move"].performed -= ctx => moveInput = ctx.ReadValue<Vector2>();
        map["Move"].canceled -= ctx => moveInput = Vector2.zero;

        map["Look"].performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        map["Look"].canceled -= ctx => lookInput = Vector2.zero;

        map["Jump"].started -= ctx => jumpPressed = true;
        map["Jump"].canceled -= ctx => jumpPressed = false;

        map["Slide"].started -= ctx => slidePressed = true;
        map["Slide"].canceled -= ctx => slidePressed = false;
    }

    void Update()
    {
        // Movimiento horizontal
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        move = Vector3.ClampMagnitude(move, 1f) * speed;

        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Salto
        if (jumpPressed && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        // Gravedad
        if (velocity.y < 0)
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        else
            velocity.y += gravity * Time.deltaTime;

        if (!isGrounded && !jumpPressed && velocity.y > 0f)
            velocity.y *= jumpCutMultiplier;

        // Slide
        slide.slideButtonHeld = slidePressed;
        if (slidePressed && isGrounded)
            slide.TryStartSlide(move);

        slide.UpdateSlide();

        // Movimiento final
        if (slide.IsSliding)
            controller.Move(slide.SlideVelocity * Time.deltaTime);
        else
            controller.Move((move + Vector3.up * velocity.y) * Time.deltaTime);

        // Rotación cámara
        RotateCamera();
    }

    private void RotateCamera()
    {
        float mouseX = lookInput.x * lookSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * lookSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
}
