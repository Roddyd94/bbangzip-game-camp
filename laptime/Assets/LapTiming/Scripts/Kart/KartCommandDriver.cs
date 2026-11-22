using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(KartMotor))]
public sealed class KartCommandDriver : MonoBehaviour
{
    [SerializeField] private InputActionAsset inputActions;

    private KartPlayerDriveSource playerSource;
    private KartReplayDriveSource replaySource;
    private IKartDriveSource activeSource;
    private KartMotor motor;
    private Rigidbody body;
    private KartReplay recording;
    private bool externallyTicked;

    private void Awake()
    {
        motor = GetComponent<KartMotor>();
        body = GetComponent<Rigidbody>();
        playerSource = new KartPlayerDriveSource(inputActions);
        replaySource = new KartReplayDriveSource(body);
        activeSource = playerSource;
    }

    private void OnEnable()
    {
        playerSource.SetActive(activeSource == playerSource);
    }

    private void OnDisable()
    {
        playerSource.SetActive(false);
        replaySource.ReleaseFinalPose();
    }

    private void OnDestroy() => playerSource.Dispose();

    private void FixedUpdate()
    {
        if (externallyTicked)
            return;

        PrepareTick();
        ExecuteTick(Time.fixedDeltaTime);
    }

    internal void UseExternalTick()
    {
        externallyTicked = true;
    }

    internal void PrepareTick()
    {
        if (activeSource == replaySource)
            replaySource.PrepareTick();
    }

    internal void ExecuteTick(float step)
    {
        int tick = recording?.commands.Count ?? 0;
        if (!activeSource.TryGetCommand(tick, step, out KartDriveCommand command))
        {
            if (!playerSource.InputEnabled)
            {
                replaySource.HoldFinalPose();
                return;
            }

            SwitchSource(playerSource);
            playerSource.TryGetCommand(tick, step, out command);
        }

        recording?.commands.Add(command);
        command.Execute(motor, step);
    }

    public void SetPlayerInputEnabled(bool enabled)
    {
        playerSource.SetInputEnabled(enabled);
    }

    internal void UseDriveSource(IKartDriveSource driveSource)
    {
        SwitchSource(driveSource);
    }

    public void BeginRecording()
    {
        if (activeSource == replaySource)
            SwitchSource(playerSource);

        recording = new KartReplay
        {
            startPosition = body.position,
            startRotation = body.rotation,
            hasInitialVelocities = true,
            startLinearVelocity = body.linearVelocity,
            startAngularVelocity = body.angularVelocity,
            fixedStep = Time.fixedDeltaTime
        };
    }

    public KartReplay EndRecording()
    {
        KartReplay replay = recording;
        recording = null;
        return replay;
    }

    public bool PlayReplay(KartReplay replay)
    {
        if (!replaySource.TryLoad(replay, Time.fixedDeltaTime))
            return false;

        recording = null;
        SwitchSource(replaySource);
        return true;
    }

    private void SwitchSource(IKartDriveSource nextSource)
    {
        if (activeSource == replaySource)
            replaySource.ReleaseFinalPose();

        activeSource = nextSource;
        playerSource.SetActive(isActiveAndEnabled && activeSource == playerSource);
    }
}
