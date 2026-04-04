using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Core
{

    public class ThirdCameraController : MonoBehaviour
    {
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float zoomLerpSpeed = 2f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 2f;

        private InputSystem_Actions controls;
        private Vector2 scrollDelta;
        private CinemachineCamera cam;
        private CinemachineOrbitalFollow orbital;
        private float targetZoom;
        private float currentZoom;
        void Start()
        {
            controls = new InputSystem_Actions();
            controls.Enable();
            controls.CameraControls.MouseZoom.performed += HandleMouseScroll;

            Cursor.lockState = CursorLockMode.Locked;
            cam = GetComponent<CinemachineCamera>();
            orbital = cam.GetComponent<CinemachineOrbitalFollow>();

            targetZoom = currentZoom = orbital.Radius;
        }
        private void HandleMouseScroll(InputAction.CallbackContext context)
        {
            scrollDelta = context.ReadValue<Vector2>();
        }

        void Update()
        {
            if(scrollDelta.y != 0)
            {
                if(orbital != null)
                {
                    targetZoom = Mathf.Clamp(orbital.Radius - scrollDelta.y * zoomSpeed, minDistance, maxDistance);
                    scrollDelta = Vector2.zero;
                }
            }
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomLerpSpeed * Time.deltaTime);
            orbital.Radius = currentZoom;
        }
    }
}