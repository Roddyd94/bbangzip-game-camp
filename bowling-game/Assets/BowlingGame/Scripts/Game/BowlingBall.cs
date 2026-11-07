using UnityEngine;
using UnityEngine.UIElements;

public class BowlingBall : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rigidbody;
    [SerializeField, Range(0f, 2f)]
    private float maximumHookAcceleration = 0.9f;

    private Vector3 launchPosition;
    private Vector3 laneForward;
    private float sideSpin;
    private bool isRolling;

    public Vector3 Position => rigidbody.position;
    public float Speed => rigidbody.linearVelocity.magnitude;

    private void Reset()
    {
        isRolling = false;

        rigidbody.isKinematic = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
        rigidbody.isKinematic = true;
        rigidbody.detectCollisions = false;
        rigidbody.rotation = Quaternion.Euler(-90f, 0f, 0f);
    }

    public void Place(Vector3 position)
    {
        Reset();
        rigidbody.position = position;
        transform.SetPositionAndRotation(position, rigidbody.rotation);
    }

    public void Launch(BowlingShot shot, Vector3 laneForwardDIrection)
    {
        laneForward = laneForwardDIrection;
        launchPosition = rigidbody.position;
        sideSpin = Mathf.Sin(shot.SpinDirection * Mathf.Deg2Rad) * shot.SpinStrength / 100f;
        rigidbody.isKinematic = false;
        rigidbody.detectCollisions = true;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.maxAngularVelocity = 140f;
        Vector3 direction = shot.GetThrowDirection(laneForward);
        rigidbody.linearVelocity = direction * shot.LaunchSpeed;
        rigidbody.angularVelocity = Vector3.Cross(Vector3.up, direction) * (shot.LaunchSpeed / 0.109f)
            + shot.GetSpinAxis(laneForward) * shot.ExtraSpinSpeed;

        isRolling = true;
    }

    private void FixedUpdate()
    {
        if (!isRolling)
            return;

        float traveled = Vector3.Dot(rigidbody.position - launchPosition, laneForward);
        if (rigidbody.position.y > 0.2f
            || rigidbody.position.y < 0.08f
            || traveled > 14f)
            return;

        // 데모용 레인 그립 근사입니다. 오일 분포를 정밀하게 계산하는 모델은 아닙니다.
        float laneGrip = Mathf.SmoothStep(0f, 1f, traveled / 12f);
        Vector3 laneRight = Vector3.Cross(Vector3.up, laneForward);
        rigidbody.AddForce(laneRight * sideSpin * laneGrip * maximumHookAcceleration, ForceMode.Acceleration);
    }
}
