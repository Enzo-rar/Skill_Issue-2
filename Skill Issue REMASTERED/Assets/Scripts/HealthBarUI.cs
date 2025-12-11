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
}

