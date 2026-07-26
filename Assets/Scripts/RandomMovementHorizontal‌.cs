using System;
using System.Collections;
using UnityEngine;

public class RandomMovementHorizontal : MonoBehaviour
{
    public int directionStablity = 2;
    public float redirectingIntervalSec = 2f;
    public float redirectingIntervalSecShort = 0.1f;
    public float positionLimitX = 6f;
    public float speedMin = 4f;
    public float speedMax = 8f;

    private Coroutine movement;

    private void Start()
    {
        StartMoving();
    }

    public void StartMoving()
    {
        StartCoroutine(KeepMoving());
    }

    private IEnumerator KeepMoving()
    {
        while (true)
        {
            int dir = DateTime.Now.Second % 2 * 2 - 1; // dir = 1 or -1
            float speed = UnityEngine.Random.Range(speedMin, speedMax);

            movement = StartCoroutine(Move(dir, speed));
            yield return new WaitForSeconds(
                UnityEngine.Random.Range(0, directionStablity) == 0
                ? redirectingIntervalSecShort
                : redirectingIntervalSec
            );
            StopCoroutine(movement);
        }
    }

    private IEnumerator Move(int dir, float speed)
    {
        while (true)
        {
            if (transform.position.x > positionLimitX)
                dir = -1;
            else if (transform.position.x < -positionLimitX)
                dir = 1;
            transform.Translate(dir * speed * Vector3.right * Time.deltaTime);
            yield return null;
        }
    }
}
