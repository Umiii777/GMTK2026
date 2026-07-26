using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UIType
{
    Failure = 0,
    Rules = 1,
    GameClear = 2,
    AbilityChoice = 3
}

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Tooltip("顺序需与 UIType 一致：0=Failure，1=Rules，2=GameClear，3=AbilityChoice")]
    public GameObject[] uiPrefabs;

    private void Awake()
    {
        instance = this;
    }

    public void LoadFailureUI()
    {
        InstantiatePrefab(UIType.Failure);
    }

    public void LoadRulesUI()
    {
        GameObject uiObj = InstantiatePrefab(UIType.Rules);
        if (uiObj == null)
            return;

        Button button = uiObj.GetComponentInChildren<Button>();
        if (button != null)
            button.onClick.AddListener(() => Destroy(uiObj));
    }

    public void LoadGameClearUI()
    {
        InstantiatePrefab(UIType.GameClear);
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
