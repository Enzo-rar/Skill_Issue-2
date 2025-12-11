using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField]
    private GameObject _gameLogicObject;
     [SerializeField]
    private TextMeshProUGUI _UITextRondasSet;

    [Header("Configuración")]
    public int setsParaGanarRonda = 2; // Best of 3 (quien llegue a 2 gana)
    public int rondasParaGanarPartida = 2; // Best of 5 (quien llegue a 3 gana)
    public GameObject SpawnPlayerPrefab;
    [Header("Estado Actual")]
    public int scoreP1_Sets = 0;
    public int scoreP2_Sets = 0;
    public int rondaActual = 1;
    
    [Header("Mapas")]
    public List<Mapas> mapasDisponibles;
    private Mapas mapaActualData;
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

    [Header("Lobby Manager")]
    [SerializeField] private bool isLobby = false;
    [SerializeField] private float lobbyTimer = 5.0f;
    [SerializeField] private TextMeshProUGUI statusT;
    [TextArea(2,5)]
    public List <string> mensajesCarga;


    private void Start()
    {
        if(PlayerInputManager.instance != null)
        {
            PlayerInputManager.instance.onPlayerJoined += OnPlayerJoined;
            if (isLobby)
            {
                statusT.gameObject.SetActive(true);
                statusT.text = "Jugador 1, pulsa un botón para unirte a la partida";
            }
            else
            {
                statusT.gameObject.SetActive(false);
            }
        }

        
    }

    public void OnPlayerJoined(PlayerInput input)
    {
        Debug.Log("Ok so isLobby is " + isLobby);
        if(isLobby)
        {
            if(input.playerIndex == 0)
            {
                statusT.text = "Jugador 2, pulsa un botón para unirte a la partida";
            }
            else if(input.playerIndex == 1)
            {
                StartCoroutine(LobbyTimer());
            }
        }
    }

    private IEnumerator LobbyTimer()
    {
        statusT.text = "Todo listo, la partida comenzará en breves...";

        yield return new WaitForSeconds(1.0f);

        if (mensajesCarga != null && mensajesCarga.Count > 0)
        {
            int randID = UnityEngine.Random.Range(0, mensajesCarga.Count);
            statusT.text = mensajesCarga[randID];
        }

        yield return new WaitForSeconds(3.0f);

        float timer = lobbyTimer;
        while(timer > 0)
        {
            statusT.text = $"La partida comenzará en {Mathf.CeilToInt(timer)}";
            timer -= Time.deltaTime;
            yield return null;
        }

        isLobby = false;
        StartCoroutine(GenerarMapaSet());
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public int RegistrarJugador(PlayerInput playerInput, Camera camaraJugador, PlayerCharacter personaje = null)
    {
		Canvas canvasJugador = personaje.GetComponentInChildren<Canvas>();
		if (canvasJugador != null)
		{
			canvasJugador.renderMode = RenderMode.ScreenSpaceCamera;
			canvasJugador.worldCamera = camaraJugador;
			// Ajustar el "Plane Distance" para que no se corte con objetos 3D (opcional, prueba con 1)
			canvasJugador.planeDistance = 1;
		}
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
            playerCharacter1.dropWeapon();
            playerCharacter2.dropWeapon();
        }
        else
        {
            // La ronda sigue, solo reiniciamos posiciones para el siguiente set

            Debug.Log("Set terminado. Iniciando siguiente set...");
            _UITextRondasSet.text = $"Rondas: {Jugador1RondasGanadas} - {Jugador2RondasGanadas}   Sets: {scoreP1_Sets} - {scoreP2_Sets}";
            ReiniciarArenaParaSiguienteSet();
             perkSelectorUI.InicializarSeleccion(ultimoPerdedor, ultimoGanador);
            playerCharacter1.dropWeapon();
            playerCharacter2.dropWeapon();
            // Abrir la UI de selección de perks para el perdedor del set para escoger una ventaja
            // Aqui se puede cambiar la logica para que sea el que lleve menos sets ganados en total
           
            
            

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
            // Aquí puedes agregar lógica para finalizar la partida completa como llevarte a la pantalla de volver a jugar o menu principal
            _UITextRondasSet.text += $"HA GANADO EL JUGADOR {idGanadorRonda}!";
        }
        else
        {
            if (idGanadorRonda == 1) Jugador1RondasGanadas++;
            else Jugador2RondasGanadas++;
         
        
        perkSelectorUI.InicializarSeleccion(idPerdedor, idGanadorRonda);
        ComenzarNuevaRonda();
        // Resetear scores de sets para la nueva ronda
        scoreP1_Sets = 0;
        scoreP2_Sets = 0;
        rondaActual++;

        //Actualiza las rondas en el UI
        _UITextRondasSet.text = $"Rondas: {Jugador1RondasGanadas} - {Jugador2RondasGanadas}   Sets: {scoreP1_Sets} - {scoreP2_Sets}";
        } 
    }

    public void ComenzarNuevaRonda()
    {
        // Lógica para limpiar el mapa y empezar de cero
        ReiniciarArenaParaSiguienteSet();
        Debug.Log("Nueva ronda comenzada.");
    }

    private IEnumerator GenerarMapaSet()
    {
        // 1. Elegir un mapa aleatorio (evitando repetir el actual si es posible)
        Mapas nuevoMapa = null;
        
        if (mapasDisponibles.Count > 0)
        {
            List<Mapas> poolMapas = new List<Mapas>(mapasDisponibles);
            if (mapaActualData != null && poolMapas.Count > 1) 
            {
                poolMapas.Remove(mapaActualData); // Evitar repetir el mismo mapa seguido
            }
            nuevoMapa = poolMapas[UnityEngine.Random.Range(0, poolMapas.Count)];
        }

        if (nuevoMapa == null)
        {
            Debug.LogError("No hay mapas disponibles en el GameManager.");
            yield break;
        }

        // 2. Guardar referencia de la escena vieja para descargarla luego
        Scene escenaAntigua = SceneManager.GetActiveScene();
        Debug.Log("Escena antigua a descargar: " + escenaAntigua.path);
        Debug.Log("Cargando nuevo mapa: " + nuevoMapa.scenePath);
        // 3. Cargar la nueva escena de forma ADITIVA (para no borrar el GameManager ni los Players)
        AsyncOperation carga = SceneManager.LoadSceneAsync(nuevoMapa.scenePath, LoadSceneMode.Additive);
        
        // Esperar a que cargue
        while (!carga.isDone) yield return null;

        // 4. Configurar la nueva escena como activa
        Scene nuevaEscena = SceneManager.GetSceneByPath(nuevoMapa.scenePath);
        SceneManager.SetActiveScene(nuevaEscena);
        mapaActualData = nuevoMapa;

        // 5. Mover Jugadores y logica a la nueva escena
        // Si no cuando descarguemos la 'escenaAntigua' se borrarán
        if (playerCharacter1 != null) SceneManager.MoveGameObjectToScene(playerCharacter1.transform.root.gameObject, nuevaEscena);
        if (playerCharacter2 != null) SceneManager.MoveGameObjectToScene(playerCharacter2.transform.root.gameObject, nuevaEscena);
        if (_gameLogicObject != null){ 
            SceneManager.MoveGameObjectToScene(_gameLogicObject, nuevaEscena);
        }
        else
        {
            Debug.LogError("No tienes una referencia al GameLogic mira el inspector.");
        }
        // 6. Descargar la escena vieja (si es diferente a la nueva y es válida)
        if (escenaAntigua.IsValid() && escenaAntigua != nuevaEscena)
        {
           AsyncOperation operacionDescarga = SceneManager.UnloadSceneAsync(escenaAntigua);
           Debug.Log("Descargando escena antigua: " + escenaAntigua.path+" ... OperacionDescarga estado ->"+operacionDescarga);
           while (!operacionDescarga.isDone)
           {
                yield return null;
           }
        }

        // 7. Reposicionar Jugadores (Spawns)
        PosicionarJugadoresEnNuevaEscena();

        // 8. Avisar al resto de sistemas que el set ha terminado y que ejecuten su logica necesaria.
        // En un principio nadie depende de este evento, pero por si acaso.
        OnSetEnded?.Invoke();
        Debug.Log("Mapa cargado y jugadores posicionados.");
    }

    private void PosicionarJugadoresEnNuevaEscena()
    {
        // Buscar objetos vacíos en la escena nueva que tengan un tag RespawnPoint
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");

        if (spawns.Length >= 2)
        {
            // Mezclar spawns para que sea aleatorio quién sale dónde
            System.Random rnd = new System.Random();
            spawns = spawns.OrderBy(x => rnd.Next()).ToArray();

            if (playerCharacter1 != null) 
            {
                // Asumiendo que usas CharacterController o Rigidbody, a veces hay que desactivarlo para teletransportar
                playerCharacter1.RevivirJugadorSiguienteSet(spawns[0].transform);
                
            }

            if (playerCharacter2 != null) 
            {
                playerCharacter2.RevivirJugadorSiguienteSet(spawns[1].transform);
                
            }
        }
        else
        {
            Debug.LogError("No se encontraron suficientes puntos de spawn (Tag 'Respawn') en el mapa cargado.");
        }
    }
    

    private void ReiniciarArenaParaSiguienteSet()
    {
        // Aquí se resetean posiciones, vida, munición, etc, habra que añadir lógica específica para cada uno por ejemplo jugador, objetos etc.
        StartCoroutine(GenerarMapaSet());
        
    }

   /* public void IniciarJuego()
    {
        Debug.Log("Iniciando juego...");
        StartCoroutine(IniciarJuegoCoroutine());
       
    }

    private IEnumerator IniciarJuegoCoroutine()
    {
         Mapas mapaInicial = null;
         if (mapasDisponibles.Count > 0)
        {
            List<Mapas> poolMapas = new List<Mapas>(mapasDisponibles);
            if (mapaActualData != null && poolMapas.Count > 1) 
            {
                poolMapas.Remove(mapaActualData); // Evitar repetir el mismo mapa seguido
            }
            mapaInicial = poolMapas[UnityEngine.Random.Range(0, poolMapas.Count)];
        }


        // 2. Guardar referencia de la escena vieja para descargarla luego
        Scene escenaAntigua = SceneManager.GetActiveScene();
        Debug.Log("Escena antigua a descargar: " + escenaAntigua.path);
        Debug.Log("Cargando nuevo mapa: " + mapaInicial.scenePath);
        // 3. Cargar la nueva escena de forma ADITIVA (para no borrar el GameManager ni los Players)
        AsyncOperation carga = SceneManager.LoadSceneAsync(mapaInicial.scenePath, LoadSceneMode.Additive);
        
        // Esperar a que cargue
        while (!carga.isDone) yield return null;

        // 4. Configurar la nueva escena como activa
        Scene nuevaEscena = SceneManager.GetSceneByPath(mapaInicial.scenePath);
        SceneManager.SetActiveScene(nuevaEscena);
        mapaActualData = mapaInicial;

        // 5. Mover lógica a la nueva escena
        SceneManager.MoveGameObjectToScene(_gameLogicObject, nuevaEscena);

        // 6. Descargar la escena vieja (si es diferente a la nueva y es válida)
        if (escenaAntigua.IsValid() && escenaAntigua != nuevaEscena)
        {
           AsyncOperation operacionDescarga = SceneManager.UnloadSceneAsync(escenaAntigua);
           Debug.Log("Descargando escena antigua: " + escenaAntigua.path+" ... OperacionDescarga estado ->"+operacionDescarga);
           while (!operacionDescarga.isDone)
           {
                yield return null;
           }
        

        }

        //Spawnear jugadores
        GameObject InstantiatePlayer1 = Instantiate(SpawnPlayerPrefab);

         // Buscar objetos vacíos en la escena nueva que tengan un tag RespawnPoint
        GameObject[] spawns = GameObject.FindGameObjectsWithTag("Respawn");

        if (spawns.Length >= 2)
        {
            // Mezclar spawns para que sea aleatorio quién sale dónde
            System.Random rnd = new System.Random();
            spawns = spawns.OrderBy(x => rnd.Next()).ToArray();
        }


        

    }
    */
}