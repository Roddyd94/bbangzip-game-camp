using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class BowlingHud : MonoBehaviour
{
    public event Action<BowlingRequest, float?> Requested;

    [SerializeField] private BowlingUiText uiText;
    [SerializeField] private TMP_Text stageTitle;
    [SerializeField] private TMP_Text stageDescription;
    [SerializeField] private TMP_Text stageValue;
    [SerializeField] private TMP_Text gameStatus;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private TMP_Text cameraLabel;
    [SerializeField] private TMP_Text confirmLabel;
    [SerializeField] private Image[] stepBackgrounds;
    [SerializeField] private Slider valueSlider;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button cameraButton;

    private bool isUpdatingSliderRange;

    public BowlingUiText Text => uiText;

    private void OnEnable()
    {
        confirmButton.onClick.AddListener(OnConfirm);
        backButton.onClick.AddListener(OnBack);
        restartButton.onClick.AddListener(OnRestart);
        cameraButton.onClick.AddListener(OnCamera);
        valueSlider.onValueChanged.AddListener(OnValueChanged);
    }

    private void OnDisable()
    {
        confirmButton.onClick.RemoveListener(OnConfirm);
        backButton.onClick.RemoveListener(OnBack);
        restartButton.onClick.RemoveListener(OnRestart);
        cameraButton.onClick.RemoveListener(OnCamera);
        valueSlider.onValueChanged.RemoveListener(OnValueChanged);
    }

    public void ShowStage(BowlingShotStage stage, BowlingShot shot, float speed, int score,
        float minimum, float maximum, float sliderValue)
    {
        BowlingUiText.StageText copy = uiText.GetStage(stage);
        stageTitle.text = copy.title;
        stageDescription.text = copy.description;
        string format = copy.valueFormat;
        stageValue.text = stage switch
        {
            BowlingShotStage.StartPosition => string.Format(format, shot.StartOffset),
            BowlingShotStage.ThrowAngle => string.Format(format, shot.ThrowAngle),
            BowlingShotStage.ThrowPower => string.Format(format, shot.ThrowPower, shot.LaunchSpeed),
            BowlingShotStage.SpinDirection => string.Format(format, shot.SpinDirection),
            BowlingShotStage.SpinStrength => string.Format(format, shot.SpinStrength, shot.ExtraSpinSpeed),
            BowlingShotStage.Ready => string.Format(format, shot.ThrowAngle, shot.ThrowPower, shot.SpinDirection, shot.SpinStrength),
            BowlingShotStage.Rolling => string.Format(format, speed),
            BowlingShotStage.Finished => string.Format(format, score),
            _ => format
        };

        bool adjusting = stage <= BowlingShotStage.SpinStrength;
        valueSlider.gameObject.SetActive(adjusting);
        if (adjusting)
        {
            isUpdatingSliderRange = true;
            valueSlider.minValue = minimum;
            valueSlider.maxValue = maximum;
            valueSlider.SetValueWithoutNotify(sliderValue);
            isUpdatingSliderRange = false;
        }

        confirmButton.interactable = adjusting || stage == BowlingShotStage.Ready || stage == BowlingShotStage.Finished;
        backButton.interactable = stage > BowlingShotStage.StartPosition && stage <= BowlingShotStage.Ready;
        confirmLabel.text = stage == BowlingShotStage.Ready ? uiText.throwButton : stage == BowlingShotStage.Finished ? uiText.restartButton : uiText.nextButton;

        for (int i = 0; i < stepBackgrounds.Length; ++i)
            stepBackgrounds[i].color = i == (int)stage ? new Color(0.03f, 0.48f, 0.43f, 1f) : new Color(0.10f, 0.15f, 0.20f, 1f);
    }

    public void ShowGame(int laneNumber, int frameNumber, int rollNumber, int totalScore, int cameraIndex)
    {
        gameStatus.text = string.Format(uiText.gameStatusFormat, laneNumber, frameNumber, rollNumber, totalScore);
        cameraLabel.text = string.Format(uiText.cameraLabelFormat, uiText.cameraNames[cameraIndex]);
    }

    public void ShowResult(string message) { resultText.text = message; }
    private void OnConfirm() { Requested?.Invoke(BowlingRequest.Confirm, null); }
    private void OnBack() { Requested?.Invoke(BowlingRequest.Back, null); }
    private void OnRestart() { Requested?.Invoke(BowlingRequest.Restart, null); }
    private void OnCamera() { Requested?.Invoke(BowlingRequest.ChangeCamera, null); }
    private void OnValueChanged(float value)
    {
        if (!isUpdatingSliderRange)
            Requested?.Invoke(BowlingRequest.SetStageValue, value);
    }
}
