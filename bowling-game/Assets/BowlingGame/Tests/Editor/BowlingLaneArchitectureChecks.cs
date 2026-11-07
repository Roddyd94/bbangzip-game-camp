using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

// 물리 시뮬레이션과 분리해서 기록·판정·이벤트 경계를 검증합니다.
public static class BowlingLaneArchitectureChecks
{
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static int checks;

    private sealed class Observer : IEventSubscriber<GameData>, IEventSubscriber<RollData>
    {
        public int Starts;
        public RollData LastRoll;
        public IEventPublisherBase LastPublisher;

        public void OnEvent(IEventPublisher<GameData> publisher, EventData<GameData> data)
        {
            ++Starts;
            LastPublisher = publisher;
        }

        public void OnEvent(IEventPublisher<RollData> publisher, EventData<RollData> data)
        {
            LastRoll = data.Data;
            LastPublisher = publisher;
        }
    }

    [MenuItem("Tools/Bowling/Validate Lane Architecture")]
    public static void Validate()
    {
        Debug.Log(Run());
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
            throw new Exception(message);
        ++checks;
    }

    private static BowlingLane CreateLane(int number)
    {
        var root = new GameObject("Test Lane " + number);
        objects.Add(root);
        var lane = root.AddComponent<BowlingLane>();
        var so = new SerializedObject(lane);
        so.FindProperty("laneNumber").intValue = number;
        so.FindProperty("referee").objectReferenceValue = root.GetComponent<BowlingReferee>();
        so.FindProperty("eventPublisher").objectReferenceValue = root.GetComponent<BowlingLanePublisher>();
        so.ApplyModifiedPropertiesWithoutUndo();
        root.GetComponent<BowlingLanePublisher>().Subscribe(lane);
        lane.BeginGame();
        return lane;
    }

    private static void Bowl(BowlingLane lane, int pinCount)
    {
        if (lane.Blackboard.AwaitingPinSetup)
            Check(lane.CompletePinSetup(), "Pin setup did not complete");
        ushort mask = lane.Blackboard.StandingPinMask;
        Check(pinCount <= BowlingReferee.CountPins(mask), "Impossible test input");
        for (int i = 0; i < pinCount; ++i)
            mask = (ushort)(mask & (mask - 1));
        Check(lane.ResolveRoll(mask), "Valid roll rejected");
    }

    private static void Game(string name, int[] rolls, int expected)
    {
        var lane = CreateLane(1);
        foreach (int count in rolls)
            Bowl(lane, count);
        Check(lane.Blackboard.IsFinished, name + " did not finish");
        Check(lane.Blackboard.TotalScore == expected, name + " wrong score: " + lane.Blackboard.TotalScore);
        Check(lane.Blackboard.Frames.All(f => f.IsScoreConfirmed), name + " has pending scores");
        Check(lane.Blackboard.Frames.Sum(f => f.Rolls.Count) == rolls.Length, name + " missing history");
        Check(!lane.ResolveRoll(0), name + " accepted roll after game end");
    }

