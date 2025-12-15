
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Android;
using static UnityEngine.GraphicsBuffer;

public class WeaponClass : MonoBehaviour
{
    private Camera _camera;

    //public Arma arma;
    public int damage;
    public int bulletXShot;
    public float bulletCooldown;
    public int numberOfBullets;
    public bool isAutomatic;
    
    public Animator anim;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        //arma = new Arma(damage, bulletXShot, bulletCooldown, numberOfBullets, isAutomatic);
    }

    public void setCamera(Camera c)
    {
        _camera = c;
    }

    public void shootProjectile()
    {
        if (bulletXShot == 1)
        {
            Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var hitobject = hit.transform.gameObject;
                var target = hitobject.GetComponentInParent<PlayerCharacter>();
                if (target != null)
                {
                    target.Hurt(damage);
                }
            }
        }
        else
        {
            for (int i = 1; i <= bulletXShot; i++)
            {
                Vector3 angle = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0.0f);
                //Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Ray ray = new Ray(_camera.transform.position, Vector3.Normalize(_camera.transform.forward + angle));
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
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
    }
}
