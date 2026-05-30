using Global;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUI.Lobby
{
    /// <summary>3D hero preview for loadout panel — RenderTexture + drag yaw.</summary>
    public class HeroPreviewController : MonoBehaviour
    {
        public static HeroPreviewController Instance { get; private set; }

        [SerializeField] private Camera previewCamera;
        [SerializeField] private Transform modelPivot;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private float dragDegreesPerPixel = 0.4f;
        [SerializeField] private Vector3 modelLocalPosition = new(0f, 0f, 0f);

        private GameObject previewModel;
        private VisualElement dragLayer;
        private bool isDragging;
        private int activePointerId = -1;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (renderTexture == null)
                renderTexture = new RenderTexture(512, 512, 16);

            if (previewCamera != null)
            {
                previewCamera.targetTexture = renderTexture;
                previewCamera.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            UnregisterDragLayer();
            ClearModel();
        }

        public RenderTexture GetRenderTexture() => renderTexture;

        public void ShowPreview()
        {
            if (previewCamera != null)
                previewCamera.enabled = true;

            EnsureModel();
            ResetModelRotation();
        }

        public void HidePreview()
        {
            UnregisterDragLayer();

            if (previewCamera != null)
                previewCamera.enabled = false;

            ClearModel();
        }

        public void RegisterPreviewElement(VisualElement previewPanel, Image previewImage)
        {
            if (previewImage != null && renderTexture != null)
                previewImage.image = renderTexture;

            dragLayer = previewPanel?.Q<VisualElement>("preview-drag-layer") ?? previewPanel;
            if (dragLayer == null) return;

            dragLayer.RegisterCallback<PointerDownEvent>(OnPointerDown);
            dragLayer.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            dragLayer.RegisterCallback<PointerUpEvent>(OnPointerUp);
            dragLayer.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
            dragLayer.RegisterCallback<PointerLeaveEvent>(OnPointerLeave);
        }

        public void UnregisterPreviewElement(VisualElement previewPanel)
        {
            UnregisterDragLayer();
        }

        private void UnregisterDragLayer()
        {
            if (dragLayer == null) return;

            dragLayer.UnregisterCallback<PointerDownEvent>(OnPointerDown);
            dragLayer.UnregisterCallback<PointerMoveEvent>(OnPointerMove);
            dragLayer.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            dragLayer.UnregisterCallback<PointerCancelEvent>(OnPointerCancel);
            dragLayer.UnregisterCallback<PointerLeaveEvent>(OnPointerLeave);
            dragLayer = null;
            isDragging = false;
            activePointerId = -1;
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (previewModel == null) return;

            isDragging = true;
            activePointerId = evt.pointerId;
            dragLayer.CapturePointer(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (!isDragging || previewModel == null || evt.pointerId != activePointerId)
                return;

            var deltaX = evt.deltaPosition.x;
            if (Mathf.Abs(deltaX) < 0.01f)
                return;

            previewModel.transform.Rotate(Vector3.up, -deltaX * dragDegreesPerPixel, Space.World);
            evt.StopPropagation();
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != activePointerId) return;
            EndDrag(evt.pointerId);
            evt.StopPropagation();
        }

        private void OnPointerCancel(PointerCancelEvent evt)
        {
            if (evt.pointerId != activePointerId) return;
            EndDrag(evt.pointerId);
        }

        private void OnPointerLeave(PointerLeaveEvent evt)
        {
            if (!isDragging || evt.pointerId != activePointerId) return;
            EndDrag(evt.pointerId);
        }

        private void EndDrag(int pointerId)
        {
            isDragging = false;
            activePointerId = -1;
            if (dragLayer != null && dragLayer.HasPointerCapture(pointerId))
                dragLayer.ReleasePointer(pointerId);
        }

        private void ResetModelRotation()
        {
            if (previewModel != null)
                previewModel.transform.localRotation = Quaternion.identity;
        }

        private void EnsureModel()
        {
            if (previewModel != null || modelPivot == null) return;

            var prefab = GlobalEntities.Instance?.PlayerPrefab;
            if (prefab == null) return;

            previewModel = Instantiate(prefab, modelPivot);
            previewModel.transform.localPosition = modelLocalPosition;
            previewModel.transform.localRotation = Quaternion.identity;

            DisableGameplayComponents(previewModel);
        }

        private static void DisableGameplayComponents(GameObject model)
        {
            foreach (var rotate in model.GetComponentsInChildren<PlayerController.Rotate>(true))
                rotate.enabled = false;

            foreach (var movement in model.GetComponentsInChildren<PlayerController.Movement>(true))
                movement.enabled = false;

            foreach (var attack in model.GetComponentsInChildren<PlayerController.Attack>(true))
                attack.enabled = false;

            var rb = model.GetComponent<Rigidbody>();
            if (rb != null)
                rb.isKinematic = true;

            var cc = model.GetComponent<CharacterController>();
            if (cc != null)
                cc.enabled = false;
        }

        private void ClearModel()
        {
            if (previewModel != null)
                Destroy(previewModel);
            previewModel = null;
        }
    }
}
