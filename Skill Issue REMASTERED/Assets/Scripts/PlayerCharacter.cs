using UnityEngine;
public class PlayerCharacter : MonoBehaviour
{
    private int _health;
    //private GameObject _armaEquipada;
    public GameObject _armaEquipada { get; private set; } //getter y setter automatico. Lectura publica pero escritora privada

    void Start()
    {
        _health = 100;
    }

    public void Hurt(int damage)
    {
        _health -= damage;
        Debug.Log("Health: " + _health);
    }
    public void SetItemEquipped(GameObject item)
    {
        _armaEquipada = item;
        Debug.Log("Arma equipada: " + _armaEquipada.name);
    }

    //public GameObject GetItemEquipped()
    //{
    //    return _armaEquipada;
    //}
}