using UnityEngine;

// 스킨 메시 대신, 움직이는 본을 따라가는 단순 충돌체를 사용합니다.
public sealed class BowlingSweeperCollider : MonoBehaviour
{
    [SerializeField] 
    private Transform sweepTravelBone;
    [SerializeField] 
    private Transform pinsetterRoot;
    [SerializeField] 
    private Rigidbody sweepBody;
    [SerializeField] 
    private Vector3 barCenterOffset = new Vector3(0f, -0.21975f, 0f);

    private void FixedUpdate()
    {
        sweepBody.MovePosition(sweepTravelBone.position + pinsetterRoot.TransformDirection(barCenterOffset));
        sweepBody.MoveRotation(pinsetterRoot.rotation);
    }
}
