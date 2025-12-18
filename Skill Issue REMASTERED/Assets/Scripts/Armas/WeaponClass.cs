
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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

    public Transform firePoint;
    public LineRenderer tracerPrefab;
    public float velocidadTracer = 200f;
    public float longitudTracer = 2f;

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

            Vector3 tracerDest;

            if (Physics.Raycast(ray, out hit))
            {

                tracerDest = hit.point;

                var hitobject = hit.transform.gameObject;
                var target = hitobject.GetComponentInParent<PlayerCharacter>();
                if (target != null)
                {
                    target.Hurt(damage);
                }
            }
            else
            {
                tracerDest = ray.GetPoint(100f);
            }
            StartCoroutine(SpawnTracer(tracerDest));
        }
        else
        {
            for (int i = 1; i <= bulletXShot; i++)
            {
                Vector3 angle = new Vector3(Random.Range(-0.2f, 0.2f), Random.Range(-0.2f, 0.2f), 0.0f);
                //Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                Ray ray = new Ray(_camera.transform.position, Vector3.Normalize(_camera.transform.forward + angle));
                RaycastHit hit;

                Vector3 tracerDest;
                if (Physics.Raycast(ray, out hit))
                {
                    tracerDest = hit.point;
                    var hitobject = hit.transform.gameObject;
                    var target = hitobject.GetComponentInParent<PlayerCharacter>();
                    if (target != null)
                    {
                        target.Hurt(damage);
                    }
                }
                else
                {
                    tracerDest = ray.GetPoint(50f);
                }
                StartCoroutine(SpawnTracer(tracerDest));
            }
        }
    }

    private IEnumerator SpawnTracer(Vector3 hitPoint)
    {
        Vector3 startPosition = firePoint.position;
        Vector3 direction = (hitPoint - startPosition).normalized;
        

        float distanceToHit = Vector3.Distance(startPosition, hitPoint);

        
        LineRenderer tracer = Instantiate(tracerPrefab, startPosition, Quaternion.identity);
        tracer.positionCount = 2; 

        float distanceTravelled = 0f;

        
        while (distanceTravelled < distanceToHit)
        {
            
            distanceTravelled += velocidadTracer * Time.deltaTime;
            Vector3 headPosition = startPosition + (direction * distanceTravelled);

            
            float tailDistance = Mathf.Max(0, distanceTravelled - longitudTracer);
            Vector3 tailPosition = startPosition + (direction * tailDistance);

           
            if (distanceTravelled >= distanceToHit)
            {
                headPosition = hitPoint;
                // Si quieres que la bala se "aplaste" contra la pared, descomenta la siguiente linea:
                // tailPosition = hitPoint - (direction * Mathf.Min(longitudTracer, distanceToHit));
            }

           
            tracer.SetPosition(0, tailPosition); 
            tracer.SetPosition(1, headPosition); 

            
            yield return null;
        }

        
        Destroy(tracer.gameObject);
    }

}
