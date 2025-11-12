using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerLook : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerBody;       // El transform del jugador (gira en Y)
    public Transform playerCamera;     // La cámara (gira en X)

    [Header("Sensibilidad")]
    public float lookSensitivity = 100f;
    public float minPitch = -45f;
    public float maxPitch = 45f;

    private float pitch = 0f;          // Rotación vertical de la cámara
    private Vector2 lookInput = Vector2.zero;

    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    void OnEnable()
    {
        var map = playerInput.currentActionMap;

        map["Look"].performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        map["Look"].canceled += ctx => lookInput = Vector2.zero;
    }

    void OnDisable()
    {
        var map = playerInput.currentActionMap;

        map["Look"].performed -= ctx => lookInput = ctx.ReadValue<Vector2>();
        map["Look"].canceled -= ctx => lookInput = Vector2.zero;
    }

    void LateUpdate()
    {
        // Rotación horizontal (Player)
        float deltaX = lookInput.x * lookSensitivity * Time.deltaTime;
        playerBody.Rotate(Vector3.up * deltaX);

        // Rotación vertical (Cámara)
        float deltaY = lookInput.y * lookSensitivity * Time.deltaTime;
        pitch -= deltaY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
