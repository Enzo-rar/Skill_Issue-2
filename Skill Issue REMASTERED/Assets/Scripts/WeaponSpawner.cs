using UnityEngine;
using System.Collections;

public class WeaponSpawner : MonoBehaviour
{

    [SerializeField] private GameObject obj;
    [SerializeField] private Transform spawnPos;
    [SerializeField] private float spawntime = 10.0f;

    private GameObject currentObj;
    private Vector3 initialPos;
    private bool waiting = false;


    [Header("Animation")]
    [SerializeField] private float rotSpeed = 50f;
    [SerializeField] private float floatingH = 0.2f;
    [SerializeField] private float floatingS = 2.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Spawn();
    }

    // Update is called once per frame
    void Update()
    {
        if(waiting) return;

        else if (currentObj == null)
        {
            StartCoroutine(Respawn());
        }

        ObjetoReactivo reactivo = currentObj.GetComponent<ObjetoReactivo>();
        if(reactivo != null && reactivo.IsGrabbed())
        {
            currentObj = null;
            StartCoroutine(Respawn());
        }

        if(currentObj != null)
        {
            Dance();
        }
    }

    private void Dance()
    {
        currentObj.transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime, Space.World);

        float newY = initialPos.y + Mathf.Sin(Time.time * floatingS) * floatingH;

        currentObj.transform.position = new Vector3(currentObj.transform.position.x, newY, currentObj.transform.position.z);
    }

    private void Spawn()
    {
        currentObj = Instantiate(obj, spawnPos.position, spawnPos.rotation);

        initialPos = spawnPos.position;

        Rigidbody rb = currentObj.GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private IEnumerator Respawn()
    {
        waiting = true;
        yield return new WaitForSeconds(spawntime);
        Spawn();
        waiting = false;
    }
}
