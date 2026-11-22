using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Splines;

[RequireComponent(typeof(KartMotor), typeof(Rigidbody))]
public sealed class KartAiDriveSource : MonoBehaviour, IKartDriveSource
{
    private Rigidbody body;
    private SplineContainer track;
    private float trackLength;
    private float progress;
    private float stoppedFor;
    private float reverseFor;
    private float cruiseSpeed = 7f;

    public float UpcomingTurn { get; private set; }

    internal void Configure(SplineContainer route)
    {
        track = route;
        trackLength = track.CalculateLength();
    }

    internal void ResetProgress()
    {
        Vector3 position = track.transform.InverseTransformPoint(body.position);
        SplineUtility.GetNearestPoint(track.Spline, (Unity.Mathematics.float3)position, out _, out progress);
        stoppedFor = 0f;
        reverseFor = 0f;
    }

    public bool TryGetCommand(int tick, float fixedDeltaTime, out KartDriveCommand command)
    {
        if (trackLength <= 0f)
        {
            command = new KartDriveCommand(tick, Vector2.zero);
            return true;
        }

        Vector3 position = body.position;
        // Search around the last known point so nearby sections of the loop do not steal progress.
        float bestDistance = float.PositiveInfinity;
        float nextProgress = progress;
        for (int i = -2; i <= 8; i++)
        {
            float candidate = Mathf.Repeat(progress + i * 0.5f / trackLength, 1f);
            float distance = ((Vector3)track.EvaluatePosition(candidate) - position).sqrMagnitude;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nextProgress = candidate;
            }
        }
        progress = nextProgress;

        Vector3 target = track.EvaluatePosition(Mathf.Repeat(progress + 3f / trackLength, 1f));
        if (NavMesh.SamplePosition(target, out NavMeshHit road, 1f, NavMesh.AllAreas))
            target = road.position;

        Vector3 forward = Vector3.ProjectOnPlane(body.rotation * Vector3.back, Vector3.up).normalized;
        Vector3 direction = Vector3.ProjectOnPlane(target - position, Vector3.up);
        float angle = Vector3.SignedAngle(forward, direction, Vector3.up);
        UpcomingTurn = Mathf.Clamp01(Mathf.Abs(angle) / 65f);
        float steer = Mathf.Clamp(angle / 30f, -1f, 1f);
        float targetSpeed = cruiseSpeed * Mathf.Lerp(1f, 0.32f, Mathf.Clamp01(Mathf.Abs(angle) / 65f));
        float speed = Vector3.Dot(body.linearVelocity, forward);
        if (reverseFor > 0f)
        {
            reverseFor -= fixedDeltaTime;
            command = new KartDriveCommand(tick, new Vector2(-steer, -0.65f));
            return true;
        }

        stoppedFor = Mathf.Abs(speed) < 0.15f ? stoppedFor + fixedDeltaTime : 0f;
        if (stoppedFor >= 1.5f)
        {
            stoppedFor = 0f;
            reverseFor = 0.8f;
        }

        float throttle = Mathf.Clamp((targetSpeed - speed) * 0.65f, -1f, 1f);
        command = new KartDriveCommand(tick, new Vector2(steer, throttle));
        return true;
    }

    public void SetCruiseSpeed(float speed)
    {
        cruiseSpeed = Mathf.Max(1f, speed);
    }

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
    }
}
