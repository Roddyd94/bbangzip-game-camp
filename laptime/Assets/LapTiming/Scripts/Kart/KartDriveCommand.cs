using System;
using UnityEngine;

[Serializable]
public struct KartDriveCommand
{
    [SerializeField] private int tick;
    [SerializeField] private Vector2 input;

    public int Tick => tick;

    public KartDriveCommand(int tick, Vector2 input)
    {
        this.tick = tick;
        this.input = Vector2.ClampMagnitude(input, 1f);
    }

    public void Execute(KartMotor motor, float step)
    {
        motor.Drive(input, step);
    }
}
