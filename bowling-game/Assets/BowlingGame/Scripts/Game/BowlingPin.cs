using UnityEngine;

public class BowlingPin : MonoBehaviour
{
    public bool IsStanding =>
        gameObject.activeInHierarchy 
        && transform.position.y >= -0.05f
        && Vector3.Angle(transform.forward, Vector3.up) <= standingAngleLimit;

    [SerializeField]
    private BowlingLane lane;

    public int LaneNumber => lane.LaneNumber;
    [field: SerializeField]
    public int PinNumber { get; private set; }

    [SerializeField, Range(0f, 60f)]
    private float standingAngleLimit = 25f;
}
