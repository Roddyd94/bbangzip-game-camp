using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(BoxCollider))]
public sealed class KartMotor : MonoBehaviour
{
    [SerializeField, Min(0f)] private float acceleration = 12f;
    [SerializeField, Min(0f)] private float maxSpeed = 18f;
    [SerializeField, Min(0f)] private float coastDeceleration = 4f;
    [SerializeField, Range(0f, 45f)] private float maxSteeringAngle = 28f;
    [UnityEngine.Serialization.FormerlySerializedAs("tireMaterial")]
    [SerializeField] private PhysicsMaterial frontTireMaterial;
    [SerializeField] private PhysicsMaterial rearTireMaterial;

    private const float WheelRadius = 0.235f;
    private const float TrackHalfWidth = 0.56f;
    private const float FrontAxleZ = -0.59f;
    private const float RearAxleZ = 0.57f;
    private const float SuspensionTravel = 0.12f;
    private const float SpringStrength = 120f;
    private const float SpringDamping = 6f;
    private const float LateralGripResponse = 10f;

    private Rigidbody body;
    private Transform frontWheelA;
    private Transform rearWheelA;
    private Transform frontWheelB;
    private Transform rearWheelB;
    private Quaternion frontWheelARest;
    private Quaternion rearWheelARest;
    private Quaternion frontWheelBRest;
    private Quaternion rearWheelBRest;
    private float steeringAngle;
    private float frontWheelASpin;
    private float rearWheelASpin;
    private float frontWheelBSpin;
    private float rearWheelBSpin;

    // The imported model faces local -Z.
    public float ThrottleInput { get; private set; }
    public bool HasWheelContact { get; private set; }
    public float TireSlipSpeed { get; private set; }

