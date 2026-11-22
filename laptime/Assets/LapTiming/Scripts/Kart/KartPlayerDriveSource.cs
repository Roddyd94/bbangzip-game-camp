using UnityEngine;
using UnityEngine.InputSystem;

internal sealed class KartPlayerDriveSource : IKartDriveSource
{
    private readonly InputAction moveAction;
    private bool active;

    public bool InputEnabled { get; private set; } = true;

    public KartPlayerDriveSource(InputActionAsset inputActions)
    {
        moveAction = inputActions?.FindAction("Player/Move")?.Clone();
    }

    public void SetActive(bool value)
    {
        active = value;
        SetInputEnabled(InputEnabled);
    }

    public bool TryGetCommand(int tick, float fixedDeltaTime, out KartDriveCommand command)
    {
        Vector2 input = InputEnabled ? (moveAction?.ReadValue<Vector2>() ?? Vector2.zero) : Vector2.zero;
        command = new KartDriveCommand(tick, input);
        return true;
    }

    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;
        if (active && enabled)
            moveAction?.Enable();
        else
            moveAction?.Disable();
    }

    public void Dispose()
    {
        moveAction?.Dispose();
    }
}
