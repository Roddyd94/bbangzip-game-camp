using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public sealed class LapTimingHud : MonoBehaviour, IEventSubscriber<GameStartData>, IEventSubscriber<RaceStartData>, IEventSubscriber<LapCheckData>, IEventSubscriber<RaceFinishData>, IEventSubscriber<LapInvalidatedData>
{
    [SerializeField] private RectTransform timingPanel;
    [SerializeField] private Button restartButton;
    [SerializeField] private TextMeshProUGUI speedReadout;
    [SerializeField] private TextMeshProUGUI lapReadout;
    [SerializeField] private TextMeshProUGUI timeReadout;
    [SerializeField] private TextMeshProUGUI bestReadout;
    [SerializeField] private TextMeshProUGUI totalReadout;

    private Rigidbody playerKart;
    private double? lapStartedAt;
    private double? finishedLapTime;
    private double? raceStartedAt;
    private double? finishedRaceTime;
    private void Awake()
    {
        restartButton.onClick.AddListener(RestartRace);

        lapReadout.text = "WAITING";
        bestReadout.text = $"{FormatTime(null)}";
        totalReadout.text = $"{FormatTime(null)}";

        EventBus.Instance.Subscribe(EventType.GameStarted, this);
        EventBus.Instance.Subscribe(EventType.RaceStarted, this);
        EventBus.Instance.Subscribe(EventType.LapChecked, this);
        EventBus.Instance.Subscribe(EventType.LapInvalidated, this);
        EventBus.Instance.Subscribe(EventType.RaceFinished, this);
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(RestartRace);
        EventBus.Instance.Unsubscribe(EventType.GameStarted, this);
        EventBus.Instance.Unsubscribe(EventType.RaceStarted, this);
        EventBus.Instance.Unsubscribe(EventType.LapChecked, this);
        EventBus.Instance.Unsubscribe(EventType.LapInvalidated, this);
        EventBus.Instance.Unsubscribe(EventType.RaceFinished, this);
    }

    private void Update()
    {
        float speed = playerKart == null ? 0f : Vector3.ProjectOnPlane(playerKart.linearVelocity, Vector3.up).magnitude;
        speedReadout.text = $"{speed:0.0} m/s";
        double? elapsed = lapStartedAt.HasValue ? Math.Max(0d, Time.timeAsDouble - lapStartedAt.Value) : finishedLapTime;
        timeReadout.text = $"{FormatTime(elapsed)}";
        double? total = finishedRaceTime ?? (raceStartedAt.HasValue ? Math.Max(0d, Time.timeAsDouble - raceStartedAt.Value) : null);
        totalReadout.text = $"{FormatTime(total)}";
    }

    public void OnEvent(IEventPublisher<GameStartData> publisher, EventData<GameStartData> gameEvent)
    {
        if (playerKart == null)
            playerKart = gameEvent.Data.PlayerKart;
    }

    public void OnEvent(IEventPublisher<RaceStartData> publisher, EventData<RaceStartData> startEvent)
    {
        RaceStartData data = startEvent.Data;
        if (playerKart != null && playerKart != data.PlayerKart)
            return;

        playerKart = data.PlayerKart;
        lapStartedAt = data.LapStartedAt;
        lapReadout.text = $"{data.LapNumber}";
        bestReadout.text = $"{FormatTime(null)}";
        raceStartedAt = data.LapStartedAt;
        finishedRaceTime = null;
    }

    public void OnEvent(IEventPublisher<LapCheckData> publisher, EventData<LapCheckData> lapEvent)
    {
        LapCheckData data = lapEvent.Data;
        if (playerKart != null && playerKart != data.PlayerKart)
            return;

        playerKart = data.PlayerKart;
        lapStartedAt = data.LapStartedAt;
        lapReadout.text = $"{data.NextLapNumber}";
        bestReadout.text = $"{FormatTime(data.BestLapTime)}";
    }

    public void OnEvent(IEventPublisher<LapInvalidatedData> publisher, EventData<LapInvalidatedData> lapEvent)
    {
        LapInvalidatedData data = lapEvent.Data;
        if (data.PlayerKart == playerKart)
            lapStartedAt = data.LapStartedAt;
    }

    public void OnEvent(IEventPublisher<RaceFinishData> publisher, EventData<RaceFinishData> finishEvent)
    {
        RaceFinishData data = finishEvent.Data;
        if (playerKart != null && playerKart != data.PlayerKart)
            return;

        playerKart = data.PlayerKart;
        lapStartedAt = null;
        finishedLapTime = data.LastLapTime;
        finishedRaceTime = data.TotalRaceTime;
        lapReadout.text = data.ReplayStarted ? $"{data.CompletedLaps} / Replay" : $"{data.CompletedLaps} / Finish";
        bestReadout.text = $"{FormatTime(data.BestLapTime)}";
        timingPanel.sizeDelta = new Vector2(266f, 190f);

        restartButton.gameObject.SetActive(true);
    }

    private static void RestartRace()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private static string FormatTime(double? seconds)
    {
        return seconds.HasValue ? TimeSpan.FromSeconds(seconds.Value).ToString(@"mm\:ss\.fff") : "--:--.---";
    }
}
