using UnityEngine;
public readonly struct TrackGateTriggerData
{
    public int GateIndex { get; }
    public bool IsStartLine => GateIndex == 0;
    public Rigidbody VehicleBody { get; }
    public GameObject Vehicle => VehicleBody.gameObject;

    public TrackGateTriggerData(int gateIndex, Rigidbody vehicleBody)
    {
        GateIndex = gateIndex;
        VehicleBody = vehicleBody;
    }

    public override string ToString()
    {
        return $"TrackGateTriggerData(GateIndex={GateIndex}, IsStartLine={IsStartLine}, VehicleBody={(VehicleBody != null ? VehicleBody.name : "<none>")})";
    }
}

[RequireComponent(typeof(BoxCollider))]
public sealed class TrackGateTrigger : MonoBehaviour, IEventPublisher<TrackGateTriggerData>
{
    [SerializeField, Min(0)] private int gateIndex;

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody vehicle = other.attachedRigidbody;
        if (vehicle == null)
            return;

        var data = new TrackGateTriggerData(gateIndex, vehicle);
        Publish(new EventData<TrackGateTriggerData>(EventType.TrackGateEntered, data));
    }

    void Publish(EventData<TrackGateTriggerData> eventData)
    {
        EventBus.Instance.Publish(this, eventData);
    }
}
