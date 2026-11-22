using UnityEngine;

public interface IEventSubscriberBase
{
    public void Subscribe(EventType eventType)
    {
        EventBus.Instance.Subscribe(eventType, this);
    }
    public void Unsubscribe(EventType eventType)
    {
        EventBus.Instance.Unsubscribe(eventType, this);
    }
}

public interface IEventSubscriber : IEventSubscriberBase
{
    public void OnEvent(IEventPublisher publisher, EventData eventData);
}

public interface IEventSubscriber<T> : IEventSubscriberBase where T : struct
{
    public void OnEvent(IEventPublisher<T> publisher, EventData<T> eventData);
}
