using System;

public enum BowlingRollMark : byte
{
    None         = 0b0000_0000,
    Split        = 0b0000_1000,
    Strike       = 0b1100_0001,
    Spare        = 0b1100_0010,
    Miss         = 0b1000_0100,
    Empty        = 0b1000_0000,
    HideNumber   = 0b1000_0000,
    IsLaneClear  = 0b0100_0000,
}

[Serializable]
public sealed class BowlingFrameDisplay
{
    public int[] Rolls = Array.Empty<int>();
    public BowlingRollMark[] MarkTypes = Array.Empty<BowlingRollMark>();
    public int Score;
    public bool IsConfirmed;
}

[Serializable]
public sealed class BowlingScoreboardData
{
    public string PlayerName = string.Empty;
    public BowlingFrameDisplay[] Frames = new BowlingFrameDisplay[10];
    public int TotalScore;
    public int CurrentFrame = 1;
    public int CurrentRoll = 1;
    public int RemainingPins = 10;
    public bool IsFinished;
}
