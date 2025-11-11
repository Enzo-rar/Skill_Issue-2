using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Crouch : MonoBehaviour
{

    private Vector3 originalSize;
    private float originalMoveSpeed;
    private PlayerMovement _playerMovement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        originalSize = GetComponent<Transform>().localScale;
        originalMoveSpeed = _playerMovement.moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            transform.localScale = new Vector3(originalSize.x, originalSize.y*60/100, originalSize.z);
            _playerMovement.moveSpeed = originalMoveSpeed/2;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            transform.localScale = originalSize;
            _playerMovement.moveSpeed = originalMoveSpeed;
        }
    }
}
