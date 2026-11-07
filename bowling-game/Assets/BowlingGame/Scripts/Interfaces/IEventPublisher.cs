using UnityEngine;

public interface IEventPublisherBase
{
}

public interface IEventPublisher: IEventPublisherBase
{
    public void Publish(EventData eventData);
}

public interface IEventPublisher<T> : IEventPublisherBase where T : struct
{
    public void Publish(EventData<T> eventData);
}
