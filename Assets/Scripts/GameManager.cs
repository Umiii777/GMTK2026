using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public bool isToSkipMainMenu;

    [SerializeField]
    private GameObject platformSpawner;
    [SerializeField]
    private GameObject mainMenuUI;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        DontDestroyOnLoad(instance);
        OnLoadScene();
        SceneManager.sceneLoaded += OnLoadScene;
    }

    private void OnLoadScene(Scene scene = default, LoadSceneMode mode = default)
    {
        if (isToSkipMainMenu)
        {
            mainMenuUI.SetActive(false);
            StartGamePlay();
        }
        else
        {
            Timer.instance?.StopTimer();
            NumberFallManager.instance?.StopAllCoroutines();
        }
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

        platformSpawner.SetActive(true);
    }
}
