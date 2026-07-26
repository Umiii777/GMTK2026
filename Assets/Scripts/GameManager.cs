using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isToSkipMainMenu;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(instance);
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
        Timer.instance.StartTimer(90f);
        NumberFallManager.instance.StartFalling();

        FindAnyObjectByType<PlatformSpawner>(FindObjectsInactive.Include).gameObject.SetActive(true);
    }
}
