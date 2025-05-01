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

    [Header("Pan Settings")]
    [Tooltip("How fast the pivot moves to the new plot. Higher => snappier.")]
    public float panSpeed = 8f;

    private Vector2 rotationVelocity;
    private Vector3 targetPosition;
    private Camera cam;

    void Start()
    {
        cam = GetComponentInChildren<Camera>();
        if (target == null)
        {
            var pivotGO = GameObject.Find("CameraPivot");
            if (pivotGO != null)
                target = pivotGO.transform;
        }

        // initialize our stored destination
        if (target != null)
            targetPosition = target.position;

        distance = Mathf.Clamp(distance, minDistance, maxDistance);
        UpdateCameraPosition();
    }

    /// <summary>
    /// Call this to tell the camera where it should move its pivot to.
    /// </summary>
    public void SetTargetPosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    void LateUpdate()
    {
        if (target == null || cam == null) return;

        // 0) PAN the pivot toward our stored destination:
        // ——————————————————————————————————————————————

        // Instant snap (uncomment to use):
        // target.position = targetPosition;

        // Smooth pan (uncomment to use):
        target.position = Vector3.Lerp(
            target.position,
            targetPosition,
            Time.deltaTime * panSpeed
        );

        // 1) Zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.02f)
            distance = Mathf.Clamp(distance - scroll * zoomSpeed, minDistance, maxDistance);

        // 2) Rotation on right-drag
        if (Input.GetMouseButton(1))
        {
            float mX = Input.GetAxis("Mouse X");
            float mY = -Input.GetAxis("Mouse Y");
            rotationVelocity.x = mX * rotationSpeed * dragSensitivity;
            rotationVelocity.y = mY * rotationSpeed * dragSensitivity;
        }
        else
        {
            rotationVelocity = Vector2.Lerp(rotationVelocity, Vector2.zero, Time.deltaTime * 5f);
        }

        horizontalAngle += rotationVelocity.x;
        verticalAngle = Mathf.Clamp(verticalAngle + rotationVelocity.y, minVerticalAngle, maxVerticalAngle);

        // 3) Finally place & aim the camera
        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        if (target == null) return;
        Quaternion rot = Quaternion.Euler(verticalAngle, horizontalAngle, 0f);
        Vector3 dir = rot * Vector3.forward;
        transform.position = target.position - dir * distance;
        transform.LookAt(target.position);
    }
}
