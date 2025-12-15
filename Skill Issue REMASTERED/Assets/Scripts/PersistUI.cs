using UnityEngine;

public class PersistUI : MonoBehaviour
{
	private void Awake()
	{
		// Si ya existe otro igual, destruye este (evita duplicados al volver al Lobby)
		var all = FindObjectsByType<PersistUI>(FindObjectsSortMode.None);
		if (all.Length > 1)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
	}
}
