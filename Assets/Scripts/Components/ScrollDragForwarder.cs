using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Components
{
    /// <summary>Forwards drag events from scroll children (e.g. map cards) to the parent ScrollRect.</summary>
    public class ScrollDragForwarder : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ScrollRect scrollRect;

        private void Awake()
        {
            scrollRect = GetComponentInParent<ScrollRect>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (IsPointerOverInteractableButton(eventData))
                return;

            scrollRect?.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (IsPointerOverInteractableButton(eventData))
                return;

            scrollRect?.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (IsPointerOverInteractableButton(eventData))
                return;

            scrollRect?.OnEndDrag(eventData);
        }

        private static bool IsPointerOverInteractableButton(PointerEventData eventData)
        {
            if (eventData?.pointerPress == null)
                return false;

            var button = eventData.pointerPress.GetComponent<Button>()
                         ?? eventData.pointerPress.GetComponentInParent<Button>();

            return button != null && button.interactable;
        }
    }
}
