using System.Collections;
using UnityEngine;

// 단계 전환과 데모 진행만 담당합니다. 입력·카메라·화면·물리는 각각의 클래스가 처리합니다.
public sealed class BowlingSession : MonoBehaviour, IEventSubscriber<RollData>, IEventSubscriber<GameData>
{
    [SerializeField]
    private BowlingLane lane;
    [SerializeField]
    private BowlingInput playerInput;
    [SerializeField]
    private BowlingCamera cameraController;
    [SerializeField]
    private BowlingHud hud;
    [SerializeField]
    private BowlingShot shot = new BowlingShot();
    [SerializeField, Range(5f, 30f)]
    private float rollTimeLimit = 15f;

    private BowlingShotStage stage;
    private float rollingSeconds;
    private float stoppedSeconds;
    private bool resultRequested;
    private bool initialized;

    public BowlingShotStage Stage => stage;
    public BowlingShot Shot => shot;
    public BowlingLane Lane => lane;

    private static readonly EventType[] LaneEventTypes =
    {
        EventType.RollStart, EventType.RollEnd, EventType.GameEnd
    };

    private void OnEnable()
    {
        playerInput.Requested += OnRequest;
        hud.Requested += OnRequest;
        foreach (EventType eventType in LaneEventTypes)
            EventBus.Instance.Subscribe(eventType, this);
    }

    private void OnDisable()
    {
        playerInput.Requested -= OnRequest;
        hud.Requested -= OnRequest;
        foreach (EventType eventType in LaneEventTypes)
            EventBus.Instance.Unsubscribe(eventType, this);
    }

    private void OnRequest(BowlingRequest request, float? value)
    {
        switch (request)
        {
        case BowlingRequest.Confirm:
            ConfirmStage();
            break;
        case BowlingRequest.Back:
            PreviousStage();
            break;
        case BowlingRequest.Restart:
            RestartGame();
            break;
        case BowlingRequest.ChangeCamera:
            ChangeCamera();
            break;
        case BowlingRequest.SetStageValue:
            if (value.HasValue)
                SetStageValue(value.Value);
            break;
        }
    }

    private IEnumerator Start()
    {
        yield return null;
        initialized = true;
        RestartGame();
    }

    public void RestartGame()
    {
        if (!initialized)
            return;

        lane.PinSetter.ResetPins();
        lane.Ball.Place(GetBallStartPosition());
        stage = BowlingShotStage.StartPosition;
        resultRequested = false;
        lane.BeginGame(hud.Text.playerName);
        cameraController.ShowSetup(GetBallStartPosition(), lane.HeadPinPoint.position);
        hud.ShowResult(hud.Text.setupHint);
        RefreshHud();
    }

    public void ConfirmStage()
    {
        if (!initialized)
            return;

        if (stage == BowlingShotStage.Finished)
        {
            RestartGame();
            return;
        }

        if (stage < BowlingShotStage.Ready)
            ++stage;
        else if (stage == BowlingShotStage.Ready)
        {
            stage = BowlingShotStage.Rolling;
            rollingSeconds = 0f;
            stoppedSeconds = 0f;
            resultRequested = false;
            lane.Ball.Launch(shot, Vector3.back);
            lane.AimPreview.ResetPreview();
            cameraController.TrackBall(lane.Ball.transform);
            hud.ShowResult(hud.Text.rollingHint);
        }

        RefreshHud();
    }

    public void PreviousStage()
    {
        if (stage > BowlingShotStage.StartPosition
            && stage <= BowlingShotStage.Ready)
            --stage;

        RefreshHud();
    }

    private void ChangeCamera()
    {
        cameraController.CycleView();
        RefreshHud();
    }

