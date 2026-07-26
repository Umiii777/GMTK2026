using System.Collections;
using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void Start()
    {
        if (GameManager.instance.isToSkipMainMenu)
        {
            Camera.main.orthographicSize = 5f;
            StartGameplay();
        }
    }

    public void OnClickRuies()
    {
        UIManager.instance.LoadRulesUI();
    }
    
    public void OnClickStart()
    {
        StartCoroutine(ZoomIn());
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    private IEnumerator ZoomIn()
    {
        float delta = 0f;
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();

        while (delta < Mathf.PI / 2)
        {
            canvasGroup.alpha -= 0.1f;
            delta += 0.02f;
            Camera.main.orthographicSize = 3 + 2 * Mathf.Sin(delta);
            yield return new WaitForSecondsRealtime(0.002f);
        }
        StartGameplay();
    }

    private void StartGameplay()
    {
        GameManager.instance.StartGamePlay();
        Destroy(gameObject);
    }
}
