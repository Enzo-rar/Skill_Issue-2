using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.EventSystems;

public class PerkSelectorUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField]
    public GameObject panelContenedor; // El objeto padre que activas/desactivas
    public Transform contenedorDeCartas; // Donde se instancian los botones (Grid Layout Group)
    public BotonVentajaUI prefabCarta;   // El prefab que tiene el script de arriba
    
    [SerializeField]
    public Canvas canvasVentajas; // Canvas que contiene la UI de ventajas

    // Variables internas
    private InputSystemUIInputModule uiInputModule;
    private int idPerdedorActual;
    private int idGanadorActual;

    private int indiceSeleccionado = 0; // Índice de la carta actualmente seleccionada
    private List<BotonVentajaUI> cartasInstanciadas;

    // Variables para guardar las acciones y poder desuscribirse luego
    private InputAction navegacionAction;
    private InputAction submitAction;


    void Start()
    {
        // Accede al GameObject del EventSystem actual y busca el módulo en él.
        if (EventSystem.current != null)
        {
            uiInputModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
        }
        
        if (uiInputModule == null)
        {
            Debug.LogError("InputSystemUIInputModule no encontrado en el objeto EventSystem.");
        }
    }

    // --- LLAMADO POR EL GAMEMANAGER AL FINAL DE LA RONDA ---
    public void InicializarSeleccion(int idPerdedor, int idGanador)
    {
        idPerdedorActual = idPerdedor;
        idGanadorActual = idGanador;
        
        if (canvasVentajas == null)
        {
            Debug.LogError("Canvas de Ventajas no asignado en PerkSelectorUI.");
            return;
        }

        if (idPerdedor == 1)
        {
            // Debug.Log("Asignando Jugador 1");
            canvasVentajas.worldCamera = GameManager.Instance.playerCamera1;
            canvasVentajas.planeDistance = 0.5f; 
            AsignarControlUI(GameManager.Instance.playerInput1);

        }
        else
        {    
            // Debug.Log("Asignando Jugador 2");
            canvasVentajas.worldCamera = GameManager.Instance.playerCamera2;
            canvasVentajas.planeDistance = 0.5f; 
            AsignarControlUI(GameManager.Instance.playerInput2);
        }

        panelContenedor.SetActive(true);
        GenerarCartas();
    }

    private void GenerarCartas()
    {
        // 1. Limpiar cartas anteriores
        foreach (Transform hijo in contenedorDeCartas)
        {
            Destroy(hijo.gameObject);
        }
        
        cartasInstanciadas = new List<BotonVentajaUI>();

        // 2. Pedir 3 ventajas aleatorias al PerkManager
        List<Ventajas> opciones = PerkManager.Instance.ObtenerOpcionesAleatorias(3);

        // 3. Crear los botones
        foreach (Ventajas ventaja in opciones)
        {
            BotonVentajaUI nuevaCarta = Instantiate(prefabCarta, contenedorDeCartas);
            
            // Configurar la carta y el evento de Click (para ratón y para Invoke)
            nuevaCarta.ConfigurarCarta(ventaja, () => ConfirmarEleccion(ventaja));
       
            cartasInstanciadas.Add(nuevaCarta);
        }

        // 4. Reiniciar índice y establecer foco visual inicial
        indiceSeleccionado = 0;
        if (cartasInstanciadas.Count > 0)
        {
            EstablecerFoco(cartasInstanciadas[0].gameObject);
            // Forzamos el resaltado visual en la primera carta
            cartasInstanciadas[0].SetResaltado(true);
        }
    }

    private void EstablecerFoco(GameObject objetoInicial)
    {
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(objetoInicial);
        }
    }

    private void Navegar(int direccion) // direccion es +1 (Derecha) o -1 (Izquierda)
    {
        if (cartasInstanciadas == null || cartasInstanciadas.Count == 0) return;

        // 1. Quitar el resaltado de la carta anterior
        cartasInstanciadas[indiceSeleccionado].SetResaltado(false); 
        
        // 2. Calcular el nuevo índice (Lógica de navegación circular)
        int numCartas = cartasInstanciadas.Count;
        indiceSeleccionado = (indiceSeleccionado + direccion) % numCartas;
        
        // Corrige el índice si el módulo resultó negativo
        if (indiceSeleccionado < 0)
        {
            indiceSeleccionado += numCartas; 
        }

        // 3. Aplicar el resaltado a la nueva carta
        cartasInstanciadas[indiceSeleccionado].SetResaltado(true);
    }

    private void ConfirmarEleccion(Ventajas ventajaElegida)
    {
        // 1. Comunicar la decisión al PerkManager
        PerkManager.Instance.AplicarVentajaSeleccionada(ventajaElegida, idPerdedorActual, idGanadorActual);
        
        // 2. Desuscribir Acciones (LIMPIEZA CRÍTICA)
        DesuscribirInputs();

        // 3. Limpiar el foco del sistema de eventos
        if(EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);

        // 4. Cerrar la UI
        panelContenedor.SetActive(false);
    }

    private void OnNavigatePerformed(InputAction.CallbackContext context)
    {
        // Leemos el valor una sola vez al dispararse el evento
        Vector2 navegacion = context.ReadValue<Vector2>();

        if (navegacion.x > 0.5f)
        {
            Navegar(1); // Mover a la carta derecha
        }
        else if (navegacion.x < -0.5f)
        {
            Navegar(-1); // Mover a la carta izquierda
        }
    }

    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        // ESTA ES LA CORRECCIÓN CLAVE:
        // En lugar de pedir datos, simulamos el click en el botón seleccionado.
        if (cartasInstanciadas != null && cartasInstanciadas.Count > 0)
        {
            // Invocamos el onClick del botón, que ya tiene la lambda "ConfirmarEleccion(ventaja)" configurada.
            cartasInstanciadas[indiceSeleccionado].botonComponente.onClick.Invoke();
        }
    }

    public void AsignarControlUI(PlayerInput InputJugador)
    {
        Debug.Log("Asignando control UI al jugador con ID: " + InputJugador.playerIndex+" con IdDeJugadorPerdedor ->"+idPerdedorActual);
        if (uiInputModule == null) return;

        uiInputModule.actionsAsset = InputJugador.actions;          

        //Suscripción Manual (Para lógica visual de navegación y selección)
        // 1. Navegación
        navegacionAction = InputJugador.actions["Navigate"];
        if(navegacionAction != null)
        {
            navegacionAction.performed += OnNavigatePerformed;
            navegacionAction.Enable();
        }

        // 2. Submit / Interact
        submitAction = InputJugador.actions["Submit"]; // Asegúrate que se llama "Submit" o "Interact" en tu input
        if(submitAction != null)
        {
            submitAction.performed += OnSubmitPerformed;
            submitAction.Enable();
        }

    }

    // Función auxiliar para limpiar inputs
    private void DesuscribirInputs()
    {
        if (submitAction != null)
        {
            submitAction.performed -= OnSubmitPerformed;
            submitAction.Disable();
            submitAction = null;
        }

        if (navegacionAction != null)
        {
            navegacionAction.performed -= OnNavigatePerformed;
            navegacionAction.Disable();
            navegacionAction = null;
        }
    }
    
    // Seguridad extra: Si se destruye el objeto o se desactiva
    private void OnDisable()
    {
         DesuscribirInputs();
    }
}