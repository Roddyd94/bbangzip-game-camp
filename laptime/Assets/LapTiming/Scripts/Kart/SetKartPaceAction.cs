using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Set Kart Pace", story: "Set the kart pace for the upcoming turn",
    category: "Action/Kart", id: "9268c72df6f944abac86fceade344e55")]
public partial class SetKartPaceAction : Unity.Behavior.Action
{
    private KartAiDriveSource driveSource;
    [SerializeReference]
    public BlackboardVariable<float> StraightSpeed = new(8f);

    [SerializeReference]
    public BlackboardVariable<float> CornerSpeed = new(4f);

    protected override Status OnStart()
    {
        return GameObject.TryGetComponent(out driveSource) ? Status.Running : Status.Failure;
    }

    protected override Status OnUpdate()
    {
        driveSource.SetCruiseSpeed(Mathf.Lerp(StraightSpeed, CornerSpeed, driveSource.UpcomingTurn));
        return Status.Running;
    }
}
