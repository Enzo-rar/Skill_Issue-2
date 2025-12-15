using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PauseMenuController : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] private GameObject pauseRoot; // el Canvas o panel raíz del menú

	[Header("Escenas")]
	[SerializeField] private string mainMenuSceneName = "MainMenu";

	[Header("Cursor")]
	[SerializeField] private bool controlCursor = true;

	private bool isPaused;

	private void Awake()
	{
		SetPaused(false);
	}

	private void OnEnable()
	{
		// Solo en gameplay: si este objeto existe solo en la escena gameplay, ya cumples esto.
		// Si lo haces persistente, luego te digo cómo filtrar por escena.
	}

	public void OnPause(InputAction.CallbackContext ctx)
	{
		if (!ctx.performed) return;
		TogglePause();
	}

	public void TogglePause()
	{
		Debug.Log("[PauseMenuController] TogglePause()", this);

		SetPaused(!isPaused);
	}

	public void Resume()
	{
		SetPaused(false);
	}

	public void ExitToMainMenu()
	{
		SetPaused(false);
		SceneManager.LoadScene(mainMenuSceneName);
	}

	private void SetPaused(bool value)
	{
		isPaused = value;
		Debug.Log($"[PauseMenuController] SetPaused({isPaused}). pauseRoot={(pauseRoot != null ? pauseRoot.name : "NULL")}", this);

		if (pauseRoot != null)
			pauseRoot.SetActive(isPaused);

		Time.timeScale = isPaused ? 0f : 1f;

		if (controlCursor)
		{
			if (isPaused)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
			else
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}
	}
}
