using UnityEngine;

public readonly struct GameStartData
{
    public Rigidbody PlayerKart { get; }
    public int CheckpointCount { get; }
    public int TotalLaps { get; }

    public GameStartData(Rigidbody playerKart, int checkpointCount, int totalLaps)
    {
        PlayerKart = playerKart;
        CheckpointCount = checkpointCount;
        TotalLaps = totalLaps;
    }

    public override string ToString()
    {
        return $"GameStartData(PlayerKart={(PlayerKart != null ? PlayerKart.name : "<none>")}, CheckpointCount={CheckpointCount}, TotalLaps={TotalLaps})";
    }
}

public readonly struct RaceStartData
{
    public Rigidbody PlayerKart { get; }
    public int LapNumber { get; }
    public double LapStartedAt { get; }

    public RaceStartData(Rigidbody playerKart, int lapNumber, double lapStartedAt)
    {
        PlayerKart = playerKart;
        LapNumber = lapNumber;
        LapStartedAt = lapStartedAt;
    }

    public override string ToString()
    {
        return $"RaceStartData(PlayerKart={(PlayerKart != null ? PlayerKart.name : "<none>")}, LapNumber={LapNumber}, LapStartedAt={LapStartedAt:F3})";
    }
}

public readonly struct SectorCheckData
{
    public Rigidbody PlayerKart { get; }
    public int LapNumber { get; }
    public int SectorNumber { get; }
    public double SectorTime { get; }

    public SectorCheckData(Rigidbody playerKart, int lapNumber, int sectorNumber, double sectorTime)
    {
        PlayerKart = playerKart;
        LapNumber = lapNumber;
        SectorNumber = sectorNumber;
        SectorTime = sectorTime;
    }

    public override string ToString()
    {
        return $"SectorCheckData(PlayerKart={(PlayerKart != null ? PlayerKart.name : "<none>")}, LapNumber={LapNumber}, SectorNumber={SectorNumber}, SectorTime={SectorTime:F3})";
    }
}

public readonly struct LapCheckData
{
    public Rigidbody PlayerKart { get; }
    public int NextLapNumber { get; }
    public double LapStartedAt { get; }
    public double LastLapTime { get; }
    public double LastSectorTime { get; }
    public double BestLapTime { get; }

    public LapCheckData(Rigidbody playerKart, int nextLapNumber, double lapStartedAt, double lastLapTime, double lastSectorTime, double bestLapTime)
    {
        PlayerKart = playerKart;
        NextLapNumber = nextLapNumber;
        LapStartedAt = lapStartedAt;
        LastLapTime = lastLapTime;
        LastSectorTime = lastSectorTime;
        BestLapTime = bestLapTime;
    }

    public override string ToString()
    {
        return $"LapCheckData(PlayerKart={(PlayerKart != null ? PlayerKart.name : "<none>")}, NextLapNumber={NextLapNumber}, LapStartedAt={LapStartedAt:F3}, LastLapTime={LastLapTime:F3}, LastSectorTime={LastSectorTime:F3}, BestLapTime={BestLapTime:F3})";
    }
}

public readonly struct RaceFinishData
{
    public Rigidbody PlayerKart { get; }
    public int CompletedLaps { get; }
    public double LastLapTime { get; }
    public double LastSectorTime { get; }
    public double BestLapTime { get; }
    public double TotalRaceTime { get; }
    public bool ReplayStarted { get; }

    public RaceFinishData(Rigidbody playerKart, int completedLaps, double lastLapTime, double lastSectorTime, double bestLapTime, double totalRaceTime, bool replayStarted)
    {
        PlayerKart = playerKart;
        CompletedLaps = completedLaps;
        LastLapTime = lastLapTime;
        LastSectorTime = lastSectorTime;
        BestLapTime = bestLapTime;
        TotalRaceTime = totalRaceTime;
        ReplayStarted = replayStarted;
    }

    public override string ToString()
    {
        return $"RaceFinishData(PlayerKart={(PlayerKart != null ? PlayerKart.name : "<none>")}, CompletedLaps={CompletedLaps}, LastLapTime={LastLapTime:F3}, LastSectorTime={LastSectorTime:F3}, BestLapTime={BestLapTime:F3}, TotalRaceTime={TotalRaceTime:F3}, ReplayStarted={ReplayStarted})";
    }
}

public readonly struct LapInvalidatedData
{
    public Rigidbody PlayerKart { get; }
    public int LapNumber { get; }
    public int ExpectedCheckpoint { get; }
    public double LapStartedAt { get; }

    public LapInvalidatedData(Rigidbody playerKart, int lapNumber, int expectedCheckpoint, double lapStartedAt)
    {
        PlayerKart = playerKart;
        LapNumber = lapNumber;
        ExpectedCheckpoint = expectedCheckpoint;
        LapStartedAt = lapStartedAt;
    }

    public override string ToString()
    {
        return $"LapInvalidatedData(PlayerKart={(PlayerKart != null ? PlayerKart.name : "<none>")}, LapNumber={LapNumber}, ExpectedCheckpoint={ExpectedCheckpoint}, LapStartedAt={LapStartedAt:F3})";
    }
}
