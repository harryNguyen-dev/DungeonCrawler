using Unity.Cinemachine;
using UnityEngine;

namespace Core
{
    /// <summary>UI drag pans Cinemachine follow offset; releases snap back to default.</summary>
    [RequireComponent(typeof(CinemachineCamera))]
    public class BattleCameraPanController : MonoBehaviour
    {
        [SerializeField] private float panUnitsPerPixel = 0.04f;
        [SerializeField] private float maxPanDistance = 12f;

        private CinemachineFollow follow;
        private Vector3 baseFollowOffset;
        private Vector3 panOffset;
        private bool wasDragging;

        private void Awake()
        {
            follow = GetComponent<CinemachineFollow>();
            if (follow != null)
                baseFollowOffset = follow.FollowOffset;
        }

        private void LateUpdate()
        {
            if (follow == null || InputManager.Instance == null)
                return;

            bool dragging = InputManager.Instance.IsUiCameraDragging();

            if (dragging)
            {
                ApplyDragDelta(InputManager.Instance.ConsumeCameraDragDelta());
            }
            else
            {
                InputManager.Instance.ConsumeCameraDragDelta();
                if (wasDragging)
                    panOffset = Vector3.zero;
            }

            wasDragging = dragging;

            follow.FollowOffset = baseFollowOffset + panOffset;
        }

        private void ApplyDragDelta(Vector2 dragDelta)
        {
            if (dragDelta.sqrMagnitude < 0.0001f)
                return;

            float yaw = transform.eulerAngles.y * Mathf.Deg2Rad;
            float sin = Mathf.Sin(yaw);
            float cos = Mathf.Cos(yaw);

            Vector3 right = new Vector3(cos, 0f, -sin);
            Vector3 forward = new Vector3(sin, 0f, cos);
            Vector3 worldDelta = (right * dragDelta.x + forward * dragDelta.y) * panUnitsPerPixel;

            panOffset += worldDelta;
            if (panOffset.sqrMagnitude > maxPanDistance * maxPanDistance)
                panOffset = panOffset.normalized * maxPanDistance;
        }
    }
}
