using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void StopGamePlay()
    {
        Camera.main.GetComponent<CameraRise>().Speed = 0f;
        NumberFallManager.instance.StopAllCoroutines();
        Timer.instance.StopTimer();
    }
}
