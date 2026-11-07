using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BowlingReferee), typeof(BowlingLanePublisher))]
public sealed class BowlingLane : MonoBehaviour
{
    public event Action<EventType, GameData> GameEvent;
    public event Action<EventType, FrameData> FrameEvent;
    public event Action<EventType, RollData> RollEvent;
    public event Action<BowlingRollDecision> PinSetupRequested;

    public int LaneNumber => laneNumber;
    public BowlingLaneBlackboard Blackboard => blackboard;
    public BowlingReferee Referee => referee;
    public BowlingPinSetter PinSetter => pinSetter;
    public BowlingBall Ball => ball;
    public bool IsResolvingRoll => waitAndSweepCoroutine != null;
    public BowlingAimPreview AimPreview => aimPreview;
    public Transform BallStartPoint => ballStartPoint;
    public Transform HeadPinPoint => headPinPoint;

    [SerializeField]
    private BowlingLaneEnd laneEnd;
    [SerializeField]
    private List<BowlingPin> pins = new List<BowlingPin>();
    [SerializeField]
    private int laneNumber = 1;
    [SerializeField]
    private string defaultPlayerName;
    [SerializeField, Range(0f, 10f)]
    private float settleSeconds = 3f;
    [SerializeField]
    private BowlingReferee referee;
    [SerializeField]
    private BowlingLanePublisher eventPublisher;
    [SerializeField]
    private BowlingPinSetter pinSetter;
    [SerializeField]
    private BowlingBall ball;
    [SerializeField]
    private BowlingAimPreview aimPreview;
    [SerializeField]
    private Transform ballStartPoint;
    [SerializeField]
    private Transform headPinPoint;
    [SerializeField]
    private BowlingLaneBlackboard blackboard = new BowlingLaneBlackboard();

    private Coroutine waitAndSweepCoroutine;
    private bool startsNewFrame;

    private void Start()
    {
        if (!blackboard.IsStarted)
            BeginGame();
    }

    private void OnEnable()
    {
        laneEnd.BallReached += OnBallReached;
        eventPublisher.Subscribe(this);
    }

    private void OnDisable()
    {
        laneEnd.BallReached -= OnBallReached;
        eventPublisher.Unsubscribe(this);
        StopWaiting();
    }

    public void BeginGame(string playerName = null)
    {
        StopWaiting();
        blackboard.BeginGame(laneNumber, string.IsNullOrWhiteSpace(playerName) ? defaultPlayerName : playerName);
        GameEvent?.Invoke(EventType.GameStart, GetGameData());
        EmitFrameStart();
        EmitRollStart();
    }

    private void OnBallReached(BowlingLaneEnd sender, BowlingBall ball)
    {
        if (sender == laneEnd && ball == Ball)
            OnBallReachLaneEnd();
    }

    public void OnBallReachLaneEnd()
    {
        if (!blackboard.IsStarted
            || blackboard.IsFinished
            || blackboard.AwaitingPinSetup
            || waitAndSweepCoroutine != null)
            return;

        waitAndSweepCoroutine = StartCoroutine(WaitAndSweep());
    }

    private IEnumerator WaitAndSweep()
    {
        yield return new WaitForSeconds(settleSeconds);

        waitAndSweepCoroutine = null;

        if (TryCapturePinMask(out ushort standingPinMask))
            ResolveRoll(standingPinMask);
    }

    private bool TryCapturePinMask(out ushort standingPinMask)
    {
        standingPinMask = 0;
        ushort checkedPinMask = 0;
        foreach (var pin in pins)
        {
            if (pin == null
                || pin.PinNumber < 1
                || pin.PinNumber > 10)
                return false;

            ushort pinBit = (ushort)(1 << (pin.PinNumber - 1));
            if ((checkedPinMask & pinBit) != 0)
                return false;

            checkedPinMask |= pinBit;
            if (pin.IsStanding)
                standingPinMask |= pinBit;
        }
        return checkedPinMask == BowlingLaneBlackboard.FullRackMask;
    }

    // 관측한 핀 결과를 심판에게 전달하고 확정된 결과를 로컬 이벤트로 알립니다.
    public bool ResolveRoll(ushort standingPinMask)
    {
        var previousConfirmed = new bool[10];
        var previousScores = new int[10];
        for (int i = 0; i < blackboard.Frames.Count; ++i)
        {
            previousConfirmed[i] = blackboard.Frames[i].IsScoreConfirmed;
            previousScores[i] = blackboard.Frames[i].TotalScore;
        }
        if (!referee.TryResolveRoll(blackboard, standingPinMask, out var roll, out var decision))
            return false;

        RollEvent?.Invoke(EventType.RollEnd, new RollData
        {
            LaneNumber = laneNumber,
            FrameNumber = roll.FrameNumber,
            RollNumber = roll.RollNumber,
            PinCount = roll.PinCount,
            RemainingPinCount = roll.RemainingPinCount,
            Mark = roll.Mark
        });
        foreach (var frame in blackboard.Frames)
        {
            int index = frame.FrameNumber - 1;
            if (frame.IsComplete && ((frame.FrameNumber == roll.FrameNumber && decision.IsFrameComplete)
                || frame.IsScoreConfirmed != previousConfirmed[index]
                || frame.TotalScore != previousScores[index]))
                EmitFrameResult(frame);
        }

        if (decision.IsGameFinished)
            GameEvent?.Invoke(EventType.GameEnd, GetGameData());
        else
        {
            startsNewFrame = decision.NextFrameNumber != roll.FrameNumber;
            PinSetupRequested?.Invoke(decision);
        }
        return true;
    }

    // 핀세터가 보존/재배치를 마친 뒤 호출합니다. 물리 배치 완료 전에 다음 투구를 열지 않습니다.
    public bool CompletePinSetup()
    {
        if (!blackboard.CompletePinSetup())
            return false;
        if (startsNewFrame)
            EmitFrameStart();
        EmitRollStart();
        return true;
    }

    private void StopWaiting()
    {
        if (waitAndSweepCoroutine == null)
            return;
        StopCoroutine(waitAndSweepCoroutine);
        waitAndSweepCoroutine = null;
    }

    private GameData GetGameData()
    {
        return new GameData { LaneNumber = laneNumber, PlayerName = blackboard.PlayerName, TotalScore = blackboard.TotalScore };
    }

    private void EmitFrameStart()
    {
        FrameEvent?.Invoke(EventType.FrameStart, new FrameData
        {
            LaneNumber = laneNumber,
            FrameNumber = blackboard.CurrentFrameNumber,
            TotalScore = blackboard.TotalScore
        });
    }

    private void EmitRollStart()
    {
        RollEvent?.Invoke(EventType.RollStart, new RollData
        {
            LaneNumber = laneNumber,
            FrameNumber = blackboard.CurrentFrameNumber,
            RollNumber = blackboard.CurrentRollNumber,
            RemainingPinCount = BowlingReferee.CountPins(blackboard.StandingPinMask)
        });
    }

    private void EmitFrameResult(BowlingFrameRecord frame)
    {
        FrameEvent?.Invoke(EventType.FrameEnd, new FrameData
        {
            LaneNumber = laneNumber,
            FrameNumber = frame.FrameNumber,
            Score = frame.TotalScore,
            TotalScore = blackboard.TotalScore,
            IsConfirmed = frame.IsScoreConfirmed
        });
    }
}

