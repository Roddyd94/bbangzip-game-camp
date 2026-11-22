using System;
using TMPro;
using UnityEngine;

public sealed class SectorTimingHud : MonoBehaviour, IEventSubscriber<GameStartData>, IEventSubscriber<RaceStartData>,
    IEventSubscriber<SectorCheckData>, IEventSubscriber<LapCheckData>, IEventSubscriber<RaceFinishData>, IEventSubscriber<LapInvalidatedData>
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI labelTemplate;
    [SerializeField] private TextMeshProUGUI valueTemplate;

    private const float Padding = 12f;
    private const float RowHeight = 24f;
    private const float LapWidth = 64f;
    private const float SectorWidth = 98f;

    private Rigidbody playerKart;
    private TextMeshProUGUI[] lapLabels;
    private TextMeshProUGUI[,] sectorValues;
    private int activeLap;

    private void Awake()
    {
        EventBus.Instance.Subscribe(EventType.GameStarted, this);
        EventBus.Instance.Subscribe(EventType.RaceStarted, this);
        EventBus.Instance.Subscribe(EventType.SectorChecked, this);
        EventBus.Instance.Subscribe(EventType.LapChecked, this);
        EventBus.Instance.Subscribe(EventType.LapInvalidated, this);
        EventBus.Instance.Subscribe(EventType.RaceFinished, this);
    }

    private void OnDestroy()
    {
        EventBus.Instance.Unsubscribe(EventType.GameStarted, this);
        EventBus.Instance.Unsubscribe(EventType.RaceStarted, this);
        EventBus.Instance.Unsubscribe(EventType.SectorChecked, this);
        EventBus.Instance.Unsubscribe(EventType.LapChecked, this);
        EventBus.Instance.Unsubscribe(EventType.LapInvalidated, this);
        EventBus.Instance.Unsubscribe(EventType.RaceFinished, this);
    }

    public void OnEvent(IEventPublisher<GameStartData> publisher, EventData<GameStartData> gameEvent)
    {
        GameStartData data = gameEvent.Data;
        if (sectorValues != null)
            return;

        playerKart = data.PlayerKart;
        int sectorCount = data.CheckpointCount + 1;
        lapLabels = new TextMeshProUGUI[data.TotalLaps];
        sectorValues = new TextMeshProUGUI[data.TotalLaps, sectorCount];
        panel.sizeDelta = new Vector2(2 * Padding + LapWidth + sectorCount * SectorWidth,
            2 * Padding + (data.TotalLaps + 1) * RowHeight);

        CreateCell(labelTemplate, "Lap Header", "LAP", 0, Padding, LapWidth);
        for (int sector = 0; sector < sectorCount; sector++)
        {
            float x = Padding + LapWidth + sector * SectorWidth;
            CreateCell(labelTemplate, $"Sector {sector + 1} Label", $"S{sector + 1}", 0, x, SectorWidth);
        }

        for (int lap = 0; lap < data.TotalLaps; lap++)
        {
            lapLabels[lap] = CreateCell(labelTemplate, $"Lap {lap + 1} Label", $"{lap + 1}", lap + 1, Padding, LapWidth);
            for (int sector = 0; sector < sectorCount; sector++)
            {
                float x = Padding + LapWidth + sector * SectorWidth;
                sectorValues[lap, sector] = CreateCell(valueTemplate, $"Lap {lap + 1} Sector {sector + 1} Value",
                    "--:--.---", lap + 1, x, SectorWidth);
            }
        }
    }

    public void OnEvent(IEventPublisher<RaceStartData> publisher, EventData<RaceStartData> raceEvent)
    {
        if (sectorValues != null && raceEvent.Data.PlayerKart == playerKart)
            SetActiveLap(raceEvent.Data.LapNumber);
    }

    public void OnEvent(IEventPublisher<SectorCheckData> publisher, EventData<SectorCheckData> sectorEvent)
    {
        SectorCheckData data = sectorEvent.Data;
        if (sectorValues != null && data.PlayerKart == playerKart)
            SetSectorTime(data.LapNumber, data.SectorNumber, data.SectorTime);
    }

    public void OnEvent(IEventPublisher<LapCheckData> publisher, EventData<LapCheckData> lapEvent)
    {
        LapCheckData data = lapEvent.Data;
        if (sectorValues == null || data.PlayerKart != playerKart)
            return;

        SetSectorTime(data.NextLapNumber - 1, sectorValues.GetLength(1), data.LastSectorTime);
        SetActiveLap(data.NextLapNumber);
    }

    public void OnEvent(IEventPublisher<LapInvalidatedData> publisher, EventData<LapInvalidatedData> lapEvent)
    {
        LapInvalidatedData data = lapEvent.Data;
        if (sectorValues == null || data.PlayerKart != playerKart || data.LapNumber < 1 || data.LapNumber > sectorValues.GetLength(0))
            return;

        for (int sector = 0; sector < sectorValues.GetLength(1); sector++)
            sectorValues[data.LapNumber - 1, sector].text = "--:--.---";
    }

    public void OnEvent(IEventPublisher<RaceFinishData> publisher, EventData<RaceFinishData> finishEvent)
    {
        RaceFinishData data = finishEvent.Data;
        if (sectorValues == null || data.PlayerKart != playerKart)
            return;

        SetSectorTime(data.CompletedLaps, sectorValues.GetLength(1), data.LastSectorTime);
        SetActiveLap(0);
    }

    private TextMeshProUGUI CreateCell(TextMeshProUGUI template, string cellName, string text, int row, float x, float width)
    {
        TextMeshProUGUI cell = Instantiate(template, panel);
        cell.name = cellName;
        RectTransform rect = cell.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -Padding - row * RowHeight);
        rect.sizeDelta = new Vector2(width, RowHeight);
        cell.alignment = TextAlignmentOptions.Center;
        cell.text = text;
        cell.gameObject.SetActive(true);
        return cell;
    }

    private void SetActiveLap(int lap)
    {
        if (activeLap > 0 && activeLap <= lapLabels.Length)
            lapLabels[activeLap - 1].color = labelTemplate.color;

        activeLap = lap;
        if (activeLap > 0 && activeLap <= lapLabels.Length)
            lapLabels[activeLap - 1].color = new Color(0.42f, 0.85f, 1f);
    }

    private void SetSectorTime(int lap, int sector, double seconds)
    {
        if (lap < 1 || lap > sectorValues.GetLength(0) || sector < 1 || sector > sectorValues.GetLength(1))
            return;

        sectorValues[lap - 1, sector - 1].text = TimeSpan.FromSeconds(seconds).ToString(@"mm\:ss\.fff");
    }
}
