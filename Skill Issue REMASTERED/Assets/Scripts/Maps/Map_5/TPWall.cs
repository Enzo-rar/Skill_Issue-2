using NUnit.Framework.Internal;
using UnityEngine;

public class TPWall : MonoBehaviour
{
    public Vector3 desplazamiento;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();
        if (player != null)
        {
            player.transform.position += desplazamiento;
        }
    }
}
