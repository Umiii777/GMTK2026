using System.Linq;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour
{
    public static Timer instance;
    public Countdown countdown;

    private bool isStoped;
    private TextMeshPro[] timeNumberTexts;

    private void Awake()
    {
        instance = this;
        timeNumberTexts = GetComponentsInChildren<TextMeshPro>().Where(x => !x.name.StartsWith("Colon")).ToArray();
        StartCountdown(90); //临时代码
    }

    private void Update()
    {
        if (!isStoped)
            for (int i = 0; i < timeNumberTexts.Length; i++)
                timeNumberTexts[i].text = countdown.FormattedRemainingTime[i].ToString();

        if (countdown.IsFinished)
            GameManager.instance.StopGamePlay();
    }

    public void StartCountdown(int seconds) => countdown = new Countdown(seconds);

    public float GetRemainingSeconds() => countdown.RemainingSeconds;
    
    public void StopTimer()
    {
        isStoped = true;
        GetComponent<RandomMovementHorizontal>().StopAllCoroutines();
    }
}
