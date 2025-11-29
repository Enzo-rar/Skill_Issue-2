using UnityEngine;

public class AKScript : MonoBehaviour
{
    public WeaponClass weapon;
    public int AKdamage;
    public float AKbulletCooldown;
    public int AKnumberOfBullets;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AKdamage = 15;
        AKbulletCooldown = 0.1f;
        AKnumberOfBullets = 10;
        weapon = new WeaponClass(AKdamage, AKbulletCooldown, AKnumberOfBullets);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
