using UnityEngine;
public class PlayerCharacter : MonoBehaviour
{
    [SerializeField] private int _health;
    //private GameObject _armaEquipada;
    public GameObject _armaEquipada { get; private set; } //getter y setter automatico. Lectura publica pero escritora privada
    public ParticleSystem deathParticles;
    void Start()
    {
        Debug.Log("Health: " + _health);
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
        // Debug.Log("Arma equipada: " + _armaEquipada.name);
    }

    private void Die()
    {
        if(deathParticles != null)
        {
            Instantiate(deathParticles, transform.position, Quaternion.identity);
        }

        Destroy(this.gameObject);
    }

    //public GameObject GetItemEquipped()
    //{
    //    return _armaEquipada;
    //}
}