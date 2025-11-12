using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class ObjetoReactivo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    //private bool _isCollected = false;
    private GameObject _armaEquipada;
    private GameObject _armaEnPedestal;
    private Collider _collider;
    private Rigidbody _rigidbody;
    private Camera _camara;

    void Awake()
    { // Guardamos referencias si existen
        _collider = GetComponent<Collider>();
        _rigidbody = GetComponent<Rigidbody>();
    }
    // Reacciona al ser recogido
    public void ReactToCollect(GameObject item, Camera camara)
    {
        Debug.Log("Metodo reactToCollect:", item);
        _camara = camara;

        // Desactivar f�sicas mientras est� equipado
        if (_collider != null)
            _collider.enabled = false;

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;
        }

        // Hacer hijo de la camara (equiparlo visualmente)
        transform.SetParent(camara.transform);
        transform.localPosition = new Vector3(0.5f, -0.3f, 0.8f); // posici�n �en mano�
        transform.localRotation = Quaternion.identity;


        //_armaEquipada = item;
        // Registrar el item en PlayerCharacter
        var player = camara.GetComponentInParent<PlayerCharacter>();
        if (player != null)
            player.SetItemEquipped(gameObject);

        // Destroy(_armaEquipada.GetComponent<BoxCollider>());

        var playerStats = camara.GetComponentInParent<PlayerCharacter>();
        if (playerStats != null) { playerStats.SetItemEquipped(item); }
        Debug.Log("Objeto recogido y equipado: " + gameObject.name);
    }

    //   // Reacciona al ser soltado, TP A DONDE APUNTAS
    //   public void ReactToDrop(GameObject item, Vector3 pos)
    //{
    //       //GameObject armaInstanciada = Instantiate(_armaEquipada, pos, Quaternion.identity);

    //       // Soltar: quitar de la camara
    //       transform.SetParent(null);

    //       // Colocar en el mundo ligeramente por encima del suelo
    //       transform.position = pos + Vector3.up * 0.2f;

    //       // Reactivar fisicas
    //       if (_collider != null)
    //           _collider.enabled = true;

    //       if (_rigidbody != null)
    //       {
    //           _rigidbody.isKinematic = false;
    //           _rigidbody.useGravity = true;
    //           _rigidbody.linearVelocity = Vector3.zero;
    //       }

    //       Debug.Log("Objeto dropeado: " + gameObject.name);
    //   }

    // Reacciona al ser soltado, LANZAR A DONDE APUNTAS

    public void ReactToDrop(GameObject item, Vector3 pos)
    {
        Debug.Log("Lanzando objeto " + gameObject.name);

        // Si quieres reutilizar el mismo objeto:
        transform.SetParent(null);
        transform.position = _camara.transform.position + _camara.transform.forward * 0.5f;
        transform.rotation = _camara.transform.rotation;

        // Reactivar físicas
        if (_collider != null) _collider.enabled = true;
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            _rigidbody.linearVelocity = Vector3.zero;

            //  Aplicar impulso hacia delante
            _rigidbody.AddForce(_camara.transform.forward * 8f, ForceMode.VelocityChange);
        }

        // Desregistrar el arma del jugador
        var player = _camara.GetComponentInParent<PlayerCharacter>();
        if (player != null) player.SetItemEquipped(null);
    }


}
