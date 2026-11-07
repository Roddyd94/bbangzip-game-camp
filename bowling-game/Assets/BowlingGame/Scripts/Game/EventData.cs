public enum EventType
{
    None = 0,
    GameStart = 1,
    GameEnd = 2,
    FrameStart = 3,
    FrameEnd = 4,
    RollStart = 5,
    RollEnd = 6,
    BallReachedLaneEnd = 7,
    GrabbedRemainPins = 8,
    ReleasedRemainPins = 9,
    PinSweepingCompleted = 10,
}

public class EventData
{
    public EventType Type { get; private set; }

    public EventData(EventType type)
    {
        Type = type;
    }
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
}

[System.Serializable]
public struct GameData
{
    public int LaneNumber;
    public string PlayerName;
    public int TotalScore;
}

// 레인·프레임·투구 번호는 모두 1부터 시작합니다.
[System.Serializable]
public struct FrameData
{
    public int LaneNumber;
    public int FrameNumber;
    public int Score;
    public int TotalScore;
    public bool IsConfirmed;
}

[System.Serializable]
public struct RollData
{
    public int LaneNumber;
    public int FrameNumber;
    public int RollNumber;
    public int PinCount;
    public int RemainingPinCount;
    public BowlingRollMark Mark;
}

[System.Serializable]
public struct BallReachedLaneEndData
{
    public int LaneNumber;
}