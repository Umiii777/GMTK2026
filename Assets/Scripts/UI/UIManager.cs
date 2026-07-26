using System;
using System.Collections.Generic;
using UnityEngine;

public enum UIType
{
    Failure = 0,
    AbilityChoice = 1
}

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Tooltip("顺序需与 UIType 枚举一致：0=Failure，1=AbilityChoice")]
    public GameObject[] uiPrefabs;

    private void Awake()
    {
        instance = this;
    }

    public void LoadFailureUI()
    {
        InstantiatePrefab(UIType.Failure);
    }

    /// <summary>打开肉鸽能力选择界面，并注入选项数据</summary>
    public void LoadAbilityChoiceUI(List<RogueAbilityConfig> offered, Action<RogueAbilityConfig> onPicked)
    {
        GameObject go = InstantiatePrefab(UIType.AbilityChoice);
        if (go == null)
            return;

        RogueAbilityChoiceUI ui = go.GetComponent<RogueAbilityChoiceUI>();
        if (ui == null)
            ui = go.GetComponentInChildren<RogueAbilityChoiceUI>(true);

        if (ui != null)
            ui.Setup(offered, onPicked);
        else
            Debug.LogError("[UIManager] AbilityChoice 预制体上缺少 RogueAbilityChoiceUI。", go);
    }

    private GameObject InstantiatePrefab(UIType type)
    {
        int index = (int)type;
        if (uiPrefabs == null || index < 0 || index >= uiPrefabs.Length || uiPrefabs[index] == null)
        {
            Debug.LogError($"[UIManager] uiPrefabs[{index}] ({type}) 未配置。", this);
            return null;
        }

        return Instantiate(uiPrefabs[index], transform);
    }
}
