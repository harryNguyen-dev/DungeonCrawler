using Core;
using Core.Save;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private const float JoystickDeadzoneSqr = 0.01f;

    private InputSystem_Actions inputActions;
    private Core.Joystick moveJoystick;
    private bool uiAttackHeld;

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

    public void RegisterMoveJoystick(Core.Joystick joystick)
    {
        moveJoystick = joystick;
    }

    public void UnregisterMoveJoystick(Core.Joystick joystick)
    {
        if (moveJoystick == joystick)
            moveJoystick = null;
    }

    public Vector2 GetMovementVector()
    {
        if (moveJoystick != null)
        {
            Vector2 joystickInput = moveJoystick.Direction;
            if (joystickInput.sqrMagnitude > JoystickDeadzoneSqr)
                return Vector2.ClampMagnitude(joystickInput, 1f);
        }

        if (inputActions == null) return Vector2.zero;
        return inputActions.Player.Move.ReadValue<Vector2>();
    }

    public Vector2 GetMousePosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }

    public void SetUiAttackHeld(bool held)
    {
        uiAttackHeld = held;
    }

    public bool IsAttacking()
    {
        if (uiAttackHeld)
            return true;

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

#if UNITY_EDITOR
    private void Update()
    {
        if (!Debug.isDebugBuild)
            return;

        if (Keyboard.current == null || !Keyboard.current.pKey.wasPressedThisFrame)
            return;

        LevelProgressService.ResetSave();
    }
#endif
}
