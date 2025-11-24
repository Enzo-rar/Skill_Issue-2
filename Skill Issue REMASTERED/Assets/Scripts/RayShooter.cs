using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RayShooter : MonoBehaviour
{
    [Header("Input System")]
    public InputActionAsset inputActions;
    private InputAction attackAction;
    private InputAction zoomAction;

    [Header("Zoom Settings")]
    public float zoomMultiplier = 0.5f;
    public float zoomSpeed = 10f; // para zoom suave
    private float originalFOV;
    private Camera _camera;

    private void OnEnable()
    {
        var map = inputActions.FindActionMap("Player");

        attackAction = map.FindAction("Attack");
        zoomAction  = map.FindAction("Zoom");

        attackAction.Enable();
        zoomAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.Disable();
        zoomAction.Disable();
    }

    private void Start()
    {
        _camera = GetComponent<Camera>();
        originalFOV = _camera.fieldOfView;

        // Bloqueo del cursor SOLO si esta cámara pertenece al jugador local real
        // (si no quieres esto en multiplayer local, lo puedes quitar)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleZoom();
        HandleAttack();
    }

    private void HandleZoom()
    {
        float targetFOV = zoomAction.IsPressed() ? originalFOV * zoomMultiplier : originalFOV;

        // Zoom suave
        _camera.fieldOfView = Mathf.Lerp(
            _camera.fieldOfView,
            targetFOV,
            Time.deltaTime * zoomSpeed
        );
    }

    private void HandleAttack()
    {
        if (!attackAction.WasPressedThisFrame())
            return;

        // EL CAMBIO CRÍTICO: viewport-centro para evitar los 90 grados de error
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            var target = hit.transform.GetComponent<ReactiveTarget>();

            if (target != null)
            {
                target.ReactToHit();
            }
            else
            {
                StartCoroutine(SphereIndicator(hit.point));
            }
        }
    }

    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;

        Destroy(sphere.GetComponent<SphereCollider>());
        yield return new WaitForSeconds(5);
        Destroy(sphere);
    }

    private void OnGUI()
    {
        int size = 12;
        float posX = _camera.pixelWidth / 2 - size / 4;
        float posY = _camera.pixelHeight / 2 - size / 2;

        GUI.Label(new Rect(posX, posY, size, size), "*");
    }
}
