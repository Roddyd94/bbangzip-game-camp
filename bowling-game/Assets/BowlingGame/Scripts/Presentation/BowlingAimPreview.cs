using UnityEngine;

public sealed class BowlingAimPreview : MonoBehaviour
{
    private const float axisLength = 0.4f;

    [SerializeField]
    private LineRenderer directionLine;
    [SerializeField]
    private LineRenderer directionArrowhead;
    [SerializeField]
    private LineRenderer spinAxisLine;
    [SerializeField]
    private LineRenderer spinArcLine;
    [SerializeField]
    private LineRenderer spinArrowhead;
    [SerializeField, Range(8, 64)]
    private int spinArcSegments = 40;

    public void Show(Vector3 ballPosition, Vector3 laneForward, BowlingShot shot, bool visible)
    {
        directionLine.enabled = visible;
        directionArrowhead.enabled = visible;
        spinAxisLine.enabled = visible;
        bool showSpin = visible && shot.SpinStrength > 0f;
        spinArcLine.enabled = showSpin;
        spinArrowhead.enabled = showSpin;

        if (!visible)
            return;

        transform.position = ballPosition;
        Vector3 direction = transform.InverseTransformDirection(shot.GetThrowDirection(laneForward));
        Vector3 start = Vector3.up * 0.02f;
        // 최소 세기에도 출발 속도가 있으므로 화살표에도 최소 길이를 둡니다.
        float throwLength = Mathf.Lerp(2f, 6f, shot.ThrowPower / 100f);
        Vector3 tip = start + direction * throwLength;
        SetLine(directionLine, start, tip);
        SetArrowhead(directionArrowhead, tip, direction, Vector3.Cross(Vector3.up, direction), 0.28f, 0.14f);

        Vector3 axis = transform.InverseTransformDirection(shot.GetSpinAxis(laneForward)).normalized;
        Vector3 spinCenter = Vector3.zero;
        SetLine(spinAxisLine, spinCenter - axis * axisLength / 2.0f, spinCenter + axis * axisLength / 2.0f);
        
        if (!showSpin)
            return;

        // 각속도와 같은 오른손 법칙으로 축 주위를 돌며, 추가 회전이 강할수록 길게 감깁니다.
        float arcDegrees = Mathf.Lerp(40f, 300f, shot.SpinStrength / 100f);
        spinArcLine.positionCount = spinArcSegments + 1;
        Vector3 radial = direction * 0.15f;

        for (int i = 0; i <= spinArcSegments; ++i)
        {
            // 꼬리는 공 뒤쪽(투구 방향의 반대인 플레이어 쪽)에서 시작합니다.
            float angle = 180f + arcDegrees * i / spinArcSegments;
            radial = Quaternion.AngleAxis(angle, axis) * direction * 0.15f;
            spinArcLine.SetPosition(i, spinCenter + radial);
        }

        Vector3 tangent = Vector3.Cross(axis, radial).normalized;
        // 화살촉의 양 날개를 회전축 방향으로 펼쳐 가로 폭을 확보합니다.
        SetArrowhead(spinArrowhead, spinCenter + radial, tangent, axis, 0.08f, 0.06f);
    }

    public void ResetPreview()
    {
        ClearLine(directionLine);
        ClearLine(directionArrowhead);
        ClearLine(spinAxisLine);
        ClearLine(spinArcLine);
        ClearLine(spinArrowhead);
    }

    private static void ClearLine(LineRenderer line)
    {
        line.enabled = false;
        line.positionCount = 0;
    }

    private static void SetLine(LineRenderer line, Vector3 start, Vector3 end)
    {
        line.positionCount = 2;
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private static void SetArrowhead(LineRenderer line, Vector3 tip, Vector3 forward, Vector3 side, float length, float halfWidth)
    {
        line.positionCount = 3;
        line.SetPosition(0, tip - forward * length + side * halfWidth);
        line.SetPosition(1, tip);
        line.SetPosition(2, tip - forward * length - side * halfWidth);
    }
}
