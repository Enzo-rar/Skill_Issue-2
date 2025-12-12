
using System.Linq;
using UnityEngine;
using System.Collections;

public class WeaponClass : MonoBehaviour
{
    private Camera _camera;
    public int damage;
    public int bulletXShot;
    public float bulletCooldown;
    public int numberOfBullets;
    public bool isAutomatic;

    public Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }

    public void setCamera(Camera c)
    {
        _camera = c;
    }

    public void shootProjectile()
    {
        
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit) ) 
        {
            var hitobject = hit.transform.gameObject;
            var target = hitobject.GetComponentInParent<PlayerCharacter>();
            if (target != null)
            {
                target.Hurt(damage);
            }
        }
    }
}
