using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public float countdownSeconds;
    public bool isToSkipMainMenu;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(instance);
        }
        Camera.main.GetComponent<OldTelevision>().warp = false;
        Timer.instance?.StopTimer();
        NumberFallManager.instance?.StopAllCoroutines();
    }

    public void StopGamePlay()
    {
        PauseManager.Instance?.Pause();
        Timer.instance?.StopTimer();
        NumberFallManager.instance?.StopAllCoroutines();
    }

    public void StartGamePlay()
    {
        PauseManager.Instance.Resume();
        Timer.instance.StartTimer(countdownSeconds);
        NumberFallManager.instance.StartFalling();

        FindAnyObjectByType<PlatformSpawner>(FindObjectsInactive.Include).gameObject.SetActive(true);
        StartCoroutine(EnableWarp());
        UIManager.instance.transform.GetChild(0).gameObject.SetActive(true);
    }

    private IEnumerator EnableWarp()
    {
        yield return new WaitForSecondsRealtime(3f);
        Camera.main.GetComponent<OldTelevision>().warp = true;
    }
}
