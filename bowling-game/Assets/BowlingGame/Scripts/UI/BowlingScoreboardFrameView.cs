using TMPro;
using UnityEngine;

public class BowlingScoreboardFrameView : MonoBehaviour
{
    public BowlingScoreboardRollView[] RollViews => rollViews;
    public int FrameNumber
    {
        get
        {
            return int.Parse(frameNumber.text);
        }
        set
        {
            frameNumber.text = value.ToString(uiText.pinNumberFormat);
        }
    }
    public string Score
    {
        get
        {
            return score.text;
        }
        set
        {
            score.text = value;
        }
    }
    public bool IsActiveFrame
    {
        get
        {
            return activeFrameBorder.enabled;
        }
        set
        {
            activeFrameBorder.enabled = value;
        }
    }

    [SerializeField]
    private BowlingScoreboardRollView[] rollViews = new BowlingScoreboardRollView[3];
    [SerializeField]
    private TMP_Text frameNumber;
    [SerializeField]
    private BowlingUiText uiText;
    [SerializeField]
    private TMP_Text score;
    [SerializeField]
    private UnityEngine.UI.Image activeFrameBorder;

    public void ResetFrameView()
    {
        Score = uiText.unconfirmedScore;
        IsActiveFrame = false;

        ResetRollViews();
    }

    public void ResetRollViews()
    {
        if (rollViews == null)
            return;

        foreach (var roll in rollViews)
        {
            if (roll != null)
                roll.ResetRollView();
        }
    }
}
