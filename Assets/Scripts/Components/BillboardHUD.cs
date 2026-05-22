using UnityEngine;

public class BillboardHUD : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private bool useMainCameraIfNull = true;

    [Header("Behavior Settings")]
    [SerializeField] private bool freezeX = false;
    [SerializeField] private bool freezeY = false;
    [SerializeField] private bool freezeZ = false;

    private void Start()
    {
        // Automatically grab the main camera if one wasn't explicitly assigned
        if (targetCamera == null && useMainCameraIfNull)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null)
        {
            Debug.LogWarning($"[BillboardHUD] No camera assigned to {gameObject.name}!", this);
        }
    }

    // LateUpdate runs after standard movement updates, preventing jerky/stuttering UI
    private void LateUpdate()
    {
        if (targetCamera == null) return;

        // Calculate the rotation required to align with the camera
        Vector3 targetRotation = targetCamera.transform.rotation.eulerAngles;

        // Lock axes if needed (great for keeping health bars flat on the ground)
        if (freezeX) targetRotation.x = transform.rotation.eulerAngles.x;
        if (freezeY) targetRotation.y = transform.rotation.eulerAngles.y;
        if (freezeZ) targetRotation.z = transform.rotation.eulerAngles.z;

        // Apply the rotation
        transform.rotation = Quaternion.Euler(targetRotation);
    }
}