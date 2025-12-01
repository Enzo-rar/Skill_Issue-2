using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	[Header("Panels")]
	public GameObject panelMenu;
	public GameObject panelCreditos;
	public GameObject CreditosImagen;

	private bool creditosAbiertos = false;


	[Header("Nombre de la escena del juego")]
	public string primeraEscena = "CastilloFuncional";
	void Update()
	{
		if (creditosAbiertos)
		{
			// Si el usuario pulsa cualquier tecla o clic
			if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
			{
				CerrarCreditos();
			}
		}
	}

	// Llamado por el botón Jugar
	public void Jugar()
	{
		if (!string.IsNullOrEmpty(primeraEscena))
		{
			Debug.Log("Cargando escena: " + primeraEscena);
			SceneManager.LoadScene(primeraEscena);
		}
		else
		{
			Debug.LogWarning("PrimeraEscena no está configurada en MenuController.");
		}
	}
	public void AbrirCreditos()
	{
		panelCreditos.SetActive(true);
		creditosAbiertos = true;

	}

	public void CerrarCreditos()
	{
		panelCreditos.SetActive(false);
		creditosAbiertos = false;

	}
	// Llamado por el botón Créditos
	public void MostrarCreditos()
	{
		if (panelMenu != null && panelCreditos != null)
		{
			panelMenu.SetActive(false);
			panelCreditos.SetActive(true);
		}
	}

	// Llamado por el botón Volver
	public void VolverAlMenu()
	{
		if (panelMenu != null && panelCreditos != null)
		{
			panelCreditos.SetActive(false);
			panelMenu.SetActive(true);
		}
	}

	// Llamado por el botón Salir
	public void Salir()
	{	
		Debug.Log("Salir del juego (en editor no se cierra)");
		Application.Quit();
	}


}
