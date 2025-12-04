using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SplashScreenController : MonoBehaviour
{
	[Header("Scenes")]
	[Tooltip("Nombre de la escena a cargar tras la splash")]
	public string nextSceneName = "Menu";

	[Header("Timing")]
	[Tooltip("Segundos antes de pasar automáticamente")]
	public float waitTime = 3f;

	[Header("Fade (opcional)")]
	[Tooltip("CanvasGroup para controlar el fade. Dejar null para no usar fade.")]
	public CanvasGroup canvasGroup;
	[Tooltip("Duración del fade out en segundos (si hay canvasGroup)")]
	public float fadeDuration = 0.5f;

	bool loading = false;

	void Start()
	{
		// Si hay CanvasGroup, asegúrate de empezar opaco (alpha = 1)
		if (canvasGroup != null)
			canvasGroup.alpha = 1f;

		StartCoroutine(AutoAdvance());
	}

	void Update()
	{
		// Saltar si se pulsa cualquier tecla o se hace click/touch
		if (!loading && (Input.anyKeyDown || Input.GetMouseButtonDown(0) || IsTouchBegan()))
		{
			StartCoroutine(LoadNextScene());
		}
	}

	IEnumerator AutoAdvance()
	{
		float t = 0f;
		while (t < waitTime)
		{
			// Si en algún frame se presiona tecla/click, Update() lo manejará
			t += Time.unscaledDeltaTime; // usa unscaled para no depender de timeScale
			yield return null;
		}

		if (!loading)
			StartCoroutine(LoadNextScene());
	}

	IEnumerator LoadNextScene()
	{	
		loading = true;

		// Fade out si hay CanvasGroup
		if (canvasGroup != null && fadeDuration > 0f)
		{
			float t = 0f;
			float start = canvasGroup.alpha;
			while (t < fadeDuration)
			{
				t += Time.unscaledDeltaTime;
				canvasGroup.alpha = Mathf.Lerp(start, 0f, t / fadeDuration);
				yield return null;
			}
			canvasGroup.alpha = 0f;
		}

		// Carga asíncrona (opcional: podrías mostrar una barra de carga)
		AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);
		op.allowSceneActivation = true;
		while (!op.isDone)
		{
			yield return null;
		}
	}

	bool IsTouchBegan()
	{
		if (Input.touchCount > 0)
		{
			Touch t = Input.GetTouch(0);
			return t.phase == TouchPhase.Began;
		}
		return false;
	}
}
