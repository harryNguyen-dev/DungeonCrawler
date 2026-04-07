using UnityEngine;
using Player;
namespace Core
{
    // InputManager.cs — Singleton, đặt trên GameObject "InputManager" trong scene
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance { get; private set; }

        // Đọc từ bên ngoài
        public static Vector2 MoveInput => Instance._move;
        public static bool SprintHeld => Instance._sprint;
        public static bool AttackPressed => Instance._attackPressed;
        public static bool DodgePressed => Instance._dodgePressed;
        public static bool BlockHeld => Instance._blockHeld;
        public static bool ParryPressed => Instance._parryPressed;
        public static bool InteractPressed => Instance._interactPressed;
        public static bool LockOnPressed => Instance._lockOnPressed;

        // Internal state
        Vector2 _move;
        bool _sprint;
        bool _attackPressed;
        bool _dodgePressed;
        bool _blockHeld;
        bool _parryPressed;
        bool _interactPressed;
        bool _lockOnPressed;

        InputSystem_Actions _actions;
        [SerializeField] PlayerController _player;
        void Awake()
        {
            if (Instance != null) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _actions = new InputSystem_Actions();
            _actions.Player.Enable();

            // Subscribe callbacks
            _actions.Player.Attack.performed += _ => _player.OnAttackInput();
            // _actions.Player.Dodge.performed += _ => _player.OnDodgeInput();
            _actions.Player.Parry.performed += _ => _player.OnParryInput();
            _actions.Player.Interact.performed += _ => _player.OnInteractInput();
            _actions.Player.LockOn.performed += _ => _player.OnLockOnInput();
        }

        void Update()
        {
            _player.SetMoveInput(_actions.Player.Move.ReadValue<Vector2>());
            _player.SetSprintInput(_actions.Player.Sprint.IsPressed());
            _player.SetBlockInput(_actions.Player.Block.IsPressed());
        }

        void OnDestroy() => _actions?.Dispose();
    }
}