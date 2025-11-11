using UnityEngine;

public class RecogerObjeto : MonoBehaviour
{
	public Camera playerCamera;
	public float interactRange = 3f;
	public Transform holdPoint;

	private ObjetoReactivo objetoEnMano;

	void Update()
	{
		//// Si presionamos E para recoger
		//if (Input.GetKeyDown(KeyCode.E))
		//{
		//	if (objetoEnMano == null)
		//	{
		//		//TryPickup();
		//	}
		//}

		//// Si presionamos Q para soltar
		//if (Input.GetKeyDown(KeyCode.Q))
		//{
		//	if (objetoEnMano != null)
		//	{
		//		objetoEnMano.ReactToDrop();
		//		objetoEnMano = null;
		//	}
		//}
	}

	//void TryPickup()
	//{
	//	Ray ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
	//	RaycastHit hit;

	//	if (Physics.Raycast(ray, out hit, interactRange))
	//	{
	//		var objeto = hit.collider.GetComponent<ObjetoReactivo>();
	//		if (objeto != null)
	//		{
	//			objeto.ReactToCollect(holdPoint);
	//			objetoEnMano = objeto;
	//		}
	//	}
	//}
}
