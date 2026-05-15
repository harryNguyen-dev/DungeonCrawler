using UnityEngine;

namespace PlayerController
{

    using UnityEngine;

    public class Rotate : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            RotatePlayerToMouse();
        }

        private void RotatePlayerToMouse()
        {
            // 1. Lấy vị trí chuột từ New Input System thông qua InputManager
            Vector2 mousePos = InputManager.Instance.GetMousePosition();

            // 2. Tạo một tia (Ray) từ Camera
            Ray ray = mainCamera.ScreenPointToRay(mousePos);

            // 3. Tạo một mặt phẳng ảo nằm ngang (Vector3.up là pháp tuyến)
            // Mặt phẳng này đi qua vị trí hiện tại của Player
            Plane groundPlane = new Plane(Vector3.up, transform.position);

            // 4. Tìm giao điểm giữa Tia và Mặt phẳng
            // Biến 'distance' sẽ lưu khoảng cách từ Camera đến điểm giao
            if (groundPlane.Raycast(ray, out float distance))
            {
                // Lấy tọa độ 3D tại khoảng cách đó
                Vector3 mouseWorldPosition = ray.GetPoint(distance);

                // 5. Tính toán hướng xoay
                Vector3 lookDir = mouseWorldPosition - transform.position;

                // Đảm bảo không xoay theo trục đứng
                lookDir.y = 0;

                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(lookDir);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
                }
            }
        }
    }
}