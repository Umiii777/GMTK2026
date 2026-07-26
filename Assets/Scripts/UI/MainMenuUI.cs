using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void Start()
    {
        if (GameManager.instance.isToSkipMainMenu)
            OnClickStart();
    }
    public void OnClickStart()
    {
        GameManager.instance.StartGamePlay();
        Destroy(gameObject);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
