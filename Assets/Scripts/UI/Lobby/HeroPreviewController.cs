using Global;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI.Lobby
{
    /// <summary>3D hero preview for loadout panel — RenderTexture + drag yaw.</summary>
    public class HeroPreviewController : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        public static HeroPreviewController Instance { get; private set; }

        [SerializeField] private Camera previewCamera;
        [SerializeField] private Transform modelPivot;
        [SerializeField] private RenderTexture renderTexture;
        [SerializeField] private float dragDegreesPerPixel = 0.4f;
        [SerializeField] private Vector3 modelLocalPosition = new(0f, 0f, 0f);

        private GameObject previewModel;
        private RawImage boundPreviewImage;
        private bool isDragging;

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

            UnregisterPreview();
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
            UnregisterPreview();

            if (previewCamera != null)
                previewCamera.enabled = false;

            ClearModel();
        }

        public void RegisterPreview(RawImage previewImage)
        {
            UnregisterPreview();
            boundPreviewImage = previewImage;

            if (boundPreviewImage != null && renderTexture != null)
                boundPreviewImage.texture = renderTexture;
        }

        public void UnregisterPreview()
        {
            boundPreviewImage = null;
            isDragging = false;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (previewModel == null || boundPreviewImage == null)
                return;

            isDragging = true;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || previewModel == null)
                return;

            var deltaX = eventData.delta.x;
            if (Mathf.Abs(deltaX) < 0.01f)
                return;

            previewModel.transform.Rotate(Vector3.up, -deltaX * dragDegreesPerPixel, Space.World);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
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
