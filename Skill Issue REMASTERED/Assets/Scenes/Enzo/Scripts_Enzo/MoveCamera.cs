using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPos;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = cameraPos.position;
    }
}
