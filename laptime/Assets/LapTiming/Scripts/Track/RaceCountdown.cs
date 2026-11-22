using System;
using System.Collections;
using TMPro;
using UnityEngine;

public sealed class RaceCountdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private RaceAudio raceAudio;
    [SerializeField, Min(1)] private int countdownSeconds = 3;

    internal IEnumerator Play(Action onGo)
    {
        countdownText.enabled = true;
        var second = new WaitForSeconds(1f);
        for (int remaining = countdownSeconds; remaining > 0; remaining--)
        {
            countdownText.text = remaining.ToString();
            raceAudio?.PlayCountdownTick();
            yield return second;
        }

        countdownText.color = new Color(0.42f, 1f, 0.55f);
        countdownText.text = "GO!";
        raceAudio?.PlayStartSignal();
        onGo?.Invoke();
        yield return second;
        countdownText.enabled = false;
    }
}
