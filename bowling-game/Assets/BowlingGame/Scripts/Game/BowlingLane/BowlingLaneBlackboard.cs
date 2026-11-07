using System;
using System.Collections.Generic;
using UnityEngine;

// 레인별 공유 기록입니다. 판정과 이벤트 발행은 담당하지 않습니다.
[Serializable]
public sealed class BowlingLaneBlackboard
{
    public const ushort FullRackMask = 0b11_1111_1111;

    [SerializeField]
    private List<BowlingFrameRecord> frames = new List<BowlingFrameRecord>(10);

    public IReadOnlyList<BowlingFrameRecord> Frames => frames;
    [field: SerializeField] public int LaneNumber { get; private set; }
    [field: SerializeField] public string PlayerName { get; private set; }
    [field: SerializeField] public int CurrentFrameNumber { get; private set; } = 1;
    [field: SerializeField] public int CurrentRollNumber { get; private set; } = 1;
    [field: SerializeField] public ushort StandingPinMask { get; private set; } = FullRackMask;
    [field: SerializeField] public int TotalScore { get; internal set; }
    [field: SerializeField] public bool IsStarted { get; private set; }
    [field: SerializeField] public bool IsFinished { get; private set; }
    [field: SerializeField] public bool AwaitingPinSetup { get; private set; }

    public BowlingFrameRecord GetFrame(int frameNumber)
    {
        return frameNumber >= 1 && frameNumber <= frames.Count ? frames[frameNumber - 1] : null;
    }

    internal void BeginGame(int laneNumber, string playerName)
    {
        frames.Clear();
        for (int i = 1; i <= 10; ++i)
            frames.Add(new BowlingFrameRecord(i));
        LaneNumber = laneNumber;
        PlayerName = playerName ?? string.Empty;
        CurrentFrameNumber = 1;
        CurrentRollNumber = 1;
        StandingPinMask = FullRackMask;
        TotalScore = 0;
        IsStarted = true;
        IsFinished = false;
        AwaitingPinSetup = false;
    }

    internal void Record(BowlingRollRecord roll, BowlingRollDecision decision)
    {
        var frame = GetFrame(roll.FrameNumber);
        frame.AddRoll(roll);
        frame.IsComplete = decision.IsFrameComplete;
        CurrentFrameNumber = decision.NextFrameNumber;
        CurrentRollNumber = decision.NextRollNumber;
        StandingPinMask = decision.NextRollStandingPinMask;
        IsFinished = decision.IsGameFinished;
        AwaitingPinSetup = !IsFinished;
    }

    internal bool CompletePinSetup()
    {
        if (!IsStarted || IsFinished || !AwaitingPinSetup)
            return false;
        AwaitingPinSetup = false;
        return true;
    }
}

[Serializable]
public sealed class BowlingFrameRecord
{
    [SerializeField]
    private List<BowlingRollRecord> rolls = new List<BowlingRollRecord>(3);

    public IReadOnlyList<BowlingRollRecord> Rolls => rolls;
    [field: SerializeField] public int FrameNumber { get; private set; }
    [field: SerializeField] public int Score { get; internal set; }
    [field: SerializeField] public int TotalScore { get; internal set; }
    [field: SerializeField] public bool IsScoreConfirmed { get; internal set; }
    [field: SerializeField] public bool IsComplete { get; internal set; }

    internal BowlingFrameRecord(int frameNumber)
    {
        FrameNumber = frameNumber;
    }

    internal void AddRoll(BowlingRollRecord roll)
    {
        rolls.Add(roll);
    }
}

/// <summary>한 번의 투구에서 핀 상태가 어떻게 바뀌었는지 기록합니다.</summary>
[Serializable]
public sealed class BowlingRollRecord
{
    [field: SerializeField] public int FrameNumber { get; private set; }
    [field: SerializeField] public int RollNumber { get; private set; }
    [field: SerializeField] public ushort BeforePinMask { get; private set; }
    [field: SerializeField] public ushort AfterPinMask { get; private set; }
    [field: SerializeField] public int PinCount { get; private set; }
    [field: SerializeField] public int RemainingPinCount { get; private set; }
    [field: SerializeField] public BowlingRollMark Mark { get; private set; }
    public ushort FallenPinMask => (ushort)(BeforePinMask & ~AfterPinMask);

    internal BowlingRollRecord(int frameNumber, int rollNumber, ushort beforePinMask,
        ushort afterPinMask, int pinCount, int remainingPinCount, BowlingRollMark mark)
    {
        FrameNumber = frameNumber;
        RollNumber = rollNumber;
        BeforePinMask = beforePinMask;
        AfterPinMask = afterPinMask;
        PinCount = pinCount;
        RemainingPinCount = remainingPinCount;
        Mark = mark;
    }
}
