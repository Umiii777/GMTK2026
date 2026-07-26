using System.Collections;
using UnityEngine;

public class NumberFallManager : MonoBehaviour
{
    public static NumberFallManager instance;

    public bool isToFallMinuteNumbers = true;
    public bool isToFallSecondNumbers = true;
    public bool isToFallMillisecondNumbers = true;
    public int chanceForNoMillisecondNumbers = 5;
    public float intervalMillisecondNumbersFalling = 0.3f;

    [SerializeField]
    private GameObject[] numberPrefabs;
    [SerializeField]
    private Transform[] timeNumberPositions;

    private void Awake()
    {
        instance = this;
    }

    public void StartFalling()
    {
        StartCoroutine(KeepFallingMinuteNumbers());
        StartCoroutine(KeepFallingSecondNumbers());
        StartCoroutine(KeepFallingMillisecondNumbers());
    }

    private IEnumerator KeepFallingMinuteNumbers()
    {
        while (true)
        {
            if (isToFallMinuteNumbers)
                FallMinuteNumbers();
            
            float remainingSeconds = Timer.instance.GetRemainingSeconds();
            yield return new WaitForSeconds((int)remainingSeconds % 60 + remainingSeconds - (int)remainingSeconds);
        }
    }

    private IEnumerator KeepFallingSecondNumbers()
    {
        while (true)
        {
            if (isToFallSecondNumbers)
            {
                if ((int)Timer.instance.GetRemainingSeconds() % 10 == 0)
                    FallSecondNumbers(2);
                else
                    FallSecondNumbers(1);
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator KeepFallingMillisecondNumbers()
    {
        while (true)
        {
            if (isToFallMillisecondNumbers && Random.Range(0, chanceForNoMillisecondNumbers) == 0)
                FallMillisecondNumbers(Random.Range(1, timeNumberPositions.Length - 2));
            yield return new WaitForSeconds(intervalMillisecondNumbersFalling);
        }
    }

    private void FallMinuteNumbers()
    {
        int number = int.Parse(Timer.instance.countdown.FormattedRemainingTime[0].ToString());
        Instantiate(numberPrefabs[number], timeNumberPositions[0].position, Quaternion.identity, transform);
    }

    private void FallSecondNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int number = int.Parse(Timer.instance.countdown.FormattedRemainingTime[2 - i].ToString());
            Instantiate(numberPrefabs[number], timeNumberPositions[2 - i].position, Quaternion.identity, transform);
        }
    }

    private void FallMillisecondNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int number = int.Parse(Timer.instance.countdown.FormattedRemainingTime[4 - i].ToString());
            GameObject numberfall = Instantiate(numberPrefabs[number], timeNumberPositions[4 - i].position, Quaternion.identity, transform);
            numberfall.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
            numberfall.GetComponent<Rigidbody2D>().gravityScale = 2f;
        }
    }
}