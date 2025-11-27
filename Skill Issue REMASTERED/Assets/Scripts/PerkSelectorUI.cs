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
    // Variables internas para saber quién es quién en este momento
    private InputSystemUIInputModule uiInputModule;
    private int idPerdedorActual;
    private int idGanadorActual;


    void Start()
    {
        // Accede al GameObject del EventSystem actual y busca el módulo en él. (Solo existe un EventSystem por escena)
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
    // RECORDAR SIEMPRE GENERAR EN UNA ESCENA PRIMERO UN CANVAS Y LUEGO EL PERKMANAGER ******
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
            Debug.Log("Asignando camara jugador 1 al canvas de ventajas CamaraJugador1 -> " + GameManager.Instance.playerCamera1.name +" canvasVentajas -> " + canvasVentajas.name);
            canvasVentajas.worldCamera = GameManager.Instance.playerCamera1;
            canvasVentajas.planeDistance = 0.5f; // Asegura que el canvas esté frente a la cámara
            AsignarControlUI(GameManager.Instance.playerInput1);
        }
        else
        {     Debug.Log("Asignando camara jugador 2 al canvas de ventajas CamaraJugador2 -> " + GameManager.Instance.playerCamera2.name +" canvasVentajas -> " + canvasVentajas.name);
            canvasVentajas.worldCamera = GameManager.Instance.playerCamera2;
            canvasVentajas.planeDistance = 0.5f; // Asegura que el canvas esté frente a la cámara
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
        // Lista para guardar las cartas generadas
        List<BotonVentajaUI> cartasInstanciadas = new List<BotonVentajaUI>();

        // 2. Pedir 3 ventajas aleatorias al PerkManager
        List<Ventajas> opciones = PerkManager.Instance.ObtenerOpcionesAleatorias(3);

        // 3. Crear los botones
        foreach (Ventajas ventaja in opciones)
        {
            Debug.Log("Generando carta para ventaja: " + ventaja.nombreVentaja);
            //****** Falta Crear un Prefab de BotonVentajaUI y asignarlo en el inspector
            BotonVentajaUI nuevaCarta = Instantiate(prefabCarta, contenedorDeCartas);
            
            // AQUÍ OCURRE LA MAGIA:
            // Esta funcion se aplicara al boton creado para que cuando lo elijas llame a ConfirmarEleccion con la ventaja correcta en este script
            nuevaCarta.ConfigurarCarta(ventaja, () => ConfirmarEleccion(ventaja));
       
            cartasInstanciadas.Add(nuevaCarta);
        }

         // 4. Establecer el foco inicial usando la primera carta generada
        if (cartasInstanciadas.Count > 0)
        {
            EstablecerFoco(cartasInstanciadas[0].gameObject);
        }
    }

    //Funcion para establecer el foco (seleccion con un reborde de color o algo visual) en el primer boton generado
    private void EstablecerFoco(GameObject objetoInicial)
    {
        // Asegúrate de que existe el EventSystem.
        if (EventSystem.current != null)
        {
            // Limpia el foco anterior
            EventSystem.current.SetSelectedGameObject(null);
            
            // Establece el foco en el primer botón generado
            EventSystem.current.SetSelectedGameObject(objetoInicial);
        }
        else
        {
            Debug.LogError("No se encontró un EventSystem en la escena. Asegúrate de que haya uno presente para manejar la selección de UI.");
        }

    }

    private void ConfirmarEleccion(Ventajas ventajaElegida)
    {
        // 1. Comunicar la decisión al PerkManager
        PerkManager.Instance.AplicarVentajaSeleccionada(ventajaElegida, idPerdedorActual, idGanadorActual);
        
        // 2. Limpiar el foco del sistema de eventos
        EventSystem.current.SetSelectedGameObject(null);

        // 3. Cerrar la UI
        panelContenedor.SetActive(false);
    }

public void AsignarControlUI(PlayerInput InputJugador)
    {
        if (uiInputModule == null)
        {
            Debug.LogError("uiInputModule es nulo. Asignación fallida.");
            return;
        }
        // 1. Asignar el control al jugador correcto.
        uiInputModule.actionsAsset = InputJugador.actions;
       
        
        // 2. Asignar las referencias de las acciones específicas.
        uiInputModule.move = InputActionReference.Create(InputJugador.actions["Navigate"]);
        uiInputModule.submit = InputActionReference.Create(InputJugador.actions["Submit"]);
        uiInputModule.cancel = InputActionReference.Create(InputJugador.actions["Cancel"]);

        //Asegura que las acciones esten habilitadas
        //uiInputModule.actionsAsset.Enable();
        
    }


}