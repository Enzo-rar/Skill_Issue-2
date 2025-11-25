using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class RayShooter : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private PlayerInput playerInput;
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
      
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private PlayerCharacter playerStats;
    private GameObject _fireball;
    [SerializeField] private Camera _camera;



   private void OnEnable()
{
    // Cada PlayerInput tiene su propio ActionMap instanciado,
    // por lo que NO se comparte entre jugadores.
    var map = playerInput.currentActionMap;

    attackAction   = map["Attack"];
    interactAction = map["Interact"];
    tirarAction    = map["Tirar Arma"];
    zoomAction     = map["Zoom"];

    attackAction.Enable();
    interactAction.Enable();
    tirarAction.Enable();
    zoomAction.Enable();
}

private void OnDisable()
{
    attackAction.Disable();
    interactAction.Disable();
    tirarAction.Disable();
    zoomAction.Disable();
}

    private void Start()
    {
        _camera = GetComponent<Camera>();
        originalFOV = _camera.fieldOfView;

        if (playerStats == null)
        {
            Debug.LogError("RayShooter: playerStats no asignado en el inspector.");
        }
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
            
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                var item = hit.transform.gameObject;
                Debug.Log("camara: ", _camera);
                
                if (item != null)
                {
                    componenteReactivo = item.GetComponent<ObjetoReactivo>();
                    Debug.Log("componenteReactivo del item al que has hecho Raycast: ", componenteReactivo);
                    if (componenteReactivo != null)
                    {
                        Debug.Log(item.name + " seleccionado");
                        //En esta condicion puedes implementar una ventaja que te permita recoger el item aunque otro jugador lo tenga equipado (quitarselo de las manos)
                        if (componenteReactivo.IsGrabbed() == true)
                        {
                            Debug.Log("El item ya está equipado por otro jugador.");

                        } else
                        {
                            //Si tienes ya un arma en la mano que no puedas pillar otra, ¿se podria implementar una segunda arma?
                            if (!equipado)
                            {
                            componenteReactivo.ReactToCollect(item, _camera);
                            playerStats.SetItemEquipped(item);
                            equipado = true;  
                            }
                           
                        }
                        
                    }
                    
                }
                else
                {
                    Debug.Log("Seleccionado: " + hit.point + " (" + hit.transform.gameObject.name + "), no es un item valido");
                   
                }

            }
        }

        if (tirarAction.WasPressedThisFrame())
        {
            Debug.Log("QQQQQQQ");
            
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var superficie = hit.transform.gameObject;

                if (superficie != null)
                {
                    Debug.Log("Dropear en superficie: " + superficie.name);
                    
                    //componenteReactivo = playerStats._armaEquipada.GetComponent<ObjetoReactivo>();
                    var itemEquipado = playerStats.GetItemEquipped();
                    
                    if (itemEquipado != null)
                    {
                        ObjetoReactivo componenteReactivo = itemEquipado.GetComponent<ObjetoReactivo>();
                        componenteReactivo.ReactToDrop(itemEquipado, hit.point);
                        playerStats.SetItemEquipped(null);
                        equipado = false;
                        componenteReactivo.setGrabbed(false);
                    }else
                    {
                        Debug.Log("En la superficie -> " + hit.point + " (" + hit.transform.gameObject.name + "), no es un item valido ó no tienes nada equipado");
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
        
              // Punto exacto frente a la cámara (1.5m delante)
    Vector3 spawnPos = _camera.transform.position + _camera.transform.forward * 1.5f;

    // Orientación igual a la cámara
    Quaternion spawnRot = _camera.transform.rotation;

    _fireball = Instantiate(fireballPrefab, spawnPos, spawnRot);
   
    }
}
