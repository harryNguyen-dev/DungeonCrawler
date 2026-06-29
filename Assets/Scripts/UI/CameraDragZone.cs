using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CustomUI
{
    /// <summary>Transparent UI zone that forwards drag delta to InputManager for camera pan.</summary>
    [RequireComponent(typeof(Image))]
    public class CameraDragZone : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private float dragDeadzonePixels = 4f;

        private bool isDragging;

        private void Awake()
        {
            var image = GetComponent<Image>();
            if (image != null)
            {
                image.color = new Color(1f, 1f, 1f, 0f);
                image.raycastTarget = true;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            InputManager.Instance?.SetUiCameraDragging(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!isDragging || InputManager.Instance == null)
                return;

            var delta = eventData.delta;
            if (delta.sqrMagnitude < dragDeadzonePixels * dragDeadzonePixels)
                return;

            InputManager.Instance.AddUiCameraDragDelta(delta);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
            InputManager.Instance?.SetUiCameraDragging(false);
        }
    }
}
