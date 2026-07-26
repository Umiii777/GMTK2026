using UnityEngine;
using UnityEngine.UI;

public enum UIType { Failure, Rules, GameClear }

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject[] uiPrefabs;

    private void Awake()
    {
        instance = this;
    }

    public void LoadFailureUI()
    {
        Instantiate(uiPrefabs[(int)UIType.Failure], transform);
    }

    public void LoadRulesUI()
    {
        GameObject uiObj = Instantiate(uiPrefabs[(int)UIType.Rules], transform);
        uiObj.GetComponentInChildren<Button>().onClick.AddListener(() => Destroy(uiObj));
    }

    public void LoadGameClearUI()
    {
        Instantiate(uiPrefabs[(int)UIType.GameClear], transform);
    }
}
