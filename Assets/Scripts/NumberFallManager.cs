using System.Collections;
using UnityEngine;

public class NumberFallManager : MonoBehaviour
{
    public static NumberFallManager instance;

    public bool isToFallMinuteNumbers;
    public bool isToFallSecondNumbers;
    public bool isToFallMillisecondNumbers;
    public int chanceForNoMillisecondNumbers = 10;
    public float intervalMillisecondNumbersFalling = 0.3f;
    public float stoppingForce = 10f;

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

    public void SetStoppingForce(float force) => stoppingForce = force;

    public void SetChanceForNoMillisecondNumbers(int chance) => chanceForNoMillisecondNumbers = chance;

    public void SetIsToFallMillisecondNumbers(bool value) => isToFallMillisecondNumbers = value;

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
        GameObject numberFall = Instantiate(numberPrefabs[number], timeNumberPositions[0].position, Quaternion.identity, transform);

        numberFall.GetComponent<Rigidbody2D>().drag = stoppingForce;
    }

    private void FallSecondNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int number = int.Parse(Timer.instance.countdown.FormattedRemainingTime[2 - i].ToString());
            GameObject numberFall = Instantiate(numberPrefabs[number], timeNumberPositions[2 - i].position, Quaternion.identity, transform);
            
            numberFall.GetComponent<Rigidbody2D>().drag = stoppingForce;
        }
    }

    private void FallMillisecondNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            int number = int.Parse(Timer.instance.countdown.FormattedRemainingTime[4 - i].ToString());
            GameObject numberfall = Instantiate(numberPrefabs[number], timeNumberPositions[4 - i].position, Quaternion.identity, transform);
            numberfall.transform.localScale = new Vector3(0.45f, 0.45f, 1f);
            numberfall.GetComponent<Rigidbody2D>().gravityScale = 2f;
            numberfall.GetComponent<Rigidbody2D>().drag = stoppingForce;
        }
    }
}