using UnityEngine;

public sealed class LapTimingSession : MonoBehaviour, IEventSubscriber<TrackGateTriggerData>,
    IEventPublisher<GameStartData>, IEventPublisher<RaceStartData>, IEventPublisher<LapCheckData>, IEventPublisher<RaceFinishData>, IEventPublisher<SectorCheckData>, IEventPublisher<LapInvalidatedData>
{
    [SerializeField] private Rigidbody playerKart;
    [SerializeField] private Collider[] checkpoints = System.Array.Empty<Collider>();

    [SerializeField, Min(1)] private int totalLaps = 3;

    private RaceStarter raceStarter;
    private bool raceFinished;
    private int currentLap;
    private int nextCheckpoint = 1; // 다음에 통과해야 할 체크포인트 번호
    private double lapStartedAt;
    private double sectorStartedAt;
    private double raceStartedAt;

    private double? bestLapTime;

    private void Start()
    {
        if (playerKart == null)
            return;

        raceStarter = GetComponent<RaceStarter>();
        EventBus.Instance.Publish(this, new EventData<GameStartData>(EventType.GameStarted, new GameStartData(playerKart, checkpoints.Length, totalLaps)));
    }

    private void OnEnable()
    {
        EventBus.Instance.Subscribe(EventType.TrackGateEntered, this);
    }

    private void OnDisable()
    {
        EventBus.Instance.Unsubscribe(EventType.TrackGateEntered, this);
    }

    public void OnEvent(IEventPublisher<TrackGateTriggerData> publisher, EventData<TrackGateTriggerData> trackEvent)
    {
        TrackGateTriggerData gate = trackEvent.Data;
        if (raceFinished || playerKart == null || gate.VehicleBody != playerKart || publisher is not TrackGateTrigger trigger
            || Vector3.Dot(playerKart.linearVelocity, trigger.transform.forward) <= 0f)
            return;

        double now = Time.timeAsDouble;
        if (!gate.IsStartLine)
        {
            if (currentLap > 0)
                HandleCheckpoint(gate, trigger, now);
            return;
        }

        if (currentLap == 0)
        {
            currentLap = 1;
            lapStartedAt = now;
            sectorStartedAt = now;
            raceStartedAt = now;
            var start = new RaceStartData(playerKart, currentLap, lapStartedAt);
            EventBus.Instance.Publish(this, new EventData<RaceStartData>(EventType.RaceStarted, start));
            return;
        }

        HandleLapCrossing(now);
    }

    /// <summary>주행 중 체크포인트를 통과했을 때 구간 기록과 다음 체크포인트를 갱신합니다.</summary>
    private void HandleCheckpoint(TrackGateTriggerData gate, TrackGateTrigger trigger, double now)
    {
        // TODO: 기대한 순번과 등록된 체크포인트가 모두 일치하는 통과만 처리하세요.
        // 유효한 통과의 구간 시간을 계산하고, 다음 구간에 사용할 상태를 갱신한 뒤 SectorChecked를 발행하세요.
    }

    /// <summary>주행 중 시작선을 다시 통과했을 때 현재 랩을 판정하고 다음 진행을 결정합니다.</summary>
    private void HandleLapCrossing(double now)
    {
        // TODO: 체크포인트가 누락됐으면 같은 랩을 다시 시작하고 LapInvalidated를 발행하세요.
        // 정상 완주면 랩 시간과 마지막 구간 시간을 계산하고 최고 기록을 갱신하세요.
        // 정해진 랩 수를 채웠다면 FinishRace를 호출하고, 나머지는 다음 랩을 시작해 LapChecked를 발행하세요.
    }

    private void FinishRace(double now, double lastLapTime, double lastSectorTime)
    {
        raceFinished = true;
        bool replayStarted = raceStarter != null && raceStarter.PlayRaceReplay();

        var finish = new RaceFinishData(playerKart, currentLap, lastLapTime, lastSectorTime,
            bestLapTime ?? lastLapTime, now - raceStartedAt, replayStarted);
        EventBus.Instance.Publish(this, new EventData<RaceFinishData>(EventType.RaceFinished, finish));
    }
}
