using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 200f;
    public int damage = 25;

    void Update()
    {
        transform.Translate(0, 0, speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerCharacter player = other.GetComponentInParent<PlayerCharacter>();
        Debug.Log(player);
        if (player != null)
        {
            player.Hurt(damage);
        }
        Destroy(this.gameObject);
    }

}
