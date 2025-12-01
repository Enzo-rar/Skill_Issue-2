using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
	[Header("Panels")]
	public GameObject panelMenu;
	
	public GameObject CreditosImagen;
	public GameObject ImagenMenu;

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


	public void CerrarCreditos()
	{
		ImagenMenu.SetActive(true);
		panelMenu.SetActive(true);
		creditosAbiertos = false;
	}
	// Llamado por el botón Créditos
	public void MostrarCreditos()
	{ print("Mostrar Créditos");
		panelMenu.SetActive(false);
		ImagenMenu.SetActive(false);
		creditosAbiertos = true;
	}

	// Llamado por el botón Volver
	public void VolverAlMenu()
	{
		if (panelMenu != null && CreditosImagen != null)
		{
			CreditosImagen.SetActive(false);
			panelMenu.SetActive(true);
		}
	}

	// Llamado por el botón Salir
	public void Salir()
	{	
		Debug.Log("Salir del juego (en editor no se cierra2)");
		Application.Quit();
	}


}
