using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{
    public Transform target;

    [Header("Zoom")]
    public float distance = 30f;
    public float zoomSpeed = 10f;
    public float minDistance = 5f;
    public float maxDistance = 30f;

    [Header("Rotation")]
    public float rotationSpeed = 5f;
    [Range(0.1f, 5f)] public float dragSensitivity = 1.5f;
    public float verticalAngle = 45f;
    public float horizontalAngle = 0f;
    public float minVerticalAngle = 20f;
    public float maxVerticalAngle = 80f;

    private Vector2 rotationVelocity;
    private Vector3 targetPosition;

    private Camera cam;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        if (target == null)
        {
            GameObject pivot = GameObject.Find("CameraPivot");
            if (pivot != null) target = pivot.transform;
        }

        if (target != null)
            targetPosition = target.position;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        UpdateCameraPosition();
    }

    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // Scroll zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.02f)
        {
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);
        }

        // Right-click rotation input
        if (Input.GetMouseButton(1))
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = -Input.GetAxis("Mouse Y");

            rotationVelocity.x = mouseX * rotationSpeed * dragSensitivity;
            rotationVelocity.y = mouseY * rotationSpeed * dragSensitivity;
        }
        else
        {
            rotationVelocity = Vector2.Lerp(rotationVelocity, Vector2.zero, Time.deltaTime * 5f);
        }

        // Update angles
        horizontalAngle += rotationVelocity.x;
        verticalAngle = Mathf.Clamp(verticalAngle + rotationVelocity.y, minVerticalAngle, maxVerticalAngle);

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (target == null) return;

        // Calculate rotation based on vertical + horizontal angles
        Quaternion rotation = Quaternion.Euler(verticalAngle, horizontalAngle, 0);
        Vector3 direction = rotation * Vector3.forward;

        // Position = target - direction * distance
        transform.position = target.position - direction * distance;
        transform.LookAt(target.position);
    }
}
