using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayShooter : MonoBehaviour
{
    private Camera _camera;
    private float originalFOV;
    void Start()
    {
        _camera = GetComponent<Camera>();

        originalFOV = _camera.fieldOfView;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            _camera.fieldOfView = originalFOV / 2;
        } else
        {
            _camera.fieldOfView = originalFOV;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 point = new Vector3(_camera.pixelWidth / 2, _camera.pixelHeight / 2, 0);
            Ray ray = _camera.ScreenPointToRay(point);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                var hitObject = hit.transform.gameObject;
                var target = hitObject.GetComponent<ReactiveTarget>();
                if (target != null)
                {
                    //Debug.Log("Take that!");
                    target.ReactToHit();
                }
                else
                {
                    //Debug.Log("Hit " + hit.point + " (" + hit.transform.gameObject.name + ")");
                    StartCoroutine(SphereIndicator(hit.point));
                }
            }
        }
    }

    void OnGUI()
    {  // se ejecuta después de dibujar el frame del juego
        int size = 12;
        float posX = _camera.pixelWidth / 2 - size / 4;
        float posY = _camera.pixelHeight / 2 - size / 2;
        GUI.Label(new Rect(posX, posY, size, size), "*"); // puede mostrar texto e imágenes
    }

    private IEnumerator SphereIndicator(Vector3 pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = pos;
        Destroy(sphere.GetComponent<SphereCollider>());

        yield return new WaitForSeconds(5);

        Destroy(sphere);
    }

}

