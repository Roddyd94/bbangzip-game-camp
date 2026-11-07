using System.Collections.Generic;
using UnityEngine;

public class EventBus
{
    public static EventBus Instance { get; private set; } = new EventBus();

    private Dictionary<EventType, List<IEventSubscriberBase>> subscribers = new Dictionary<EventType, List<IEventSubscriberBase>>();

    private EventBus() { }

    public void Publish(IEventPublisher publisher, EventData eventData)
    {
        if (null == publisher)
            return;

        if (subscribers.ContainsKey(eventData.Type))
        {
            foreach (var subscriber in subscribers[eventData.Type])
            {
                if (subscriber is IEventSubscriber typedSubscriber)
                    typedSubscriber.OnEvent(publisher, eventData);
            }
        }
    }

    public void Publish<T>(IEventPublisher<T> publisher, EventData<T> eventData) where T : struct
    {
        if (null == publisher)
            return;

        if (subscribers.ContainsKey(eventData.Type))
        {
            foreach (var subscriber in subscribers[eventData.Type])
            {
                if (subscriber is IEventSubscriber<T> typedSubscriber)
                    typedSubscriber.OnEvent(publisher, eventData);
            }
        }
    }

    public void Subscribe(EventType eventType, IEventSubscriberBase subscriber)
    {
        if (!subscribers.ContainsKey(eventType))
            subscribers[eventType] = new List<IEventSubscriberBase>();

        subscribers[eventType].Add(subscriber);
    }

    public void Unsubscribe(EventType eventType, IEventSubscriberBase subscriber)
    {
        if (!subscribers.ContainsKey(eventType))
            subscribers[eventType] = new List<IEventSubscriberBase>();

        subscribers[eventType].Remove(subscriber);
    }
}
