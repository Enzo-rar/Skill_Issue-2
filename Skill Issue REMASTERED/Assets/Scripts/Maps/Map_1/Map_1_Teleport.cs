using UnityEngine;

public class Map_1_Teleport : MonoBehaviour
{
    public Transform dest;

    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();
        if (player != null)
        {
            player.transform
                .position = dest.position;
        }
    }
}
