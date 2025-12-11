using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.InputSystem;
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] RayShooter rayShooter;
    [SerializeField] private PlayerSoundManager _SoundManager;
    [SerializeField] private int _baseHealth = 100;
    [SerializeField] private int _remainingHealth = 100;
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private Collider _collisionHandler;
    //private GameObject _armaEquipada;
    private GameObject _armaEquipada; //getter y setter automatico. Lectura publica pero escritora privada
    public ParticleSystem deathParticles;
    public int playerId = -1; // 1 o 2
    public bool canMove = true;
    public bool canShoot = true;
    public bool estaVivo = true;
   private HashSet<Ventajas> historialVentajas = new HashSet<Ventajas>();
								  // Variables de estado (flags)
	private bool tieneDobleSalto = false;
    private bool estaCegado = false;
    public float velocidadBase = 1f;
    private bool ventajaHP = false;
    public bool grabEnemyWeapon = false;

	[Header("UI")]
	[SerializeField] private Canvas playerCanvas;   // Canvas del prefab
	[SerializeField] private GameObject hudP1;      // raíz de HUD_P1
	[SerializeField] private GameObject hudP2;      // raíz de HUD_P2
	public HealthBarUI healthBar; // Referencia al componente HealthBarUI imagen

	void Start()
    {   
        _playerInput = GetComponentInParent<PlayerInput>();
        if(_playerInput != null){
        playerId = GameManager.Instance.RegistrarJugador(_playerInput,_playerCamera, this);
        bool checkForAudio = playerId == 1 ? false : true;
        Debug.Log("Player ID: " + playerId+" Check Audio: " + checkForAudio);

			// 1) Ligar el Canvas a la cámara de este jugador
			if (playerCanvas != null)
			{
				playerCanvas.renderMode = RenderMode.ScreenSpaceCamera;
				playerCanvas.worldCamera = _playerCamera;
				playerCanvas.planeDistance = 1f;
			}

			// 2) Activar solo el HUD que toca
			if (playerId == 1)
			{
				if (hudP1 != null) hudP1.SetActive(true);
				if (hudP2 != null) hudP2.SetActive(false);

				// Buscar la barra dentro de HUD_P1
				healthBar = hudP1.GetComponentInChildren<HealthBarUI>(true);
			}
			else if (playerId == 2)
			{
				if (hudP1 != null) hudP1.SetActive(false);
				if (hudP2 != null) hudP2.SetActive(true);

				// Buscar la barra dentro de HUD_P2
				healthBar = hudP2.GetComponentInChildren<HealthBarUI>(true);
			}

			// 3) Inicializar la barra con la vida actual
			if (healthBar != null)
			{
				healthBar.SetHealth(_remainingHealth, _baseHealth);
			}
			else
			{
				Debug.LogWarning($"healthBar es null en PlayerCharacter ID {playerId}");
			}


			if (checkForAudio)
        {
            DeactivateAudioForSecondPlayer();
        }
        }else
        {
            Debug.LogWarning("El player Input es nulo para el jugador con ID: " + playerId + ". Asegúrate de que su objeto Parent tenga componente Player Input.");
        }
        
        
        Debug.Log("Se ha unido el jugador-> " + playerId + " con Health: " + _remainingHealth);

        //_remainingHealth = _baseHealth;
        
    }
    public void dropWeapon()
    {
        if (rayShooter != null)
        {
            rayShooter.dropWeaponOnDeath();
        }
    }

    //Esto solo es para probar, luego se quita, cuidado por que probablemente se esta activando mas de una vez.
   /*
     private
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            GameManager.Instance.RegistrarVictoriaSet(playerId);
        }
    }
    */
    //Desactivar el componente de audio al segundo jugador para evitar problemas.
    private void DeactivateAudioForSecondPlayer()
    {
        AudioListener audioListener = _playerCamera.GetComponent<AudioListener>();
        if (audioListener != null)
        {
            audioListener.enabled = false;
            Debug.Log("AudioListener desactivado para el jugador con ID: " + playerId);
        }
        else
        {
            Debug.LogWarning("No se encontró AudioListener en el Camera para jugador de ID: " + playerId);
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
              Debug.Log("Vida aumentada en Ventaja +100");
              ventajaHP = true;
              _remainingHealth += 100;
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

        //Aqui le diremos al gameManager que me de la referencia al jugador que debuffo y le aplicaremos el efecto
        PlayerCharacter jugadorADebuffar = GameManager.Instance.GetPlayerById(playerId == 1 ? 1 : 2);
        Debug.Log("Jugador " + playerId + " debuffando al jugador " + (playerId == 2 ? 1 : 2) + " con debuff: " + tipo);
        if (jugadorADebuffar != null)
        {
            jugadorADebuffar.activaDebuffo(tipo);
        }
        else
        {
            Debug.LogWarning("No se encontró el jugador a debuffar con ID: " + (playerId == 1 ? 2 : 1));
        }
    }

    public void activaDebuffo(VentajaDebuff tipo)
    {
        switch (tipo)
        {
            case VentajaDebuff.Flashbang:
                // Llamamos a la corrutina de ceguera
                StartCoroutine(RutinaCeguera(3f));
                break;

            case VentajaDebuff.Lentitud:

                velocidadBase *= 0.2f;
                break;
                
            case VentajaDebuff.ReduccionVida:
              
                _remainingHealth -= 50;
                Debug.Log("Vida reducida en Desventaja -50: " + _remainingHealth);
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
        _SoundManager.playHitSound();
		if (healthBar != null)
			healthBar.SetHealth(_remainingHealth, _baseHealth);
		if ( _remainingHealth <= 0 && estaVivo)
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
        canMove = false;
        estaVivo = false;
        canShoot = false;
        _SoundManager.playDeathSound();
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

    void deshazVentajas()
    {
        //Aqui se desharian las ventajas al final de cada set, si es que tienen duracion limitada.
        tieneDobleSalto = false;
        estaCegado = false;
        ventajaHP = false;
        _remainingHealth = _baseHealth;
        velocidadBase = 1f;
        
    }
    public void RevivirJugadorSiguienteSet(Transform respawnPoint)
{
    canMove = true;
    canShoot = true;
    estaVivo = true;
    
    
    //Elimina ventajas temporales
    deshazVentajas();


    Debug.Log("Jugador " + playerId + " reviviendo en: " + respawnPoint.position);

    Transform jugadorRoot = transform.root;
    Rigidbody rb = GetComponent<Rigidbody>(); 
    

    // --- 2. DESACTIVAR RIGIDBODY ---
    if (rb != null)
    {
        
        rb.isKinematic = true; 
        rb.linearVelocity = Vector3.zero; 
        rb.angularVelocity = Vector3.zero;
        rb.Sleep(); // Pone el RB a dormir para que deje de calcular colisiones
    }

    // --- 3. TELETRANSPORTAR ---
    
    //  Mover Transform
    jugadorRoot.position = respawnPoint.position;
    //jugadorRoot.rotation = respawnPoint.rotation;

    //  Mover Rigidbody explícitamente 
    if (rb != null)
    {
        rb.position = respawnPoint.position;
       // rb.rotation = respawnPoint.rotation;
    }
    
    //  Corregir posición local si es necesario
    if (jugadorRoot != transform) 
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    // --- 4. FORZAR SINCRONIZACIÓN DE FÍSICAS ---
    
    Physics.SyncTransforms(); 


    // --- 5. REACTIVAR ---
    if (rb != null)
    {
        rb.isKinematic = false;
        rb.WakeUp(); // Despertar el RB
    }

    
    
    Debug.Log("Teletransporte completado a: " + transform.position);
}

    }

   

