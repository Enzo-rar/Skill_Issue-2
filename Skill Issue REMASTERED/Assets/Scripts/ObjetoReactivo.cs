using UnityEngine;

public class ObjetoReactivo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
 
	private bool _isCollected = false;
	// Reacciona al ser recogido
	public void ReactToCollect(Transform holdPoint)
	{
		if (_isCollected) return;

		_isCollected = true;
		GetComponent<Rigidbody>().isKinematic = true;
		transform.SetParent(holdPoint);
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
	}

	// Reacciona al ser soltado
	public void ReactToDrop()
	{
		if (!_isCollected) return;

		_isCollected = false;
		transform.SetParent(null);
		GetComponent<Rigidbody>().isKinematic = false;
	}

}
