using UnityEngine;

public enum UIType { Failure }

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
}