    private Vector3 Forward => body.rotation * Vector3.back;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if (child.name == "FrontWheelA")
                frontWheelA = child;
            else if (child.name == "RearWheelA")
                rearWheelA = child;
            else if (child.name == "FrontWheelB")
                frontWheelB = child;
            else if (child.name == "RearWheelB")
                rearWheelB = child;
        }

        if (frontWheelA != null && frontWheelB != null
            && transform.InverseTransformPoint(frontWheelA.position).x > transform.InverseTransformPoint(frontWheelB.position).x)
            (frontWheelA, frontWheelB) = (frontWheelB, frontWheelA);

        if (rearWheelA != null && rearWheelB != null
            && transform.InverseTransformPoint(rearWheelA.position).x > transform.InverseTransformPoint(rearWheelB.position).x)
            (rearWheelA, rearWheelB) = (rearWheelB, rearWheelA);

        Quaternion inverseRotation = Quaternion.Inverse(transform.rotation);
        if (frontWheelA != null)
            frontWheelARest = inverseRotation * frontWheelA.rotation;
        if (rearWheelA != null)
            rearWheelARest = inverseRotation * rearWheelA.rotation;
        if (frontWheelB != null)
            frontWheelBRest = inverseRotation * frontWheelB.rotation;
        if (rearWheelB != null)
            rearWheelBRest = inverseRotation * rearWheelB.rotation;
    }

    private void LateUpdate()
    {
        Quaternion steeringRotation = transform.rotation * Quaternion.Euler(0f, steeringAngle, 0f);
        if (frontWheelA != null)
            frontWheelA.rotation = steeringRotation * Quaternion.AngleAxis(frontWheelASpin, Vector3.right) * frontWheelARest;
        if (rearWheelA != null)
            rearWheelA.rotation = transform.rotation * Quaternion.AngleAxis(rearWheelASpin, Vector3.right) * rearWheelARest;
        if (frontWheelB != null)
            frontWheelB.rotation = steeringRotation * Quaternion.AngleAxis(frontWheelBSpin, Vector3.right) * frontWheelBRest;
        if (rearWheelB != null)
            rearWheelB.rotation = transform.rotation * Quaternion.AngleAxis(rearWheelBSpin, Vector3.right) * rearWheelBRest;
    }

    internal void Drive(Vector2 input, float step)
    {
        ThrottleInput = input.y;
        HasWheelContact = false;
        TireSlipSpeed = 0f;
        Vector3 velocity = body.linearVelocity;
        Vector3 planarVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);
        if (planarVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            velocity += Vector3.ClampMagnitude(planarVelocity, maxSpeed) - planarVelocity;
            body.linearVelocity = velocity;
        }

        float forwardSpeed = Vector3.Dot(velocity, Forward);
        float driveAcceleration = input.y * acceleration;
        if (Mathf.Approximately(input.y, 0f))
            driveAcceleration = (Mathf.MoveTowards(forwardSpeed, 0f, coastDeceleration * step) - forwardSpeed) / step;

        float nextSpeed = Mathf.Clamp(forwardSpeed + driveAcceleration * step, -maxSpeed, maxSpeed);
        float rearDriveForce = body.mass * (nextSpeed - forwardSpeed) / (2f * step);

        steeringAngle = input.x * maxSteeringAngle;
        Vector3 steeredForward = body.rotation * Quaternion.Euler(0f, steeringAngle, 0f) * Vector3.back;
        // 앞바퀴는 조향과 횡력만 담당하고, 실제 이동 속도로 굴림 각도를 계산합니다.
        Vector3 frontA = GetWheelCenter(-TrackHalfWidth, FrontAxleZ);
        Vector3 frontB = GetWheelCenter(TrackHalfWidth, FrontAxleZ);
        frontWheelASpin = GetRollingAngle(frontA, steeredForward, frontWheelASpin, step);
        frontWheelBSpin = GetRollingAngle(frontB, steeredForward, frontWheelBSpin, step);
        ApplyWheelForces(frontA, steeredForward, 0f, frontTireMaterial);
        ApplyWheelForces(frontB, steeredForward, 0f, frontTireMaterial);

        // 전후 입력으로 계산한 구동력은 뒷바퀴 접지점에만 전달합니다.
        Vector3 rearA = GetWheelCenter(-TrackHalfWidth, RearAxleZ);
        rearWheelASpin = GetRollingAngle(rearA, Forward, rearWheelASpin, step);
        ApplyWheelForces(rearA, Forward, rearDriveForce, rearTireMaterial);
        Vector3 rearB = GetWheelCenter(TrackHalfWidth, RearAxleZ);
        rearWheelBSpin = GetRollingAngle(rearB, Forward, rearWheelBSpin, step);
        ApplyWheelForces(rearB, Forward, rearDriveForce, rearTireMaterial);
    }

    private Vector3 GetWheelCenter(float localX, float localZ)
    {
        return body.position + body.rotation * new Vector3(localX, WheelRadius, localZ);
    }

    private float GetRollingAngle(Vector3 wheelCenter, Vector3 wheelForward, float angle, float step)
    {
        float rollSpeed = Vector3.Dot(body.GetPointVelocity(wheelCenter), wheelForward);
        return Mathf.Repeat(angle - rollSpeed * step / WheelRadius * Mathf.Rad2Deg, 360f);
    }

    private void ApplyWheelForces(Vector3 wheelCenter, Vector3 wheelForward, float driveForce, PhysicsMaterial tireMaterial)
    {
        Vector3 origin = wheelCenter + Vector3.up * SuspensionTravel;
        if (!Physics.Raycast(origin, Vector3.down, out RaycastHit hit, WheelRadius + 2f * SuspensionTravel,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) || hit.rigidbody == body)
            return;

        Vector3 contactVelocity = body.GetPointVelocity(hit.point);
        float compression = WheelRadius + SuspensionTravel - hit.distance;
        float load = Mathf.Max(0f, compression * SpringStrength - contactVelocity.y * SpringDamping);
        Vector3 forward = Vector3.ProjectOnPlane(wheelForward, hit.normal).normalized;
        Vector3 lateral = Vector3.Cross(hit.normal, forward);
        float sideSpeed = Vector3.Dot(contactVelocity, lateral);
        if (load > 0f)
        {
            HasWheelContact = true;
            TireSlipSpeed = Mathf.Max(TireSlipSpeed, Mathf.Abs(sideSpeed));
        }
        float sideForce = -sideSpeed * LateralGripResponse * body.mass * 0.25f;
        bool sliding = Mathf.Abs(sideSpeed) > 0.1f;
        float tireFriction = GetFriction(tireMaterial, sliding);
        float surfaceFriction = GetFriction(hit.collider.sharedMaterial, sliding);
        float gripLimit = load * Mathf.Sqrt(tireFriction * surfaceFriction);
        Vector3 traction = Vector3.ClampMagnitude(forward * driveForce + lateral * sideForce, gripLimit);

        // Applying the force at the contact patch lets the front tires generate yaw.
        body.AddForceAtPosition(hit.normal * load + traction, hit.point);
    }

    private static float GetFriction(PhysicsMaterial material, bool sliding)
    {
        if (material == null)
            return 0.6f;

        return sliding ? material.dynamicFriction : material.staticFriction;
    }
}
