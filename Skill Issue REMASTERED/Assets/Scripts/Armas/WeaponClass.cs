
using UnityEngine;

public class WeaponClass : MonoBehaviour
{
    public int damage;
    public float bulletCooldown;
    public int numberOfBullets;


    public WeaponClass()
    {
        
    }
    public WeaponClass(int dmg, float bCooldown, int numBullets)
    {
        damage = dmg;
        bulletCooldown = bCooldown;
        numberOfBullets = numBullets;
    }

}
