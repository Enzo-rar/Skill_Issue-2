using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int _baseHealth = 100;
    [SerializeField] private int _remainingHealth = 1;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Collider _collisionHandler;
    //private GameObject _armaEquipada;
    private GameObject _armaEquipada; //getter y setter automatico. Lectura publica pero escritora privada
    public ParticleSystem deathParticles;
    public int playerId = -1; // 1 o 2
    public bool estaVivo = true;
   private HashSet<Ventajas> historialVentajas = new HashSet<Ventajas>();

    // Variables de estado (flags)
    private bool tieneDobleSalto = false;
    private bool estaCegado = false;
    private float velocidadBase = 5f;

    private 

     void Start()
    {   
        _playerInput = GetComponentInParent<PlayerInput>();
        if(_playerInput != null){
        playerId = GameManager.Instance.RegistrarJugador(_playerInput,_playerCamera, this);
        }
        else
        {
            Debug.LogWarning("El player Input es nulo para el jugador con ID: " + playerId + ". Asegúrate de que su objeto Parent tenga componente Player Input.");
        }
        
        
        Debug.Log("Se ha unido el jugador-> " + playerId + " con Health: " + _remainingHealth);

        //_remainingHealth = _baseHealth;
        if (GameManager.Instance != null)
        {
           //Esta seria la forma alternativa de Respawnear al jugador desde el GameManager
           // GameManager.Instance.OnSetEnded.AddListener(RevivirJugadorSiguienteSet);
           
        }
        else
        {
            Debug.LogWarning("GameManager Instance es nulo en PlayerCharacter Start(). Podria deberse al orden de creacion de los objetos.");
        }
    }

    //Esto solo es para probar, luego se quita
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameManager.Instance.RegistrarVictoriaSet(playerId);
        }
    }

    // --- PUNTO DE ENTRADA UNICO ---
    public void ActivarVentaja(Ventajas nuevaVentaja)
    {
        // 1. Guardamos la referencia (útil para UI, estadísticas o lógica persistente)
        if (!historialVentajas.Contains(nuevaVentaja))
        {
            historialVentajas.Add(nuevaVentaja);
        }

        // 2. El "Cerebro" que decide qué hacer con el objeto recibido
        ProcesarEfecto(nuevaVentaja);
    }

    private void ProcesarEfecto(Ventajas data)
    {
        // PRIORIDAD 1: ¿Es algo malo (Debuff)?
        if (data.tipoDebuff != VentajaDebuff.Ninguna)
        {
            AplicarLogicaDebuff(data.tipoDebuff);
        }
        // PRIORIDAD 2: ¿Es algo bueno (Buff)?
        else if (data.tipoFavorable != VentajaFavorable.Ninguna)
        {
            AplicarLogicaBuff(data.tipoFavorable);
        }
        else
        {
            Debug.LogWarning($"La ventaja {data.nombreVentaja} no tiene tipo definido.");
        }
    }

    // --- LÓGICA INTERNA DE BUFFS ---
    private void AplicarLogicaBuff(VentajaFavorable tipo)
    {
        switch (tipo)
        {
            case VentajaFavorable.DobleSalto:
                 // Activar doble salto en el sistema de movimiento
                tieneDobleSalto = true;
                Debug.Log("Jugador: Doble Salto Activado");
                break;

            case VentajaFavorable.MasVida:
                //Aumentar vida en 50, al igual vale la pena hacer un método en HealthSystem para esto y separar este script para ventajas y quizas armas
              //  GetComponent<HealthSystem>().Curar(50);
              Debug.Log("Vida aumentada en Ventaja +50");
              _remainingHealth += 50;
                break;
                
            case VentajaFavorable.Velocidad:
                //Conectar con el sistema de movimiento para aumentar la velocidad
                velocidadBase *= 1.5f;
                break;
                
          
        }
    }

    // --- LÓGICA INTERNA DE DEBUFFS ---
    private void AplicarLogicaDebuff(VentajaDebuff tipo)
    {
        switch (tipo)
        {
            case VentajaDebuff.Flashbang:
                // Llamamos a la corrutina de ceguera
                StartCoroutine(RutinaCeguera(3f));
                break;

            case VentajaDebuff.Lentitud:
                velocidadBase *= 0.5f;
                break;
                
            case VentajaDebuff.ReduccionVida:
              //  GetComponent<HealthSystem>().RecibirDañoDirecto(20);
                _remainingHealth -= 20;
                Debug.Log("Vida reducida en Desventaja -20: " + _remainingHealth);
                break;

        
        }
    }

    // Corutina para manejar el efecto de ceguera
    private IEnumerator RutinaCeguera(float duracion)
    {
        estaCegado = true;
        // Aquí poner como funciona la ceguera visualmente, por ejemplo con un overlay en blanco para flashear la pantalla
        Debug.Log("Jugador CEGADO");
        yield return new WaitForSeconds(duracion);
        estaCegado = false;
        Debug.Log("Jugador RECUPERADO");
    }


   
    public void Hurt(int damage)
    {
        _remainingHealth = _remainingHealth - damage;
        Debug.Log("Jugador " + playerId + " ha recibido daño: " + damage + " Vida restante: " + _remainingHealth);
        if( _remainingHealth <= 0 && estaVivo)
        {
            Die();
        }
    }
    public void SetItemEquipped(GameObject item)
    {
        _armaEquipada = item;
       
    }

    public GameObject GetItemEquipped()
    {
        return _armaEquipada;
    }

    private void Die()
    {
        //Emepzar corrutina de muerte 
        //estaVivo a false permite que no salte de nuevo la funcion de morir al disparar cadaver.
        estaVivo = false;
        StartCoroutine(ActionsAfterDeath());

        
    }


    IEnumerator ActionsAfterDeath()
    {
        // Basicamente podemos dejar un tiempo para ver las particulas de muerte o animaciones, o hacer otras cosas para celebrar la muerte
        if(deathParticles != null) deathParticles.Play();
        yield return new WaitForSeconds(3f);
        Debug.Log("Jugador " + playerId + " ha muerto -> Procedo a llamar a RegistrarVictoriaSet en GameManager");
        GameManager.Instance.RegistrarVictoriaSet(playerId);
        
        
        // Aqui ponemos logica destruccion del objeto jugador? al igual es mejor manejarlo desde el GameManager directamente
        // Resetear el mapa poner otro o reiniciar posiciones
        //Destroy(this.gameObject);
    }

    public void RevivirJugadorSiguienteSet(Transform respawnPoint)
    {
        estaVivo = true;
        // En caso de una ventaja para aumentar HP cambiar _baseHealth antes de revivir
        _remainingHealth = _baseHealth; 
        Debug.Log("Jugador " + playerId + " ha sido revivido con vida: " + _remainingHealth);
        // Aqui faltaria resetear las animaciones,eliminar las ventajas anteriores, etc.
        GetComponentInParent<Transform>().position = respawnPoint.position;
        GetComponentInParent<Transform>().rotation = respawnPoint.rotation;
    }
   
}
