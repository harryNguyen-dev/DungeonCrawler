using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInput : MonoBehaviour, InputSystem_Actions.IPlayerActions
    {
        public Vector2 MoveInput { get; private set; }
        public bool AttackTriggered { get; set; }
        public bool AttackHeld { get; private set; }
        public bool JumpTriggered { get; set; }
        public bool SprintTriggered {get; set;}
        public Vector2 LookInput { get; private set; }
        private InputSystem_Actions input;
        [SerializeField] private float animationSmoothTime = 0.1f; // Thời gian smooth animation
        private PlayerRotate playerRotation;
        private void Awake()
        {
            playerRotation = GetComponent<PlayerRotate>();
            input = new InputSystem_Actions();
            input.Player.SetCallbacks(this);
        }
        private void OnEnable()
        {
            input.Player.Enable();
        }

        private void OnDisable()
        {
            input.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            MoveInput = context.ReadValue<Vector2>();
        }
        
        public void OnLook(InputAction.CallbackContext context)
        {
            LookInput = context.ReadValue<Vector2>();
        }
        public void OnAttack(InputAction.CallbackContext context)
        {
            if(context.started)
            {
                AttackTriggered = true;
            }
            else if(context.canceled)
            {
                AttackTriggered = false;
            }
        }
        public void OnCrouch(InputAction.CallbackContext context) { }
        public void OnInteract(InputAction.CallbackContext context) 
        { }
        public void OnJump(InputAction.CallbackContext context)
        {
        }
        
        public void OnSprint(InputAction.CallbackContext context) 
        { 
        }

        public void OnPrevious(InputAction.CallbackContext context)
        {
        }

        public void OnNext(InputAction.CallbackContext context)
        {
        }
    }
}
