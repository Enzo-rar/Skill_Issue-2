using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Configuración")]
    public int setsParaGanarRonda = 2; // Best of 3 (quien llegue a 2 gana)
    public int rondasParaGanarPartida = 3; // Best of 5 (quien llegue a 3 gana)
    [Header("Estado Actual")]
    public int scoreP1_Sets = 0;
    public int scoreP2_Sets = 0;
    public int rondaActual = 1;
    
   
    // Eventos para actualizar UI u otras cosas
    public UnityEvent<int> OnRoundEnded; // Pasa el ID del ganador (1 o 2)
    public UnityEvent OnSetEnded;


    // Referencia a la UI de selección de perks
    [SerializeField] private PerkSelectorUI perkSelectorUI;

    //Variables de los jugadores
    private int jugadoresRegistradosID = 1;
    public int Jugador1RondasGanadas = 0;
    public int Jugador2RondasGanadas = 0;

    public Camera playerCamera1;
    public Camera playerCamera2;
    private PlayerInput playerInput1;
    private PlayerInput playerInput2;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int RegistrarJugador(PlayerInput playerInput, Camera camaraJugador)
    {   
        if (jugadoresRegistradosID == 1)
        {
            playerInput1 = playerInput;
            playerCamera1 = camaraJugador;
            
        }
        else if (jugadoresRegistradosID == 2)
        {
            playerInput2 = playerInput;
            playerCamera2 = camaraJugador;
            //Configurar vista dividida vertical esto puede variar segun como quieras la division, se podria hacer una funcion en los jugadores 
            // para que desde opciones cambien el tipo de division
            playerCamera1.rect = new Rect(0, 0.5f, 1f, 0.5f);
            playerCamera2.rect = new Rect(0, 0, 1f, 0.5f);
        }
        return jugadoresRegistradosID++;
    }

    // Llama a esto cuando alguien muere o se acaba el tiempo del set
    public void RegistrarVictoriaSet(int idJugadorGanador)
    {
        if (idJugadorGanador == 1) scoreP1_Sets++;
        else scoreP2_Sets++;
        int idPerdedor = (idJugadorGanador == 1) ? 2 : 1;
        VerificarEstadoRonda(idJugadorGanador, idPerdedor);

    }

    private void VerificarEstadoRonda(int ultimoGanador, int ultimoPerdedor)
    {

        // Si alguien ha llegado al número de sets necesarios, la ronda termina y 
        if (scoreP1_Sets >= setsParaGanarRonda || scoreP2_Sets >= setsParaGanarRonda)
        {
            FinalizarRonda(ultimoGanador);
        }
        else
        {
            // La ronda sigue, solo reiniciamos posiciones para el siguiente set
            Debug.Log("Set terminado. Iniciando siguiente set...");
            ReiniciarArenaParaSiguienteSet();
            if (scoreP1_Sets > scoreP2_Sets)
            {
            // Abrir la UI de selección de perks para el perdedor del set para escoger una ventaja
            perkSelectorUI.InicializarSeleccion(ultimoPerdedor, ultimoGanador);
            }
        }

      
        
    }

    private void FinalizarRonda(int idGanadorRonda)
    {
        //Aqui puedes agregar lógica adicional para finalizar la ronda como la llamada auna funcion UI que muestre el ganador etc.
        Debug.Log($"Ronda {rondaActual} terminada. Ganador: Jugador {idGanadorRonda}");
        
        // Determinar perdedor
        int idPerdedor = (idGanadorRonda == 1) ? 2 : 1;

        // Abrir interfaz de selección para el perdedor
        if (Jugador1RondasGanadas >= rondasParaGanarPartida || Jugador2RondasGanadas >= rondasParaGanarPartida)
        {
            Debug.Log($"Partida terminada. Ganador final: Jugador {idGanadorRonda}");
            // Aquí puedes agregar lógica para finalizar la partida completa
        }
        else
        {
            if (idGanadorRonda == 1) Jugador1RondasGanadas++;
            else Jugador2RondasGanadas++;
        }  
        
        perkSelectorUI.InicializarSeleccion(idPerdedor, idGanadorRonda);

        // Resetear scores de sets para la nueva ronda
        scoreP1_Sets = 0;
        scoreP2_Sets = 0;
        rondaActual++;
    }

    public void ComenzarNuevaRonda()
    {
        // Lógica para limpiar el mapa y empezar de cero
        ReiniciarArenaParaSiguienteSet();
        Debug.Log("Nueva ronda comenzada.");
    }

    private void ReiniciarArenaParaSiguienteSet()
    {
        // Aquí se resetean posiciones, vida, munición, etc, habra que añadir lógica específica para cada uno por ejemplo jugador, objetos etc.
        // No olvidar suscribir a este evento desde el script que quiera hacer algo cuando termine un set.
        OnSetEnded?.Invoke();
    }
}