using UnityEngine;

// 레인 내부 통신은 로컬 이벤트, 점수판·세션 통신은 EventBus를 사용합니다.
public sealed class BowlingLanePublisher : MonoBehaviour,
    IEventPublisher<GameData>, IEventPublisher<FrameData>, IEventPublisher<RollData>
{
    public void Subscribe(BowlingLane lane)
    {
        lane.GameEvent += OnGameEvent;
        lane.FrameEvent += OnFrameEvent;
        lane.RollEvent += OnRollEvent;
    }

    public void Unsubscribe(BowlingLane lane)
    {
        lane.GameEvent -= OnGameEvent;
        lane.FrameEvent -= OnFrameEvent;
        lane.RollEvent -= OnRollEvent;
    }

    private void OnGameEvent(EventType type, GameData data)
    {
        Publish(new EventData<GameData>(type, data));
    }

    private void OnFrameEvent(EventType type, FrameData data)
    {
        Publish(new EventData<FrameData>(type, data));
    }

    private void OnRollEvent(EventType type, RollData data)
    {
        Publish(new EventData<RollData>(type, data));
    }

    public void Publish(EventData<GameData> eventData)
    {
        if (eventData != null)
            EventBus.Instance.Publish(this, eventData);
    }

    public void Publish(EventData<FrameData> eventData)
    {
        if (eventData != null)
            EventBus.Instance.Publish(this, eventData);
    }

    public void Publish(EventData<RollData> eventData)
    {
        if (eventData != null)
            EventBus.Instance.Publish(this, eventData);
    }
}
