public enum EventType
{ None, TrackGateEntered, GameStarted, LapChecked, RaceStarted, RaceFinished, SectorChecked, LapInvalidated }

public class EventData
{
    public EventType Type { get; private set; }

    public EventData(EventType type)
    {
        Type = type;
    }

    public override string ToString() => Type.ToString();
}

public class EventData<T> where T : struct
{
    public EventType Type { get; private set; }
    public T Data { get; set; }

    public EventData(EventType type, T data)
    {
        Type = type;
        Data = data;
    }

    public override string ToString() => $"{Type} {Data}";
}
