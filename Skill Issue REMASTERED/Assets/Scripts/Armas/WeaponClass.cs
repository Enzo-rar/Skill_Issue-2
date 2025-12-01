
using System.Linq;
using UnityEngine;

public class WeaponClass : MonoBehaviour
{
    public int damage;
    public int bulletXShot;
    public float bulletCooldown;
    public int numberOfBullets;
    public bool isAutomatic;

    [SerializeField] private GameObject fireballPrefab;
    private GameObject _fireball;
    private Transform bStartShoot;

    public void shootProjectile()
    {
        // Punto exacto frente a la cámara (1.5m delante)
        // Vector3 spawnPos = _camera.transform.position + _camera.transform.forward * 1.5f;
        
        Vector3 spawnPos;

        // Orientación igual a la cámara
        Quaternion spawnRot;
        fireballPrefab.GetComponent<Fireball>().SetDamage(damage);
        for (int i = 0; i < bulletXShot; i++)
        {
            bStartShoot = GetComponent<WeaponClass>().transform.Find("bulletExit" + i.ToString());
            spawnPos = bStartShoot.position;
            spawnRot = bStartShoot.rotation;
            _fireball = Instantiate(fireballPrefab, spawnPos, spawnRot);
        }
        //Debug.Log("Damage :" + fireballPrefab.GetComponent<Fireball>().damage);
    }

}