    public static string Run()
    {
        checks = 0;
        var observer = new Observer();
        EventBus.Instance.Subscribe(EventType.GameStart, observer);
        EventBus.Instance.Subscribe(EventType.RollEnd, observer);
        try
        {
            Game("Perfect", Enumerable.Repeat(10, 12).ToArray(), 300);
            Game("All spares", Enumerable.Range(0, 10).SelectMany(i => new[] { 9, 1 }).Concat(new[] { 9 }).ToArray(), 190);
            Game("Gutters", new int[20], 0);
            Game("Open frames", Enumerable.Range(0, 10).SelectMany(i => new[] { 9, 0 }).ToArray(), 90);
            Game("Tenth X 7 /", new int[18].Concat(new[] { 10, 7, 3 }).ToArray(), 20);
            Game("Tenth 7 / X", new int[18].Concat(new[] { 7, 3, 10 }).ToArray(), 20);
            Game("Tenth X X 7", new int[18].Concat(new[] { 10, 10, 7 }).ToArray(), 27);
            Game("Tenth open", new int[18].Concat(new[] { 7, 2 }).ToArray(), 9);
            Game("Tenth X - /", new int[18].Concat(new[] { 10, 0, 10 }).ToArray(), 20);
            Game("Tenth - / -", new int[18].Concat(new[] { 0, 10, 0 }).ToArray(), 10);

            var lane = CreateLane(1);
            Bowl(lane, 10);
            Check(!lane.Blackboard.GetFrame(1).IsScoreConfirmed, "Strike confirmed without bonus");
            Bowl(lane, 3);
            Check(!lane.Blackboard.GetFrame(1).IsScoreConfirmed, "Strike confirmed after one bonus");
            Bowl(lane, 4);
            Check(lane.Blackboard.GetFrame(1).TotalScore == 17 && lane.Blackboard.TotalScore == 24, "Strike bonus wrong");
            lane.BeginGame();
            Bowl(lane, 7);
            Bowl(lane, 3);
            Check(!lane.Blackboard.GetFrame(1).IsScoreConfirmed, "Spare confirmed without bonus");
            Bowl(lane, 4);
            Check(lane.Blackboard.GetFrame(1).TotalScore == 14, "Spare bonus wrong");

            lane.BeginGame();
            Check(lane.ResolveRoll(0b10_0100_0000), "7-10 leave rejected");
            var record = lane.Blackboard.GetFrame(1).Rolls[0];
            Check(record.Mark == BowlingRollMark.Split && record.PinCount == 8, "Split judgement wrong");
            Check(record.BeforePinMask == 1023 && record.AfterPinMask == 576, "Masks not recorded");
            Check(!lane.ResolveRoll(576), "Accepted duplicate before physical setup");
            lane.CompletePinSetup();
            Check(!lane.ResolveRoll(1023), "Accepted regrown pins");
            Check(lane.ResolveRoll(512), "Second roll rejected");
            Check(observer.LastRoll.PinCount == 1 && observer.LastRoll.RemainingPinCount == 1, "Publisher sent cumulative pin count");
            Check(observer.LastPublisher == lane.GetComponent<BowlingLanePublisher>(), "Lane published instead of publisher");
            Check(!(object.Equals(lane.Blackboard.GetFrame(1).Rolls[0], lane.Blackboard.GetFrame(1).Rolls[1])), "History overwritten");
            Check(!lane.Referee.IsSplit(66 | 1) && lane.Referee.IsSplit(66), "Head-pin split handling wrong");
            Check(!lane.Referee.IsSplit(130) && !lane.Referee.IsSplit(260), "Sleeper falsely split");

            var other = CreateLane(2);
            Check(other.Blackboard.LaneNumber == 2 && other.Blackboard.GetFrame(1).Rolls.Count == 0, "Lane histories shared");
            Bowl(other, 5);
            Check(lane.Blackboard.GetFrame(1).Rolls.Count == 2, "Other lane mutated history");
            int starts = observer.Starts;
            var publisher = other.GetComponent<BowlingLanePublisher>();
            publisher.Unsubscribe(other);
            publisher.Subscribe(other);
            other.BeginGame("새 경기");
            Check(observer.Starts == starts + 1, "Publisher reconnect failed");
            Check(other.Blackboard.TotalScore == 0 && other.Blackboard.Frames.All(f => f.Rolls.Count == 0), "Restart failed");
            publisher.Unsubscribe(other);
            starts = observer.Starts;
            other.BeginGame();
            Check(observer.Starts == starts, "Unbound publisher still emitted");
            Check(!other.ResolveRoll(1024), "Out-of-range mask accepted");

            var endObject = new GameObject("Test local LaneEnd");
            objects.Add(endObject);
            var end = endObject.AddComponent<BowlingLaneEnd>();
            int arrivals = 0;
            end.BallReached += (sender, ball) => { ++arrivals; };
            var ballObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objects.Add(ballObject);
            ballObject.AddComponent<Rigidbody>().isKinematic = true;
            ballObject.AddComponent<BowlingBall>();
            end.SendMessage("OnTriggerEnter", ballObject.GetComponent<Collider>());
            Check(arrivals == 1, "Local ball arrival missing");
            var pinObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            objects.Add(pinObject);
            pinObject.AddComponent<Rigidbody>().isKinematic = true;
            end.SendMessage("OnTriggerEnter", pinObject.GetComponent<Collider>());
            Check(arrivals == 1, "Pin treated as ball");
            return "PASS: " + checks + " assertions; 10 complete-game scenarios, pending bonuses, split masks, per-roll history, setup gate, isolated lanes, publisher binding and restart.";
        }
        finally
        {
            EventBus.Instance.Unsubscribe(EventType.GameStart, observer);
            EventBus.Instance.Unsubscribe(EventType.RollEnd, observer);
            foreach (var root in objects)
            {
                if (root != null)
                {
                    var publisher = root.GetComponent<BowlingLanePublisher>();
                    if (publisher != null)
                        publisher.Unsubscribe(root.GetComponent<BowlingLane>());
                    UnityEngine.Object.DestroyImmediate(root);
                }
            }
            objects.Clear();
        }
    }
}
