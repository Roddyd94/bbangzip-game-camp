using System;
using System.Collections.Generic;
using UnityEngine;

internal sealed class KartReplayDriveSource : IKartDriveSource
{
    private readonly Rigidbody body;
    private KartReplay replay;
    private int index;
    private bool restorePending;
    private bool holdingFinalPose;
    private CollisionDetectionMode driveCollisionMode;

    public KartReplayDriveSource(Rigidbody body)
    {
        this.body = body;
    }

    public bool TryLoad(KartReplay recording, float step)
    {
        if (recording?.commands == null || recording.commands.Count == 0
            || !Mathf.Approximately(recording.fixedStep, step))
            return false;

        replay = recording;
        index = 0;
        restorePending = true;
        return true;
    }

    public void ReleaseFinalPose()
    {
        if (!holdingFinalPose)
            return;

        body.isKinematic = false;
        body.collisionDetectionMode = driveCollisionMode;
        holdingFinalPose = false;
    }

    public void PrepareTick()
    {
        if (!restorePending)
            return;

        RestoreStartState();
        restorePending = false;
    }

    public bool TryGetCommand(int tick, float fixedDeltaTime, out KartDriveCommand command)
    {
        if (index >= replay.commands.Count)
        {
            command = default;
            return false;
        }

        command = replay.commands[index++];
        return true;
    }

    public void HoldFinalPose()
    {
        if (holdingFinalPose || body.isKinematic)
            return;

        driveCollisionMode = body.collisionDetectionMode;
        body.collisionDetectionMode = CollisionDetectionMode.Discrete;
        body.isKinematic = true;
        holdingFinalPose = true;
    }

    private void RestoreStartState()
    {
        body.position = replay.startPosition;
        body.rotation = replay.startRotation;
        body.linearVelocity = replay.hasInitialVelocities
            ? replay.startLinearVelocity
            : replay.startRotation * Vector3.back * replay.startSpeed;
        body.angularVelocity = replay.hasInitialVelocities ? replay.startAngularVelocity : Vector3.zero;
        body.WakeUp();
    }
}

[Serializable]
public sealed class KartReplay
{
    public Vector3 startPosition;
    public Quaternion startRotation;
    public float startSpeed;
    public bool hasInitialVelocities;
    public Vector3 startLinearVelocity;
    public Vector3 startAngularVelocity;
    public float fixedStep;
    public List<KartDriveCommand> commands = new List<KartDriveCommand>();
}