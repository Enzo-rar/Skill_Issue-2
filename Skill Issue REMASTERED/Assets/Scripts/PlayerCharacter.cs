using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int _health;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Camera _playerCamera;
    //private GameObject _armaEquipada;
    private GameObject _armaEquipada; //getter y setter automatico. Lectura publica pero escritora privada
    public ParticleSystem deathParticles;
    public int playerId = -1; // 1 o 2
    
   private HashSet<Ventajas> historialVentajas = new HashSet<Ventajas>();

    // Variables de estado (flags)
    private bool tieneDobleSalto = false;
    private bool estaCegado = false;
    private float velocidadBase = 5f;

     void Start()
    {   
        _playerInput = GetComponentInParent<PlayerInput>();
        if(_playerInput != null){
        playerId = GameManager.Instance.RegistrarJugador(_playerInput,_playerCamera);
        }
        else
        {
            Debug.LogWarning("El player Input es nulo para el jugador con ID: " + playerId + ". Asegúrate de que su objeto Parent tenga componente Player Input.");
        }
        
        
        Debug.Log("Se ha unido el jugador-> " + playerId + " con Health: " + _health);
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
              _health += 50;
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
                _health -= 20;
                Debug.Log("Vida reducida en Desventaja -20: " + _health);
                break;

        
        }
    }

    // Corutina para manejar el efecto de ceguera
    private System.Collections.IEnumerator RutinaCeguera(float duracion)
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
        Debug.Log("Health: " + _health);
        _health = _health - damage;
        Debug.Log("Health: " + _health);
        if( _health <= 0)
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
        StartCoroutine(ActionsAfterDeath());

        
    }

    IEnumerator ActionsAfterDeath()
    {
        // Basicamente podemos dejar un tiempo para ver las particulas de muerte o animaciones, o hacer otras cosas para celebrar la muerte
        yield return new WaitForSeconds(3f);
        Debug.Log("Jugador " + playerId + " ha muerto -> Procedo a llamar a RegistrarVictoriaSet en GameManager");
        GameManager.Instance.RegistrarVictoriaSet(playerId);
        if(deathParticles != null) deathParticles.Play();
        
        // Aqui ponemos logica destruccion del objeto jugador? al igual es mejor manejarlo desde el GameManager directamente
        // Resetear el mapa poner otro o reiniciar posiciones
        //Destroy(this.gameObject);
    }
   
}