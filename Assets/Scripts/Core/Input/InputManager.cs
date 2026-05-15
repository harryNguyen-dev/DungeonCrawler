using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    InputSystem_Actions inputActions;
    PlayerController.Movement movement;
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Khởi tạo instance của Input Actions
            inputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    public Vector2 GetMovementVector()
    {
        // Đọc giá trị Vector2 từ Action "Move"
        return inputActions.Player.Move.ReadValue<Vector2>();
    }
    public Vector2 GetMousePosition()
    {
        return Mouse.current.position.ReadValue();
    }
    public bool IsAttacking()
    {
        return inputActions.Player.Attack.IsPressed();
    }
    public bool WasAttackPressed()
    {
        // Chỉ trả về true vào đúng khung hình người chơi nhấn xuống
        return inputActions.Player.Attack.WasPressedThisFrame();
    }
}
