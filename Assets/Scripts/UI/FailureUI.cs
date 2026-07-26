using UnityEngine;
using UnityEngine.SceneManagement;

public class FailureUI : MonoBehaviour
{
    private OldTelevision oldTelevision;

    private void Start()
    {
        oldTelevision = Camera.main.GetComponent<OldTelevision>();

        oldTelevision.warpVertical = true;
        OldTelevision.stabilityOne = 0.1f;
        OldTelevision.stabilityTwo = 1f;
    }

    public void OnClickRestart()
    {
        ResetOldTelevision();
        GameManager.instance.isToSkipMainMenu = true;
        SceneManager.LoadScene(0);
    }

    public void OnClickMainMenu()
    {
        ResetOldTelevision();
        SceneManager.LoadScene(0);
    }

    private void ResetOldTelevision()
    {
        oldTelevision.warpVertical = false;
        OldTelevision.stabilityOne = 6f;
        OldTelevision.stabilityTwo = 50f;
    }
}
