using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCam : MonoBehaviour
{
    [SerializeField] private PlayerInput playerInput;

    public float sensX, sensY;
    float xRotation, yRotation;

    public Transform orientation;

    Vector2 lookInput;

    void OnEnable()
    {
        if (playerInput == null)
        {
            Debug.LogError("PlayerInput no asignado en PlayerCam");
            return;
        }

        var map = playerInput.currentActionMap;
        if (map == null) return;

        // Suscribimos la acción "Look" (o como se llame en tu Input Actions)
        map["Look"].performed += OnLookPerformed;
        map["Look"].canceled += OnLookCanceled;
    }

    void OnDisable()
    {
        if (playerInput == null) return;
        var map = playerInput.currentActionMap;
        if (map == null) return;

        map["Look"].performed -= OnLookPerformed;
        map["Look"].canceled -= OnLookCanceled;
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx) => lookInput = ctx.ReadValue<Vector2>();
    private void OnLookCanceled(InputAction.CallbackContext ctx) => lookInput = Vector2.zero;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = lookInput.x * Time.deltaTime * sensX;
        float mouseY = lookInput.y * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}
