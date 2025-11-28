using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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
    
    [Header("Mapas")]
    public List<Mapas> mapasDisponibles;
   
    // Eventos para actualizar UI u otras cosas
    public UnityEvent<int> OnRoundEnded; // Pasa el ID del ganador (1 o 2)
    public UnityEvent OnSetEnded;


    // Referencia a la UI de selección de perks
    [SerializeField] private PerkSelectorUI perkSelectorUI;

    //Variables de los jugadores
    private int jugadoresRegistradosID = 1;
    [NonSerialized]
    public int Jugador1RondasGanadas = 0;
    [NonSerialized]
    public int Jugador2RondasGanadas = 0;

    [NonSerialized]
    public Camera playerCamera1;
    [NonSerialized]
    public Camera playerCamera2;
    
    //Referencias alcanzables globalmente desde otros scripts
    [NonSerialized]
    public PlayerInput playerInput1;
    [NonSerialized]
    public PlayerInput playerInput2;
    private PlayerCharacter playerCharacter1;
    private PlayerCharacter playerCharacter2;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int RegistrarJugador(PlayerInput playerInput, Camera camaraJugador, PlayerCharacter personaje = null)
    {   
        //Aqui basicamente solo queremos referencias necesarias en otros scripts como las camaras para la UI de perks
        if (jugadoresRegistradosID == 1)
        {
            playerCharacter1 = personaje;
            playerInput1 = playerInput;
            playerCamera1 = camaraJugador;
            
        }
        else if (jugadoresRegistradosID == 2)
        {
            playerCharacter2 = personaje;
            playerInput2 = playerInput;
            playerCamera2 = camaraJugador;
            //Configurar vista dividida vertical esto puede variar segun como quieras la division, se podria hacer una funcion en los jugadores 
            // para que desde opciones cambien el tipo de division
            playerCamera1.rect = new Rect(0, 0.5f, 1f, 0.5f);
            playerCamera2.rect = new Rect(0, 0, 1f, 0.5f);
        }
        return jugadoresRegistradosID++;
    }

    public PlayerCharacter GetPlayerById(int id)
    {
        if (id == 1)
        {
            return playerCharacter1;
        }
        else if (id == 2)
        {
            return playerCharacter2;
        }
        else
        {
            Debug.LogError("El jugador que buscas en GameManager con ID " + id + " no fue encontrado.");
            return null;
        }

    }

    // Esto lo llama el jugador que muere o cuando se acaba el tiempo del set
    // Preguntas: ¿Quién ganó? ¿Quién perdió?
    // Aqui se puede cambiar la logica para que el que se lleve la ventaja sea el que
    // menos sets ganados tenga en computo total, pero faltaria una variable adicional.
    // *** De momento se lo lleva el perdedor del set. ****
    public void RegistrarVictoriaSet(int idJugadorMuerto)
    {
        if (idJugadorMuerto == 1) scoreP2_Sets++;
        else scoreP1_Sets++;
        int idPerdedor = idJugadorMuerto;
        int idGanador = (idJugadorMuerto == 1) ? 2 : 1;
        VerificarEstadoRonda(idGanador, idPerdedor);
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
                
            // Abrir la UI de selección de perks para el perdedor del set para escoger una ventaja
            // Aqui se puede cambiar la logica para que sea el que lleve menos sets ganados en total
            perkSelectorUI.InicializarSeleccion(ultimoPerdedor, ultimoGanador);
            
            

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

    private Vector2[] GenerarMapaSet()
    {
        
        Vector2[] posicionesSpawnArray = new Vector2[2]; 
        // Aqui faltaria una llamada a una funcion que tenga la lista de Escenas de mapas creados
        // Sacar uno que no sea el actual y revisar que posiciones tiene como spawns posibles.
        // Se va a necesitar tener Scriptable objects que contengan la informacion:
        // Nombre mapa, Escena del mapa, posiciones spawn para ese mapa.
        
        // Guardar la escena en una variable para mas tarde usarla para cambiar de mapa en ReiniciarArena.
        Mapas mapaSeleccionado = mapasDisponibles[UnityEngine.Random.Range(0, mapasDisponibles.Count)];
        
        SceneManager.LoadScene(mapaSeleccionado.scenePath);
        return posicionesSpawnArray;

    }

    private void ReiniciarArenaParaSiguienteSet()
    {
        // Aquí se resetean posiciones, vida, munición, etc, habra que añadir lógica específica para cada uno por ejemplo jugador, objetos etc.
        // No olvidar suscribir a este evento desde el script que quiera hacer algo cuando termine un set.
        GenerarMapaSet();
        OnSetEnded?.Invoke();

        //Ahora cambiamos a la escena del nuevo mapa.
    }
}