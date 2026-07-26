using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
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
