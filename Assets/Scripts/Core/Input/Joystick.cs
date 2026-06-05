using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{
    public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private RectTransform joystickTransform;
        [SerializeField] private RectTransform backgroundTransform;

        private Vector2 inputVector;

        public float handleLimit = 1.0f;
        public float Horizontal => inputVector.x;
        public float Vertical => inputVector.y;
        public Vector2 Direction => inputVector;

        private void Start()
        {
            if (backgroundTransform == null)
                backgroundTransform = GetComponent<RectTransform>();

            TryRegister();
        }

        private void OnEnable()
        {
            TryRegister();
        }

        private void TryRegister()
        {
            InputManager.Instance?.RegisterMoveJoystick(this);
        }

        private void OnDisable()
        {
            InputManager.Instance?.UnregisterMoveJoystick(this);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (backgroundTransform == null || joystickTransform == null)
                return;

            Vector2 direction = eventData.position - RectTransformUtility.WorldToScreenPoint(null, backgroundTransform.position);
            float radius = backgroundTransform.sizeDelta.x / 2f;
            inputVector = direction.magnitude > radius
                ? direction.normalized
                : direction / radius;

            joystickTransform.anchoredPosition = (inputVector * radius) * handleLimit;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inputVector = Vector2.zero;
            if (joystickTransform != null)
                joystickTransform.anchoredPosition = Vector2.zero;
        }
    }
}
