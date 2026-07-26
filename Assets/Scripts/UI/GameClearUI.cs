using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClearUI : MonoBehaviour
{
    public void OnClickMainMenu()
    {
        GameManager.instance.isToSkipMainMenu = false;
        SceneManager.LoadScene(0);
    }
}
