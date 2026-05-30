using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            inputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        if (Instance != this || inputActions == null) return;
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        if (Instance != this || inputActions == null) return;
        inputActions.Player.Disable();
    }

    private void OnDestroy()
    {
        if (Instance != this) return;

        inputActions?.Dispose();
        inputActions = null;
        Instance = null;
    }

    public Vector2 GetMovementVector()
    {
        if (inputActions == null) return Vector2.zero;
        return inputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMousePosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }

    public bool IsAttacking()
    {
        return inputActions != null && inputActions.Player.Attack.IsPressed();
    }

    public bool WasAttackPressed()
    {
        return inputActions != null && inputActions.Player.Attack.WasPressedThisFrame();
    }

    public bool WasPausePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }
}
