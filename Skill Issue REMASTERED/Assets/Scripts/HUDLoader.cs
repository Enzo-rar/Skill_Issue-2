using UnityEngine;

public class HUDLoader : MonoBehaviour
{
	public GameObject hudPrefab;
	private static GameObject hudInstance;

	void Awake()
	{
		if (hudInstance == null)
		{
			hudInstance = Instantiate(hudPrefab);
			DontDestroyOnLoad(hudInstance);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
