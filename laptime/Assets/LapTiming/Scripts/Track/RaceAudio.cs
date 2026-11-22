using UnityEngine;

public sealed class RaceAudio : MonoBehaviour, IEventSubscriber<TrackGateTriggerData>
{
    [SerializeField] private Rigidbody playerKart;
    [SerializeField] private AudioClip countdownTick;
    [SerializeField] private AudioClip startSignal;
    [SerializeField] private AudioClip checkpointPass;
    [SerializeField] private AudioClip startLinePass;

    private AudioSource cues;
    private int lastGateIndex = -1;
    private float lastGateTime;

    private void Awake()
    {
        cues = gameObject.AddComponent<AudioSource>();
        cues.playOnAwake = false;
        cues.spatialBlend = 0f;
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe(EventType.TrackGateEntered, this);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe(EventType.TrackGateEntered, this);
        cues.Stop();
    }

    public void PlayCountdownTick()
    {
        cues.PlayOneShot(countdownTick, 0.62f);
    }

    public void PlayStartSignal()
    {
        cues.PlayOneShot(startSignal, 0.74f);
    }

    public void OnEvent(IEventPublisher<TrackGateTriggerData> publisher, EventData<TrackGateTriggerData> gateEvent)
    {
        TrackGateTriggerData gate = gateEvent.Data;
        if (playerKart == null || gate.VehicleBody != playerKart || publisher is not TrackGateTrigger trigger
            || Vector3.Dot(playerKart.linearVelocity, trigger.transform.forward) <= 0f)
            return;

        if (gate.GateIndex == lastGateIndex && Time.time - lastGateTime < 0.3f)
            return;

        lastGateIndex = gate.GateIndex;
        lastGateTime = Time.time;
        cues.PlayOneShot(gate.IsStartLine ? startLinePass : checkpointPass, 0.6f);
    }
}
