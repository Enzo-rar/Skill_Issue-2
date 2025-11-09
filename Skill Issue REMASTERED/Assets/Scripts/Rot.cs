using UnityEngine;

public class Rot : MonoBehaviour
{
    Vector3 speed = new Vector3(0, 100, 0);
    public Transform t;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        t.Rotate(speed * Time.deltaTime);
    }
}
