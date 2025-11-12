using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerCameraSetup : MonoBehaviour
{
    private Camera playerCamera;
    private PlayerInput playerInput;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerCamera = GetComponentInChildren<Camera>();

        if (playerCamera == null)
        {
            Debug.LogError("No se encontró cámara en el prefab del jugador.");
            return;
        }

        // Split-screen horizontal para 2 jugadores
        switch (playerInput.playerIndex)
        {
            case 0: // Jugador 1
                playerCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
                break;
            case 1: // Jugador 2
                playerCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
                break;
            default: // más jugadores
                playerCamera.rect = new Rect(0f, 0f, 1f, 1f);
                break;
        }
    }
}
