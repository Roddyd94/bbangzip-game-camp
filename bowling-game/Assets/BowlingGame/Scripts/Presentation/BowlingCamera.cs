using UnityEngine;

public sealed class BowlingCamera : MonoBehaviour
{
    [SerializeField]
    private Camera viewCamera;
    [SerializeField, Range(1f, 20f)]
    private float followSharpness = 6f;

    private Vector3 lanePinCenter;
    private Vector3 setupBallPosition;
    private Transform rollingBall;
    private bool watchingPins;
    private int viewNumber;
    public int ViewIndex => viewNumber;

    public void ShowSetup(Vector3 ballPosition, Vector3 pinCenter)
    {
        setupBallPosition = ballPosition;
        lanePinCenter = pinCenter;
        rollingBall = null;
        watchingPins = false;
        ApplyView(true);
    }

    public void TrackBall(Transform ball)
    {
        rollingBall = ball;
        watchingPins = false;
    }

    public void ShowPins()
    {
        rollingBall = null;
        watchingPins = true;
    }

    public void CycleView()
    {
        viewNumber = (viewNumber + 1) % 3;
        ApplyView(true);
    }

    private void LateUpdate()
    {
        ApplyView(false);
    }

    private void ApplyView(bool instant)
    {
        Vector3 focus = rollingBall != null ? rollingBall.position : setupBallPosition;
        if (watchingPins)
            focus = lanePinCenter;

        float cameraZ = Mathf.Max(focus.z + 3.0f, lanePinCenter.z + 3.4f);
        Vector3 cameraPosition = new Vector3(lanePinCenter.x, 1.05f, cameraZ);
        Vector3 lookPosition = lanePinCenter;

        switch (viewNumber)
        {
        case 0:
        {
            lookPosition = Vector3.Lerp(focus, lanePinCenter, 0.1f);
        }
        break;
        case 1:
        {
            cameraPosition.x += 1.25f;
            cameraPosition.y = 1.25f;
        }
        break;
        case 2:
        {
            cameraPosition = new Vector3(lanePinCenter.x + 1.4f, 2.0f, setupBallPosition.z + 2.5f);
            lookPosition = new Vector3(lanePinCenter.x, 0.3f, lanePinCenter.z + 5f);
        }
        break;
        }

        float blend = instant ? 1f : 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
        viewCamera.transform.position = Vector3.Lerp(viewCamera.transform.position, cameraPosition, blend);
        Quaternion lookRotation = Quaternion.LookRotation(lookPosition - viewCamera.transform.position);
        viewCamera.transform.rotation = Quaternion.Slerp(viewCamera.transform.rotation, lookRotation, blend);
    }
}
