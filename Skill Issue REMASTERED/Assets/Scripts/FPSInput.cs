using UnityEngine;

[RequireComponent(typeof(CharacterController))] // obliga a que el GameObject tenga cierto componente
public class FPSInput : MonoBehaviour
{
	[Header("Movimiento")]
	public float speed = 6.0f;
	[Header("Salto y gravedad")]
	public float jumpHeight = 2.5f;
	public float gravity = -9.8f;

	public float fallMultiplier = 1.5f;   // multiplica gravedad al CAER
	public float jumpCutMultiplier = 0.5f; // recorta salto al soltar Jump
	//public float maxFallSpeed = -50f;     // límite de caída 

	private CharacterController _charController;
	private Vector3 velocity;
	//private float verticalVelocity;

	private bool isGrounded;

	void Start()
    {
        _charController = GetComponent<CharacterController>();
    }

	void Jump()
	{
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void Update()
    {
		// --- Movimiento del plano horizontal ---

		float deltaX = Input.GetAxis("Horizontal"); // Las teclas asociadas están en:
        float deltaZ = Input.GetAxis("Vertical");   // Edit\Project Settings\Input
       // Vector3 movement = new Vector3(deltaX, 0, deltaZ);
		Vector3 move = transform.right * deltaX + transform.forward * deltaZ;
		//movement = Vector3.ClampMagnitude(movement, 1.0f) * speed;
		move = Vector3.ClampMagnitude(move, 1f) * speed;
		//movement.y = gravity;

		// --- Estado del suelo ---
		isGrounded = _charController.isGrounded;

		if (isGrounded && velocity.y < 0)
			velocity.y = -2f; // mantener pegado al suelo

		// --- Salto ---
		if (Input.GetButtonDown("Jump") && isGrounded)
		{
			Jump();
            //velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

		// --- Gravedad ---

		//velocity.y += gravity * Time.deltaTime;
		// más gravedad al caer
		if (velocity.y < 0f)
			velocity.y += gravity * fallMultiplier * Time.deltaTime;
		else
			velocity.y += gravity * Time.deltaTime;

		// cortar salto si sueltas el botón mientras subes
		if (!isGrounded && !Input.GetButton("Jump") && velocity.y > 0f)
			velocity.y *= jumpCutMultiplier;

		velocity.y += gravity * Time.deltaTime;


		// --- Movimiento total ---
		Vector3 finalMove = move + Vector3.up * velocity.y; 

		_charController.Move(finalMove * Time.deltaTime);

		//movement = transform.TransformDirection(movement); // convierte desde el sistema local al global
       // _charController.Move(movement * Time.deltaTime); // no movemos el transform para que se calculen
    }						// las colisiones
}

