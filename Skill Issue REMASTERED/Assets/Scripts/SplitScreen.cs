using UnityEngine;

public class SplitScreen : MonoBehaviour
{
    public int playerIndex; // asignado automáticamente por PlayerInput
private Camera cam;

void Awake()
{
    cam = GetComponentInChildren<Camera>();
    if (playerIndex == 0) // Jugador 1
        cam.rect = new Rect(0f, 0f, 0.5f, 1f);
    else // Jugador 2
        cam.rect = new Rect(0.5f, 0f, 0.5f, 1f);
}

}
