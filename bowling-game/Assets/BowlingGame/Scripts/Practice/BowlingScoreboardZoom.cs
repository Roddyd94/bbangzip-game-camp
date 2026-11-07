using UnityEngine;
using UnityEngine.EventSystems;

// 점수판의 오른쪽 위를 고정한 채 마우스 호버로 확대합니다.
public sealed class BowlingScoreboardZoom : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private RectTransform scoreboard;
    [SerializeField, Range(1f, 3f)] private float zoomMultiplier = 2.5f;
    private Vector3 normalScale;

    private void Awake()
    {
        normalScale = scoreboard.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        scoreboard.localScale = normalScale * zoomMultiplier;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        scoreboard.localScale = normalScale;
    }

    private void OnDisable()
    {
        scoreboard.localScale = normalScale;
    }
}
