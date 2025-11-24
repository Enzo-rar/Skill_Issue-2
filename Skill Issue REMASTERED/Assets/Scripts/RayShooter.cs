using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RayShooter : MonoBehaviour
{
    [Header("Input System")]
    public InputActionAsset inputActions;
    private InputAction attackAction;
    private InputAction zoomAction;
    private InputAction interactAction;
    private InputAction tirarAction;

    [Header("Zoom Settings")]
    public float zoomMultiplier = 0.5f;
    public float zoomSpeed = 10f; // para zoom suave
    private float originalFOV;
    private bool equipado;
    private GameObject _armaEquipada;
    private ObjetoReactivo componenteReactivo;
    PlayerCam posicionCamara;
      
    [SerializeField] private GameObject fireballPrefab;
    private GameObject _fireball;
    private Camera _camera;

    private void OnEnable()
    {
        var map = inputActions.FindActionMap("Player");

        attackAction = map.FindAction("Attack");
        zoomAction  = map.FindAction("Zoom");
        interactAction = map.FindAction("Interact");
        tirarAction  = map.FindAction("Tirar Arma");

        attackAction.Enable();
        zoomAction.Enable();
        interactAction.Enable();
        tirarAction.Enable();
    }

    private void OnDisable()
    {
        attackAction.Disable();
        zoomAction.Disable();
        interactAction.Disable();
        tirarAction.Disable();
    }

    private void Start()
    {
        _camera = GetComponent<Camera>();
        originalFOV = _camera.fieldOfView;

        // Bloqueo del cursor SOLO si esta cámara pertenece al jugador local real
        // (si no quieres esto en multiplayer local, lo puedes quitar)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        equipado = false;
    }
    void Update()
    {
        if (interactAction.WasPressedThisFrame())
        {
            Debug.Log("EEEEEEEEEEEE");
            Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
            Ray ray = _camera.ScreenPointToRay(point);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var item = hit.transform.gameObject;
                //si no tiene componenteReactivo salta error
                // var componenteReactivo = item.GetComponent<ObjetoReactivo>();
                Debug.Log("camara: ", _camera);
                Debug.Log("");
                if (item != null)
                {
                    componenteReactivo = item.GetComponent<ObjetoReactivo>();
                    if (componenteReactivo != null)
                    {
                        Debug.Log(item + " seleccionado");
                        componenteReactivo.ReactToCollect(item, _camera);
                        PlayerCharacter playerStats = _camera.GetComponentInParent<PlayerCharacter>();
                        playerStats.SetItemEquipped(item);
                        equipado = true;
                    }
                    
                }
                else
                {
                    Debug.Log("Seleccionado: " + hit.point + " (" + hit.transform.gameObject.name + "), no es un item valido");
                    //StartCoroutine(SphereIndicator(hit.point));
                }

            }
        }

        if (tirarAction.WasPressedThisFrame())
        {
            Debug.Log("QQQQQQQ");
            Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
            Ray ray = _camera.ScreenPointToRay(point);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var superficie = hit.transform.gameObject;

                if (superficie != null)
                {
                    Debug.Log("Dropear en superficie: ", superficie);
                    PlayerCharacter playerStats = _camera.GetComponentInParent<PlayerCharacter>();
                    //componenteReactivo = playerStats._armaEquipada.GetComponent<ObjetoReactivo>();
                    var itemEquipado = playerStats._armaEquipada;
                    ObjetoReactivo componenteReactivo = itemEquipado.GetComponent<ObjetoReactivo>();
                    if (itemEquipado != null)
                    {
                        componenteReactivo.ReactToDrop(itemEquipado, hit.point);
                        playerStats.SetItemEquipped(null);
                        equipado = false;
                    }

                    else
                    {
                        Debug.Log("Seleccionado: " + hit.point + " (" + hit.transform.gameObject.name + "), no es un item valido");
                        //StartCoroutine(SphereIndicator(hit.point));
                    }
                }
            }
        }

        HandleZoom();
        HandleAttack();
       
    }


    void OnGUI()
    {  // se ejecuta despu�s de dibujar el frame del juego
        int size = 12;
        float posX = _camera.pixelWidth / 2 - size / 4;
        float posY = _camera.pixelHeight / 2 - size / 2;
        GUI.Label(new Rect(posX, posY, size, size), "*"); // puede mostrar texto e im�genes
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

        if (equipado)
            {
                shootProjectile();
            }

        // EL CAMBIO CRÍTICO: viewport-centro para evitar los 90 grados de error
        // Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        // RaycastHit hit;

        // if (Physics.Raycast(ray, out hit))
        // {
        //     var target = hit.transform.GetComponent<ReactiveTarget>();

        //     if (target != null)
        //     {
        //         target.ReactToHit();
        //     }
        //     else
        //     {
        //         StartCoroutine(SphereIndicator(hit.point));
        //     }
        // }
    }

    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;

        Destroy(sphere.GetComponent<SphereCollider>());
        yield return new WaitForSeconds(5);
        Destroy(sphere);
    }

    void shootProjectile()
    {
        _fireball = Instantiate<GameObject>(fireballPrefab);
        posicionCamara = GetComponentInParent<PlayerCam>();
        _fireball.transform.Translate(posicionCamara.transform.position.x, posicionCamara.transform.position.y, posicionCamara.transform.position.z);
        _fireball.transform.position = transform.TransformPoint(Vector3.forward * 1.5f);
        _fireball.transform.rotation = transform.rotation;
    }
}
