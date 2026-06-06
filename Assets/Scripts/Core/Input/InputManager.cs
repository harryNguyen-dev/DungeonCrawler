using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private const float JoystickDeadzoneSqr = 0.01f;

    private InputSystem_Actions inputActions;
    private Core.Joystick moveJoystick;
    private Core.Joystick skillAimJoystick;
    private bool uiAttackHeld;
    private bool uiDashPressedThisFrame;

    private bool skillAimHeld;
    private bool skillAimReleasedThisFrame;
    private Vector2 skillAimReleaseVector;
    private bool keyboardSkillAimHeld;

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

    private void Update()
    {
        UpdateKeyboardSkillAim();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (Keyboard.current != null && Keyboard.current.pKey.wasPressedThisFrame)
            Core.Save.LevelProgressService.ResetSave();
#endif
    }

    private void LateUpdate()
    {
        uiDashPressedThisFrame = false;
        skillAimReleasedThisFrame = false;
    }

    public void RegisterMoveJoystick(Core.Joystick joystick)
    {
        if (joystick == null || joystick.Role != Core.Joystick.JoystickRole.Move)
            return;

        moveJoystick = joystick;
    }

    public void RegisterSkillAimJoystick(Core.Joystick joystick)
    {
        if (joystick == null || joystick.Role != Core.Joystick.JoystickRole.SkillAim)
            return;

        skillAimJoystick = joystick;
    }

    public void UnregisterMoveJoystick(Core.Joystick joystick)
    {
        if (moveJoystick == joystick)
            moveJoystick = null;
    }

    public void UnregisterSkillAimJoystick(Core.Joystick joystick)
    {
        if (skillAimJoystick == joystick)
            skillAimJoystick = null;
    }

    public void NotifySkillAimPressed()
    {
        skillAimHeld = true;
    }

    public void NotifySkillAimReleased(Vector2 releaseDirection)
    {
        if (!skillAimHeld)
            return;

        skillAimHeld = false;
        skillAimReleasedThisFrame = true;
        skillAimReleaseVector = releaseDirection;
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

    /// <summary>Current aim direction while holding skill aim joystick (or keyboard Q).</summary>
    public Vector2 GetSkillAimVector()
    {
        return ResolveSkillAimDirection(includeMoveFallback: keyboardSkillAimHeld);
    }

    private Vector2 ResolveSkillAimDirection(bool includeMoveFallback)
    {
        if (skillAimJoystick != null && skillAimJoystick.IsHeld)
        {
            Vector2 joystickInput = skillAimJoystick.Direction;
            if (joystickInput.sqrMagnitude > JoystickDeadzoneSqr)
                return Vector2.ClampMagnitude(joystickInput, 1f);
        }

        if (keyboardSkillAimHeld || includeMoveFallback)
        {
            if (inputActions != null)
            {
                Vector2 look = inputActions.Player.Look.ReadValue<Vector2>();
                if (look.sqrMagnitude > JoystickDeadzoneSqr)
                    return Vector2.ClampMagnitude(look, 1f);
            }

            if (includeMoveFallback)
            {
                Vector2 move = GetMovementVector();
                if (move.sqrMagnitude > JoystickDeadzoneSqr)
                    return move;
            }
        }

        return Vector2.zero;
    }

    public bool IsSkillAimHeld() => skillAimHeld || keyboardSkillAimHeld;

    public bool WasSkillAimReleased()
    {
        return skillAimReleasedThisFrame;
    }

    /// <summary>Aim direction captured at the moment skill aim was released.</summary>
    public Vector2 GetSkillAimReleaseVector() => skillAimReleaseVector;

    public Vector2 GetMousePosition()
    {
        return Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
    }

    public void SetUiAttackHeld(bool held) => uiAttackHeld = held;

    public void SetUiDashPressed() => uiDashPressedThisFrame = true;

    /// <summary>True while the Normal Attack UI button is held.</summary>
    public bool IsAttacking() => uiAttackHeld;

    public bool WasDashPressed() => uiDashPressedThisFrame;

    public bool WasPausePressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
    }

    private void UpdateKeyboardSkillAim()
    {
        if (Keyboard.current == null)
            return;

        bool qHeld = Keyboard.current.qKey.isPressed;

        if (qHeld && !keyboardSkillAimHeld)
            keyboardSkillAimHeld = true;

        if (!qHeld && keyboardSkillAimHeld)
        {
            keyboardSkillAimHeld = false;
            skillAimReleasedThisFrame = true;
            skillAimReleaseVector = ResolveSkillAimDirection(includeMoveFallback: true);
        }
    }
}
