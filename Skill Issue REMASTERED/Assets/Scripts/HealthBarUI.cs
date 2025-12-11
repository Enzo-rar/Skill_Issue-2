using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
	[SerializeField] private Image healthFill;

	// current y max vienen de tu sistema de vida
	public void SetHealth(float current, float max)
	{
		float value = current / max;
		healthFill.fillAmount = Mathf.Clamp01(value);
	}
	public void ColocarComoJugador2()
	{
		RectTransform rt = GetComponent<RectTransform>();
		if (rt == null) return;

		// Opción simple: mismo X, cambiar Y
		Vector2 pos = rt.anchoredPosition;
		pos.y = -590f;              // tu valor
		rt.anchoredPosition = pos;

		// (versió con anchors abajo) sin testear
		//rt.anchorMin = new Vector2(0, 0);
		//rt.anchorMax = new Vector2(0, 0);
		//rt.anchoredPosition = new Vector2(50f, 50f);
	}
}