    private Vector3 GetBallStartPosition()
    {
        return lane.BallStartPoint.position + Vector3.left * shot.StartOffset;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (stage <= BowlingShotStage.SpinStrength)
        {
            GetStageRange(out float minimum, out float maximum, out float value);
            if (Mathf.Abs(playerInput.Adjustment) > 0.05f)
            {
                float stageValue = value + playerInput.Adjustment * (maximum - minimum) * 0.35f * Time.deltaTime;
                SetStageValue(stageValue);
            }
        }

        if (stage <= BowlingShotStage.Ready)
            lane.Ball.Place(GetBallStartPosition());

        if (stage == BowlingShotStage.Rolling && !resultRequested)
        {
            rollingSeconds += Time.deltaTime;
            stoppedSeconds = lane.Ball.Speed < 0.12f ? stoppedSeconds + Time.deltaTime : 0f;
            if (rollingSeconds >= rollTimeLimit
                || lane.Ball.Position.y < -1f
                || (rollingSeconds > 2f && stoppedSeconds > 1f))
            {
                resultRequested = true;
                lane.OnBallReachLaneEnd();
                hud.ShowResult(hud.Text.stoppedHint);
            }
        }

        lane.AimPreview.Show(GetBallStartPosition(), Vector3.back, shot, stage <= BowlingShotStage.Ready);
        RefreshHud();
    }

    public void SetStageValue(float value)
    {
        switch (stage)
        {
        case BowlingShotStage.StartPosition: shot.StartOffset = Mathf.Clamp(value, -0.4f, 0.4f); break;
        case BowlingShotStage.ThrowAngle: shot.ThrowAngle = Mathf.Clamp(value, -8f, 8f); break;
        case BowlingShotStage.ThrowPower: shot.ThrowPower = Mathf.Clamp(value, 0f, 100f); break;
        case BowlingShotStage.SpinDirection: shot.SpinDirection = Mathf.Clamp(value, -90f, 90f); break;
        case BowlingShotStage.SpinStrength: shot.SpinStrength = Mathf.Clamp(value, 0f, 100f); break;
        }
    }

    private void GetStageRange(out float minimum, out float maximum, out float value)
    {
        minimum = 0f;
        maximum = 100f;
        value = 0f;
        switch (stage)
        {
        case BowlingShotStage.StartPosition: minimum = -0.4f; maximum = 0.4f; value = shot.StartOffset; break;
        case BowlingShotStage.ThrowAngle: minimum = -8f; maximum = 8f; value = shot.ThrowAngle; break;
        case BowlingShotStage.ThrowPower: value = shot.ThrowPower; break;
        case BowlingShotStage.SpinDirection: minimum = -90f; maximum = 90f; value = shot.SpinDirection; break;
        case BowlingShotStage.SpinStrength: value = shot.SpinStrength; break;
        }
    }

    private void RefreshHud()
    {
        float minimum;
        float maximum;
        float sliderValue;

        GetStageRange(out minimum, out maximum, out sliderValue);
        hud.ShowStage(stage, shot, lane.Ball.Speed, lane.Blackboard.TotalScore, minimum, maximum, sliderValue);
        hud.ShowGame(lane.LaneNumber, lane.Blackboard.CurrentFrameNumber, lane.Blackboard.CurrentRollNumber, lane.Blackboard.TotalScore, cameraController.ViewIndex);
    }

    public void OnEvent(IEventPublisher<RollData> publisher, EventData<RollData> eventData)
    {
        if (!initialized || eventData.Data.LaneNumber != lane.LaneNumber)
            return;

        EventType eventType = eventData.Type;
        RollData rollData = eventData.Data;
        if (eventType == EventType.RollEnd)
        {
            resultRequested = true;
            stage = BowlingShotStage.PinSetup;
            lane.Ball.Place(GetBallStartPosition());
            cameraController.ShowPins();
            string mark = rollData.Mark == BowlingRollMark.Strike ? hud.Text.strike : rollData.Mark == BowlingRollMark.Spare ? hud.Text.spare : (rollData.Mark & BowlingRollMark.Split) != 0 ? hud.Text.split : hud.Text.rollResult;
            hud.ShowResult(string.Format(hud.Text.rollResultFormat, mark, rollData.PinCount, rollData.RemainingPinCount));
        }
        else if (eventType == EventType.RollStart)
        {
            stage = BowlingShotStage.StartPosition;
            resultRequested = false;
            lane.Ball.Place(GetBallStartPosition());
            cameraController.ShowSetup(GetBallStartPosition(), lane.HeadPinPoint.position);
        }
    }

    public void OnEvent(IEventPublisher<GameData> publisher, EventData<GameData> eventData)
    {
        if (initialized && eventData.Type == EventType.GameEnd && eventData.Data.LaneNumber == lane.LaneNumber)
        {
            stage = BowlingShotStage.Finished;
            hud.ShowResult(hud.Text.gameEndHint);
        }
    }
}
