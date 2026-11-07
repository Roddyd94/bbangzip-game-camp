using System;
using UnityEngine;

public sealed class BowlingLaneEnd : MonoBehaviour
{
    public event Action<BowlingLaneEnd, BowlingBall> BallReached;

    [SerializeField]
    private BowlingLane lane;

    public int LaneNumber => lane.LaneNumber;

    private void OnTriggerEnter(Collider other)
    {
        if (other == null || other.attachedRigidbody == null)
            return;
        var ball = other.attachedRigidbody.GetComponent<BowlingBall>();
        if (ball != null)
            BallReached?.Invoke(this, ball);
    }
}
