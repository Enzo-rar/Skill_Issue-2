using UnityEngine;

public class Slide : MonoBehaviour
{
    [Header("Slide Settings")]
    public float slideBoostMultiplier = 1.4f;
    public float slideDuration = 0.8f;
    public float slideHeight = 0.8f;
    public float normalHeight = 2f;
    public AnimationCurve slideCurve;

    [Header("Camera Settings")]
    public Transform playerCamera;
    public float cameraSlideOffset = 0.6f;
    public float cameraSmoothSpeed = 10f;

    [HideInInspector] public bool IsSliding;
    [HideInInspector] public Vector3 SlideVelocity;

    private float slideTimer;
    private CharacterController controller;
    private float originalCenterY;
    private Vector3 initialDir;
    private float initialSpeed;
    private Vector3 originalCameraLocalPos;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        originalCenterY = controller.center.y;

        if (slideCurve == null || slideCurve.length == 0)
            slideCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        if (playerCamera != null)
            originalCameraLocalPos = playerCamera.localPosition;
    }

    void Update()
    {
        UpdateSlide();
        UpdateCameraPosition();
    }

    public void TryStartSlide(Vector3 currentVelocity)
    {
        if (IsSliding) return;
        if (currentVelocity.magnitude < 0.1f) return;
        IsSliding = true;

        slideTimer = slideDuration;
        initialDir = currentVelocity.normalized;
        initialSpeed = currentVelocity.magnitude * slideBoostMultiplier;

        controller.height = slideHeight;
        controller.center = new Vector3(controller.center.x, slideHeight / 2, controller.center.z);
        Debug.Log("Slide is working");
    }

    public void UpdateSlide()
    {
        if (!IsSliding) return;

        slideTimer -= Time.deltaTime;

        float curveValue = slideCurve.Evaluate(1 - (slideTimer / slideDuration));
        float currentSpeed = Mathf.Lerp(initialSpeed, 0, curveValue);
        SlideVelocity = initialDir * currentSpeed;
        Debug.Log("Sliding with speed: " + currentSpeed + " Slide Timer: " + slideTimer);

        // Detener si acaba o suelta Shift
        if (slideTimer <= 0 || Input.GetKey(KeyCode.LeftShift) == false)
        {
            Debug.Log("Stopping Slide");
            StopSlide();
        }
    }

    public void StopSlide()
    {
        if (!IsSliding) return;
        Debug.Log("Slide Stopped");
        IsSliding = false;
        controller.height = normalHeight;
        controller.center = new Vector3(controller.center.x, originalCenterY, controller.center.z);
    }

    private void UpdateCameraPosition()
    {
        if (playerCamera == null) return;

        Vector3 targetPos = originalCameraLocalPos;

        if (IsSliding)
            targetPos.y -= cameraSlideOffset;

        playerCamera.localPosition = Vector3.Lerp(
            playerCamera.localPosition,
            targetPos,
            Time.deltaTime * cameraSmoothSpeed
        );
    }
}
