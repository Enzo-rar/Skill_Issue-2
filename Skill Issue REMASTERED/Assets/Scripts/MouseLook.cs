using UnityEngine;
using UnityEngine.InputSystem;
public class MouseLook : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerBody;       // Referencia al cuerpo (DEBE ESTAR FIJO/NO ROTAR)
    // Se elimina la referencia pública a 'playerCamera' ya que este script está adjunto a la cámara.
    public Transform orientation;      // Orientación auxiliar (maneja la rotación horizontal del cuerpo para PlayerMovement)

    [Header("Componente Cámara")]
    // REFERENCIA PÚBLICA: Esto es crucial para el Raycast de disparo del jugador.
    // Esta referencia ahora apunta al componente Camera del GameObject actual.
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

    void Awake()
    {
        playerInput = GetComponentInParent<PlayerInput>(); // Se busca el PlayerInput en el padre
        
        // Asumimos que este script está en el GameObject de la cámara
        PlayerCameraComponent = GetComponent<Camera>();

        // CORRECCIÓN CRÍTICA PARA SPLIT-SCREEN: Ocultar cursor.
        Cursor.visible = false; 
        
        if (PlayerCameraComponent == null)
        {
             Debug.LogError($"MouseLook ({gameObject.name} - Player ID: {playerInput.playerIndex}): Error CRÍTICO. Este script debe estar adjunto directamente al GameObject de la cámara.");
        }

        // --- INICIALIZACIÓN DEL PUNTERO ---
        // Crea una textura simple de 1x1 píxel ROJA.
        crosshairTexture = new Texture2D(1, 1);
        crosshairTexture.SetPixel(0, 0, Color.red); // Cambiado a rojo para distinguirlo
        crosshairTexture.Apply();
    }

    void OnEnable()
    {
        // Se busca el PlayerInput en el padre si aún no se ha hecho
        if (playerInput == null) {
            playerInput = GetComponentInParent<PlayerInput>();
        }else{
            Debug.LogWarning("PlayerInput es nulo en MouseLook OnEnable para el jugador con ID: " + playerInput.playerIndex);
        }

        
        var map = playerInput.currentActionMap;
        if (map != null)
        {
            map["Look"].performed += ctx => rawInput = ctx.ReadValue<Vector2>();
            map["Look"].canceled += ctx => rawInput = Vector2.zero;
        }

        if (playerInput.currentControlScheme == "Gamepad")
        {
            lookSensitivity = 220;
        }
        
    }

    void OnDisable()
    {
        if (playerInput == null) return;
        var map = playerInput.currentActionMap;
        if (map == null) return;

        map["Look"].performed -= ctx => rawInput = Vector2.zero;
        map["Look"].canceled -= ctx => rawInput = Vector2.zero;
    }

    void Update()
    {
        // 1. INPUT (Suavizado)
        currentInputVector = Vector2.SmoothDamp(currentInputVector, rawInput, ref smoothInputVelocity, smoothTime);

        float deltaX = currentInputVector.x * lookSensitivity * Time.deltaTime;
        float deltaY = currentInputVector.y * lookSensitivity * Time.deltaTime;
        
        // 2. ROTACIÓN VERTICAL (Pitch)
        pitch -= deltaY; 
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); 
        
        // 3. ROTACIÓN HORIZONTAL (Yaw)
        yRotation += deltaX;

       
        // Esto hace que la cámara maneje toda la rotación visual, siempre que su padre esté fijo.
        transform.localRotation = Quaternion.Euler(pitch, yRotation, 0f); 

        // 4. SINCRONIZACIÓN DE MOVIMIENTO:
        // El Transform 'orientation' DEBE rotar para que el script PlayerMovement sepa hacia dónde moverse.
        if (orientation != null)
        {
            orientation.rotation = Quaternion.Euler(0f, yRotation, 0f); 
        }

        // Se elimina la rotación del playerBody para asegurar que la cámara lo controle.
    }

    void FixedUpdate()
    {
        // Este bloque se mantiene vacío, según tu petición.
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

            // 2. Definir el Rect del puntero usando las coordenadas de pantalla calculadas
            crosshairRect = new Rect(xCenter - crosshairSize / 2f, 
                                     yCenterGUI - crosshairSize / 2f, 
                                     crosshairSize, 
                                     crosshairSize);

            // 3. Dibuja la textura del puntero en el centro
            GUI.DrawTexture(crosshairRect, crosshairTexture);
        }
    }
}