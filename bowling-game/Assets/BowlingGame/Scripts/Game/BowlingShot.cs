using System;
using UnityEngine;

public enum BowlingShotStage
{
    StartPosition,
    ThrowAngle,
    ThrowPower,
    SpinDirection,
    SpinStrength,
    Ready,
    Rolling,
    PinSetup,
    Finished
}

[Serializable]
public sealed class BowlingShot
{
    [field: SerializeField, Range(-0.4f, 0.4f)]
    public float StartOffset { get; set; } = 0f;
    [field: SerializeField, Range(-8f, 8f)]
    public float ThrowAngle { get; set; } = 0f;
    [field: SerializeField, Range(0f, 100f)]
    public float ThrowPower { get; set; } = 65f;
    [field: SerializeField, Range(-90f, 90f)]
    public float SpinDirection { get; set; } = 0f;
    [field: SerializeField, Range(0f, 100f)]
    public float SpinStrength { get; set; } = 20f;

    public float LaunchSpeed => Mathf.Lerp(4f, 12f, ThrowPower / 100f);
    public float ExtraSpinSpeed => SpinStrength / 100f * 60f;

    public Vector3 GetThrowDirection(Vector3 laneForward)
    {
        return Quaternion.AngleAxis(ThrowAngle, Vector3.up) * laneForward;
    }

    public Vector3 GetSpinAxis(Vector3 laneForward)
    {
        Vector3 throwDirection = GetThrowDirection(laneForward);
        Vector3 forwardRollAxis = Vector3.Cross(Vector3.up, throwDirection);
        return Quaternion.AngleAxis(SpinDirection, throwDirection) * forwardRollAxis;
    }
}
