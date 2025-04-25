using UnityEngine;

public class BillboardCanvas : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main; // You can also expose this if needed
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        Vector3 direction = transform.position - mainCamera.transform.position;
        direction.y = 0f; // Only rotate on Y axis
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
