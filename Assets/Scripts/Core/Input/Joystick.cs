using UnityEngine;
using UnityEngine.EventSystems;

namespace Core
{
    public class Joystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler, IPointerExitHandler
    {
        public enum JoystickRole
        {
            Move,
            SkillAim
        }

        [SerializeField] private JoystickRole role = JoystickRole.Move;
        [SerializeField] private RectTransform joystickTransform;
        [SerializeField] private RectTransform backgroundTransform;

        private Vector2 inputVector;
        private Vector2 lastNonZeroAim;
        private bool isHeld;

        public JoystickRole Role => role;
        public bool IsHeld => isHeld;
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
            if (InputManager.Instance == null)
                return;

            if (role == JoystickRole.SkillAim)
                InputManager.Instance.RegisterSkillAimJoystick(this);
            else
                InputManager.Instance.RegisterMoveJoystick(this);
        }

        private void OnDisable()
        {
            if (isHeld && role == JoystickRole.SkillAim)
            {
                InputManager.Instance?.NotifySkillAimReleased(
                    inputVector.sqrMagnitude > 0.01f ? inputVector : lastNonZeroAim);
            }

            isHeld = false;
            inputVector = Vector2.zero;

            if (InputManager.Instance == null)
                return;

            if (role == JoystickRole.SkillAim)
                InputManager.Instance.UnregisterSkillAimJoystick(this);
            else
                InputManager.Instance.UnregisterMoveJoystick(this);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            isHeld = true;
            lastNonZeroAim = Vector2.zero;

            if (role == JoystickRole.SkillAim)
                InputManager.Instance?.NotifySkillAimPressed();

            OnDrag(eventData);
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

            if (role == JoystickRole.SkillAim && inputVector.sqrMagnitude > 0.01f)
                lastNonZeroAim = inputVector;

            joystickTransform.anchoredPosition = (inputVector * radius) * handleLimit;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            ReleaseSkillAimIfNeeded();
            ResetHandle();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (role != JoystickRole.SkillAim)
            {
                ResetHandle();
                return;
            }

            ReleaseSkillAimIfNeeded();
            ResetHandle();
        }

        private void ReleaseSkillAimIfNeeded()
        {
            if (role != JoystickRole.SkillAim || !isHeld)
                return;

            InputManager.Instance?.NotifySkillAimReleased(
                inputVector.sqrMagnitude > 0.01f ? inputVector : lastNonZeroAim);
            isHeld = false;
        }

        private void ResetHandle()
        {
            isHeld = false;
            inputVector = Vector2.zero;
            lastNonZeroAim = Vector2.zero;
            if (joystickTransform != null)
                joystickTransform.anchoredPosition = Vector2.zero;
        }
    }
}
