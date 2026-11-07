using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Bowling/UI Text")]
public sealed class BowlingUiText : ScriptableObject
{
    [Serializable]
    public struct StageText
    {
        public BowlingShotStage stage;
        public string title;
        [TextArea] public string description;
        public string valueFormat;
    }

    [SerializeField] private StageText[] stages;

    public string playerName;
    public string setupHint;
    public string rollingHint;
    public string stoppedHint;
    public string rejectedRollHint;
    public string gameEndHint;
    public string strike;
    public string spare;
    public string split;
    public string rollResult;
    public string rollResultFormat;
    public string nextButton;
    public string throwButton;
    public string restartButton;
    public string gameStatusFormat;
    public string cameraLabelFormat;
    public string[] cameraNames;

    public string laneLabelFormat;
    public string frameLabelFormat;
    public string rollLabelFormat;
    public string waitingForRoll;
    public string rollComplete;
    public string gameOver;
    public string complete;
    public string pinNumberFormat;
    public string scoreNumberFormat;
    public string unconfirmedScore;

    public string physicsTitle;
    public string physicsControls;
    public string physicsStatusFormat;
    public string physicsAutoReplay;
    public string physicsLaunch;
    public string physicsReset;

    public StageText GetStage(BowlingShotStage stage)
    {
        foreach (StageText entry in stages)
        {
            if (entry.stage == stage)
                return entry;
        }

        return default;
    }
}
