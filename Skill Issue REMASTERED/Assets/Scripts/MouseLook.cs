using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class MouseLook : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerBody;       // El objeto padre (debe tener el Rigidbody)
    public Transform playerCamera;     // La cámara (Transform)
    public Transform orientation;      // Orientación auxiliar

    [Header("Componente Cámara")]
    // REFERENCIA PÚBLICA: Esto es crucial para el Raycast de disparo del jugador.
    public Camera PlayerCameraComponent; 

    [Header("Puntero (Crosshair)")]
    public float crosshairSize = 20f; // Tamaño del puntero en píxeles (Aumentado para mejor visibilidad)
    private Texture2D crosshairTexture;
    private Rect crosshairRect;

    [Header("Sensibilidad")]
    public float lookSensitivity = 15f; 
    public float minPitch = -85f;
    public float maxPitch = 85f;

    [Header("Suavizado (Opcional)")]
    [Tooltip("Valores bajos (0.01) para casi instantáneo, valores altos (0.1) para muy suave pero con lag")]
    public float smoothTime = 0.01f; // Pequeño suavizado para eliminar ruido del mouse

    private float pitch = 0f;
    private float yRotation = 0f; 
    
    private Vector2 currentInputVector;
    private Vector2 smoothInputVelocity;
    private Vector2 rawInput;

    private PlayerInput playerInput;
    private Rigidbody rb;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        // CORRECCIÓN CRÍTICA PARA SPLIT-SCREEN: Ocultar cursor.
        Cursor.visible = false; 

        if (playerBody != null)
        {
            rb = playerBody.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError($"MouseLook ({gameObject.name} - Player ID: {playerInput.playerIndex}): El playerBody no tiene un Rigidbody. La rotación física no funcionará.");
            }
        }

        // --- CORRECCIÓN CRÍTICA PARA SPLIT-SCREEN: BÚSQUEDA DE CÁMARA ROBUSTA ---
        
        // 1. Si playerCamera (Transform) está asignado, intentamos obtener el componente Camera desde allí.
        if (playerCamera != null)
        {
            // Si PlayerCameraComponent NO está asignado manualmente, lo buscamos en el Transform 'playerCamera'.
            if (PlayerCameraComponent == null)
            {
                PlayerCameraComponent = playerCamera.GetComponent<Camera>();
            }
        }
        
        // 2. Si PlayerCameraComponent sigue siendo nulo, buscamos una Cámara en los hijos.
        // Esto cubre el caso en que playerCamera no fue asignado o la instancia falló al crear la referencia.
        if (PlayerCameraComponent == null)
        {
             PlayerCameraComponent = GetComponentInChildren<Camera>();
        }
        
        // --- FIN DE CORRECCIÓN ---

        if (PlayerCameraComponent == null)
        {
             Debug.LogError($"MouseLook ({gameObject.name} - Player ID: {playerInput.playerIndex}): Error CRÍTICO. No se encontró el componente Camera en la jerarquía del jugador.");
        }

        // --- INICIALIZACIÓN DEL PUNTERO ---
        // Crea una textura simple de 1x1 píxel ROJA.
        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, Color.red); // Cambiado a rojo para distinguirlo
        crosshairTexture.Apply();
    }

    void OnEnable()
    {
        var map = playerInput.currentActionMap;
        if (map != null)
        {
            map["Look"].performed += ctx => rawInput = ctx.ReadValue<Vector2>();
            map["Look"].canceled += ctx => rawInput = Vector2.zero;
        }
    }

    void OnDisable()
    {
        if (playerInput != null)
        {
            var map = playerInput.currentActionMap;
            if (map != null)
            {
                map["Look"].performed -= ctx => rawInput = Vector2.zero;
                map["Look"].canceled -= ctx => rawInput = Vector2.zero;
            }
        }
    }

    void Update()
    {
        // Suavizado de Input
        currentInputVector = Vector2.SmoothDamp(currentInputVector, rawInput, ref smoothInputVelocity, smoothTime);

        float deltaX = currentInputVector.x * lookSensitivity * Time.deltaTime;
        float deltaY = currentInputVector.y * lookSensitivity * Time.deltaTime;

        // --- ROTACIÓN VERTICAL (CÁMARA) ---
        pitch -= deltaY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        playerCamera.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        // --- ACUMULAR ROTACIÓN HORIZONTAL ---
        yRotation += deltaX;

        // Sincronizar orientación auxiliar
        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    void FixedUpdate()
    {
        // --- ROTACIÓN HORIZONTAL (CUERPO FÍSICO) ---
        if (rb != null)
        {
            Quaternion targetRotation = Quaternion.Euler(0f, yRotation, 0f);
            rb.MoveRotation(targetRotation);
        }
        else
        {
            playerBody.rotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }

    void OnGUI()
    {
        // Solo dibuja el puntero si la cámara asociada a este script está activa
        if (PlayerCameraComponent != null && PlayerCameraComponent.isActiveAndEnabled)
        {
            // Obtener el viewport normalizado (rect) que define el área de la cámara
            Rect viewportRect = PlayerCameraComponent.rect;
            
            // 1. Calcular el centro del VIEWPORT en coordenadas de PANTALLA
            float screenW = Screen.width;
            float screenH = Screen.height;

            // Calcular el centro absoluto en píxeles (origen del viewport + (ancho del viewport / 2))
            float xCenter = (viewportRect.x + viewportRect.width / 2f) * screenW;
            float yCenter = (viewportRect.y + viewportRect.height / 2f) * screenH;
            
            // Nota: Unity GUI usa (0,0) en la esquina superior izquierda. viewport.y es desde abajo.
            float yCenterGUI = screenH - yCenter; // Invertir Y para el contexto de GUI

            // DEBUG: Confirma que la función se ejecuta y las coordenadas
            if (playerInput != null && playerInput.playerIndex == 1) 
            {
              //   Debug.Log($"Puntero P2 dibujando. Viewport rect: {viewportRect}. Centro P2 (Screen/GUI): {xCenter:F0},{yCenterGUI:F0}");
            }

            // 2. Definir el Rect del puntero usando las coordenadas de pantalla calculadas
            crosshairRect = new Rect(xCenter - crosshairSize / 2f, 
                                     yCenterGUI - crosshairSize / 2f, 
                                     crosshairSize, 
                                     crosshairSize);

            // 3. Dibuja la textura del puntero en el centro
            GUI.DrawTexture(crosshairRect, crosshairTexture);
        }
        else 
        {
            // Mensaje de Debug más informativo. Esto te dirá si el problema es la referencia (NULL) o la activación.
            string status = PlayerCameraComponent == null ? "NULL" : "Inactiva";
            Debug.LogWarning($"MouseLook ({gameObject.name} - Player ID: {playerInput.playerIndex}): La cámara no está lista ({status}).");
        }
    }
}