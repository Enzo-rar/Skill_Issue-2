using System.Collections;
using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 25;
    PlayerCam posicionCamara;

    void Start()
    {
	 StartCoroutine(DestroyAfter());

		//  posicionCamara = GetComponentInParent<PlayerCam>();
		//   transform.Translate(posicionCamara.transform.position.x, posicionCamara.transform.position.y, posicionCamara.transform.position.z);
	}
    void Update()
    {
        //posicionCamara = GetComponentInParent<PlayerCam>();
        transform.Translate(-5*Time.deltaTime,0, speed * Time.deltaTime);
        //transform.Translate(posicionCamara.transform.position.x, posicionCamara.transform.position.y, speed * Time.deltaTime);
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

    public void SetDamage(int dmg)
    {
        damage = dmg;
    }
	private IEnumerator DestroyAfter()
	{
		//Wait for set amount of time
		yield return new WaitForSeconds(2);
		//Destroy bullet object
		Destroy(gameObject);
	}
}
