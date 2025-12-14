using UnityEngine;

public class Cam_rotate : MonoBehaviour
{
    public float speed = 5.0f;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.Rotate(0, speed, 0);
    }
}
