using TMPro;
using UnityEngine;

public class BowlingScoreboardRollView : MonoBehaviour
{
    public string RollValue
    {
        get
        {
            return rollValue.text;
        }
        set
        {
            rollValue.text = value;
        }
    }
    public bool IsMiss
    {
        get
        {
            return miss.enabled;
        }
        set
        {
            miss.enabled = value;
        }
    }
    public bool IsSpare
    {
        get
        {
            return spare.enabled;
        }
        set
        {
            spare.enabled = value;
        }
    }
    public bool IsStrike
    {
        get
        {
            return strike.enabled;
        }
        set
        {
            strike.enabled = value;
        }
    }
    public bool IsSplit
    {
        get
        {
            return split.enabled;
        }
        set
        {
            split.enabled = value;
        }
    }

    [SerializeField]
    private TMP_Text rollValue;
    [SerializeField]
    private UnityEngine.UI.Image miss;
    [SerializeField]
    private UnityEngine.UI.Image spare;
    [SerializeField]
    private UnityEngine.UI.Image strike;
    [SerializeField]
    private UnityEngine.UI.Image split;

    public void ResetRollView()
    {
        RollValue = string.Empty;
        IsMiss = false;
        IsSpare = false;
        IsStrike = false;
        IsSplit = false;
    }
}
