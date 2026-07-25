using UnityEngine;

public class Timer : MonoBehaviour
{
    public Countdown countdown;

    void Start()
    {
        countdown = new Countdown(90);
    }
}
