using UnityEngine;

public class LimiteMapa : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("En el límite");
        PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();
        Debug.Log(player);
        if (player != null)
        {
            player.Hurt(999);
        }
        //Destroy(this.gameObject);
    }
}
