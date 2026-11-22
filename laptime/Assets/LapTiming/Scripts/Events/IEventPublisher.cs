using UnityEngine;

public interface IEventPublisher
{
    protected void Publish(EventData eventData)
    {
        EventBus.Instance.Publish(this, eventData);
    }
}

public interface IEventPublisher<T> : IEventPublisher where T : struct
{
    protected void Publish(EventData<T> eventData)
    {
        EventBus.Instance.Publish(this, eventData);
    }
}
