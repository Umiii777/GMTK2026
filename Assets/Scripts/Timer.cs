using System.Linq;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer instance;
    public Countdown countdown;

    private bool isStoped = true;
    private float recordedSeconds;
    private TextMeshPro[] timeNumberTexts;

    private void Awake()
    {
        instance = this;
        timeNumberTexts = GetComponentsInChildren<TextMeshPro>().Where(x => !x.name.StartsWith("Colon")).ToArray();
    }

    private void Update()
    {
        if (isStoped)
            return;
            
        for (int i = 0; i < timeNumberTexts.Length; i++)
            timeNumberTexts[i].text = countdown.FormattedRemainingTime[i].ToString();

        if (countdown.IsFinished)
        {
            GameManager.instance.StopGamePlay();
            UIManager.instance.LoadGameClearUI();
        }
    }

    public float GetRemainingSeconds() => countdown.RemainingSeconds;

    public void StartTimer(float countdownDuration)
    {
        isStoped = false;
        countdown = new Countdown(countdownDuration);
        GetComponent<RandomMovementHorizontal>().StartMoving();
    }

    public void PauseTimer()
    {
        recordedSeconds = countdown.RemainingSeconds;
        StopTimer();
    }

    public void ResumeTimer()
    {
        StartTimer(recordedSeconds);
    }

    public void StopTimer()
    {
        isStoped = true;
        GetComponent<RandomMovementHorizontal>().StopAllCoroutines();
    }
}
