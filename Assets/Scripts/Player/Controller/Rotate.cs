using UnityEngine;

namespace PlayerController
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private float moveRotateSpeed = 15f;

        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        // --- Cơ chế cũ: xoay model theo chuột mỗi frame (đã tắt) ---
        // private void LateUpdate()
        // {
        //     RotatePlayerToMouse();
        // }
        //
        // private void RotatePlayerToMouse()
        // {
        //     Vector2 mousePos = InputManager.Instance.GetMousePosition();
        //     Ray ray = mainCamera.ScreenPointToRay(mousePos);
        //     Plane groundPlane = new Plane(Vector3.up, transform.position);
        //     if (groundPlane.Raycast(ray, out float distance))
        //     {
        //         Vector3 mouseWorldPosition = ray.GetPoint(distance);
        //         Vector3 lookDir = mouseWorldPosition - transform.position;
        //         lookDir.y = 0;
        //         if (lookDir != Vector3.zero)
        //         {
        //             Quaternion targetRotation = Quaternion.LookRotation(lookDir);
        //             transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
        //         }
        //     }
        // }

        /// <summary>Hướng ngắm từ chuột trên mặt phẳng ngang (không xoay model).</summary>
        public bool TryGetAimDirection(out Vector3 aimDirection)
        {
            aimDirection = Vector3.zero;
            if (mainCamera == null || InputManager.Instance == null)
                return false;

            Vector2 mousePos = InputManager.Instance.GetMousePosition();
            Ray ray = mainCamera.ScreenPointToRay(mousePos);
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            if (!groundPlane.Raycast(ray, out float distance))
                return false;

            Vector3 mouseWorldPosition = ray.GetPoint(distance);
            aimDirection = mouseWorldPosition - transform.position;
            aimDirection.y = 0f;

            return aimDirection.sqrMagnitude > 0.0001f;
        }

        public void FaceTowards(Vector3 worldDirection, float rotateSpeed)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.0001f)
                return;

            Quaternion targetRotation = Quaternion.LookRotation(worldDirection.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * rotateSpeed);
        }

        public void FaceMovementDirection(Vector3 moveDirection)
        {
            FaceTowards(moveDirection, moveRotateSpeed);
        }

        /// <summary>Xoay tức thì về hướng chuột trước khi đánh.</summary>
        public void SnapFaceAimDirection()
        {
            if (!TryGetAimDirection(out Vector3 aimDirection))
                return;

            transform.rotation = Quaternion.LookRotation(aimDirection.normalized);
        }
    }
}
