using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuSceneManager : MonoBehaviour
{

	public Button defaultButton;
	public GameObject canvasCredits;
	public GameObject canvasControls;

	void Update()
	{
		if (canvasCredits.activeSelf || canvasControls.activeSelf) 
		{
			if (Input.anyKeyDown)     
			{
				canvasCredits.SetActive(false);
				canvasControls.SetActive(false);

			}
		}
	}

	void Start()
	{
		EventSystem.current.SetSelectedGameObject(defaultButton.gameObject);
	}

	public void StartGame()
	{
		SceneManager.LoadScene("TestJose");
	}

	public void QuitGame()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
	}


	public void OnEnterName(string playerName)
	{
		Debug.Log("PlayerName: " + playerName);
	}

	public void OnSFXVolume(float volume)
	{
		Debug.Log("Volumen de Efectos: " + volume);
	}

	public void OnMusicVolume(float volume)
	{
		Debug.Log("Volumen de Música: " + volume);
	}
}
