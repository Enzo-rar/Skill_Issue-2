using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
	[Header("UI")]
	[SerializeField] private GameObject pauseRoot; // el Canvas o panel raíz del menú
	[SerializeField] private GameObject firstSelected; // PlayBtn
	[SerializeField] private GameObject canvasControls; // Canvas de controles


	[Header("Escenas")]
	[SerializeField] private string mainMenuSceneName = "MainMenu";

	[Header("Cursor")]
	[SerializeField] private bool controlCursor = true;

	private bool isPaused;

	private void Awake()
	{
		SetPaused(false);

	}
	private void Update()
	{
		if (canvasControls != null && canvasControls.activeSelf)
		{
			// teclado/mouse
			if (Input.anyKeyDown)
			{
				canvasControls.SetActive(false);
				return;
			}
		}
		// asegurar que va el mando
		var gp = UnityEngine.InputSystem.Gamepad.current;
		if (gp != null && (gp.aButton.wasPressedThisFrame || gp.bButton.wasPressedThisFrame ||
						   gp.startButton.wasPressedThisFrame || gp.dpad.up.wasPressedThisFrame ||
						   gp.dpad.down.wasPressedThisFrame || gp.leftStickButton.wasPressedThisFrame))
		{
			canvasControls.SetActive(false);
		}

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
	public void OpenControlsOverlay()
	{
		Debug.Log("[PauseMenuController] OpenControlsOverlay()", this);
		canvasControls.SetActive(true);
	}

	public void ExitToMainMenu()
	{
		// fuerza estado normal sí o sí
		Time.timeScale = 1f;

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		if (pauseRoot != null)
			pauseRoot.SetActive(false);

		Destroy(transform.root.gameObject); //añadido

		SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single); // loadscenemode.single añadido
	}

	private void SetPaused(bool value)
	{
		isPaused = value;

		if (pauseRoot != null)
			pauseRoot.SetActive(isPaused);

		Time.timeScale = isPaused ? 0f : 1f;

		if (controlCursor)
		{
			if (isPaused)
			{
				if (firstSelected != null)
					EventSystem.current.SetSelectedGameObject(firstSelected);
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
			}
			else
			{
				EventSystem.current.SetSelectedGameObject(null);

				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
		}
	}
}
