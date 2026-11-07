using TMPro;
using UnityEngine;

public sealed class BowlingScoreboardView
    : MonoBehaviour, IEventSubscriber<GameData>, IEventSubscriber<FrameData>, IEventSubscriber<RollData>
{
    [SerializeField]
    private BowlingScoreboardFrameView[] frameViews = new BowlingScoreboardFrameView[10];
    [SerializeField]
    private TMP_Text laneLabel;
    [SerializeField]
    private TMP_Text playerLabel;
    [SerializeField]
    private TMP_Text statusLabel;
    [SerializeField]
    private TMP_Text totalLabel;
    [SerializeField]
    private TMP_Text frameLabel;
    [SerializeField]
    private TMP_Text rollLabel;
    [SerializeField]
    private TMP_Text remainingLabel;

    [SerializeField]
    private BowlingLane lane;
    [SerializeField]
    private BowlingUiText uiText;

    public int LaneNumber => lane.LaneNumber;

    private void OnEnable()
    {
        if (lane == null)
            return;

        laneLabel.text = string.Format(uiText.laneLabelFormat, LaneNumber);
        (this as IEventSubscriberBase)?.Subscribe(EventType.GameStart);
        (this as IEventSubscriberBase)?.Subscribe(EventType.GameEnd);
        (this as IEventSubscriberBase)?.Subscribe(EventType.FrameStart);
        (this as IEventSubscriberBase)?.Subscribe(EventType.FrameEnd);
        (this as IEventSubscriberBase)?.Subscribe(EventType.RollStart);
        (this as IEventSubscriberBase)?.Subscribe(EventType.RollEnd);
    }

    private void OnDisable()
    {
        (this as IEventSubscriberBase)?.Unsubscribe(EventType.GameStart);
        (this as IEventSubscriberBase)?.Unsubscribe(EventType.GameEnd);
        (this as IEventSubscriberBase)?.Unsubscribe(EventType.FrameStart);
        (this as IEventSubscriberBase)?.Unsubscribe(EventType.FrameEnd);
        (this as IEventSubscriberBase)?.Unsubscribe(EventType.RollStart);
        (this as IEventSubscriberBase)?.Unsubscribe(EventType.RollEnd);
    }

    public void OnEvent(IEventPublisher<RollData> publisher, EventData<RollData> eventData)
    {
        if (null == publisher)
            return;

        if (null == eventData)
            return;
        if (eventData.Type != EventType.RollStart && eventData.Type != EventType.RollEnd)
            return;

        RollData data = eventData.Data;
        if (data.LaneNumber != LaneNumber || data.FrameNumber < 1 || data.FrameNumber > 10)
            return;
        if (frameViews == null || data.FrameNumber > frameViews.Length)
            return;

        var frame = frameViews[data.FrameNumber - 1];
        int rollCount = data.FrameNumber == 10 ? 3 : 2;
        if (frame == null || data.RollNumber < 1 || data.RollNumber > rollCount)
            return;
        if (frame.RollViews == null || data.RollNumber > frame.RollViews.Length)
            return;

        var roll = frame.RollViews[data.RollNumber - 1];
        if (roll == null)
            return;

        SetActiveFrame(data.FrameNumber);
        frameLabel.text = string.Format(uiText.frameLabelFormat, data.FrameNumber);
        rollLabel.text = string.Format(uiText.rollLabelFormat, data.RollNumber);
        remainingLabel.text = Mathf.Clamp(data.RemainingPinCount, 0, 10).ToString(uiText.pinNumberFormat);
        statusLabel.text = eventData.Type == EventType.RollStart ? uiText.waitingForRoll : uiText.rollComplete;

        if (eventData.Type == EventType.RollStart)
            return;

        bool miss = (data.Mark & BowlingRollMark.Miss) == BowlingRollMark.Miss;
        bool spare = (data.Mark & BowlingRollMark.Spare) == BowlingRollMark.Spare;
        bool strike = (data.Mark & BowlingRollMark.Strike) == BowlingRollMark.Strike;
        bool split = (data.Mark & BowlingRollMark.Split) == BowlingRollMark.Split;

        if ((data.Mark & BowlingRollMark.HideNumber) == BowlingRollMark.HideNumber)
            roll.RollValue = string.Empty;
        else
            roll.RollValue = Mathf.Clamp(data.PinCount, 0, 10).ToString(uiText.pinNumberFormat);

        roll.IsMiss = miss;
        roll.IsSpare = spare;
        roll.IsStrike = strike;
        roll.IsSplit = split;
    }

    public void OnEvent(IEventPublisher<FrameData> publisher, EventData<FrameData> eventData)
    {
        if (null == publisher)
            return;

        if (null == eventData)
            return;
        if (eventData.Type != EventType.FrameStart && eventData.Type != EventType.FrameEnd)
            return;

        FrameData data = eventData.Data;
        if (data.LaneNumber != LaneNumber || data.FrameNumber < 1 || data.FrameNumber > 10)
            return;
        if (frameViews == null || data.FrameNumber > frameViews.Length)
            return;

        var frame = frameViews[data.FrameNumber - 1];
        if (frame == null)
            return;

        if (eventData.Type == EventType.FrameStart)
        {
            SetActiveFrame(data.FrameNumber);
            frameLabel.text = string.Format(uiText.frameLabelFormat, data.FrameNumber);
            rollLabel.text = string.Format(uiText.rollLabelFormat, 1);
            remainingLabel.text = 10.ToString(uiText.pinNumberFormat);
            statusLabel.text = uiText.waitingForRoll;
            return;
        }

        // 보너스 확정 후 이전 프레임을 갱신해도 현재 진행 표시는 이동하지 않습니다.
        if (data.IsConfirmed)
            frame.Score = Mathf.Clamp(data.Score, 0, 300).ToString(uiText.scoreNumberFormat);
        totalLabel.text = Mathf.Clamp(data.TotalScore, 0, 300).ToString(uiText.scoreNumberFormat);
    }

    private void SetActiveFrame(int frameNumber)
    {
        if (frameViews == null)
            return;

        for (int i = 0; i < frameViews.Length; ++i)
        {
            if (frameViews[i] != null)
                frameViews[i].IsActiveFrame = i == frameNumber - 1;
        }
    }

    public void OnEvent(IEventPublisher<GameData> publisher, EventData<GameData> eventData)
    {
        if (null == publisher)
            return;

        if (null == eventData)
            return;
        if (eventData.Type != EventType.GameStart && eventData.Type != EventType.GameEnd)
            return;

        GameData data = eventData.Data;
        if (data.LaneNumber != LaneNumber)
            return;

        if (eventData.Type == EventType.GameEnd)
        {
            totalLabel.text = Mathf.Clamp(data.TotalScore, 0, 300).ToString(uiText.scoreNumberFormat);
            SetActiveFrame(0);
            statusLabel.text = uiText.gameOver;
            rollLabel.text = uiText.complete;
            return;
        }

        playerLabel.text = string.IsNullOrWhiteSpace(data.PlayerName) ? uiText.playerName : data.PlayerName;
        totalLabel.text = 0.ToString(uiText.scoreNumberFormat);
        frameLabel.text = string.Format(uiText.frameLabelFormat, 1);
        rollLabel.text = string.Format(uiText.rollLabelFormat, 1);
        remainingLabel.text = 10.ToString(uiText.pinNumberFormat);
        statusLabel.text = uiText.waitingForRoll;

        // 이전 경기의 숫자와 아이콘을 함께 지웁니다.
        if (frameViews != null)
        {
            foreach (var frame in frameViews)
                frame.ResetFrameView();
        }

        SetActiveFrame(1);
    }
}
