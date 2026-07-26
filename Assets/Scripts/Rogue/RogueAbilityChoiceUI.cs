using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 肉鸽三选一界面。挂在能力选择 UI 预制体根节点上。
/// 在 Inspector 里配好 3 个选项槽（按钮 + 标题 + 描述）。
/// </summary>
public class RogueAbilityChoiceUI : MonoBehaviour
{
    [Serializable]
    public class OptionSlot
    {
        public Button Button;
        public TMP_Text Title;
        public TMP_Text Description;
        public Image Icon;
    }

    [SerializeField] private OptionSlot[] _slots = new OptionSlot[3];

    private Action<RogueAbilityConfig> _onPicked;
    private List<RogueAbilityConfig> _offered;
    private bool _picked;

    public void Setup(List<RogueAbilityConfig> offered, Action<RogueAbilityConfig> onPicked)
    {
        _offered = offered;
        _onPicked = onPicked;
        _picked = false;

        for (int i = 0; i < _slots.Length; i++)
        {
            OptionSlot slot = _slots[i];
            if (slot == null || slot.Button == null)
                continue;

            slot.Button.onClick.RemoveAllListeners();

            if (offered == null || i >= offered.Count || offered[i] == null)
            {
                slot.Button.gameObject.SetActive(false);
                continue;
            }

            RogueAbilityConfig config = offered[i];
            slot.Button.gameObject.SetActive(true);

            if (slot.Title != null)
                slot.Title.text = config.DisplayName;
            if (slot.Description != null)
                slot.Description.text = config.Description;
            if (slot.Icon != null)
            {
                slot.Icon.sprite = config.Icon;
                slot.Icon.enabled = config.Icon != null;
            }

            int captured = i;
            slot.Button.onClick.AddListener(() => Pick(captured));
        }
    }

    private void Pick(int index)
    {
        if (_picked || _offered == null || index < 0 || index >= _offered.Count)
            return;

        _picked = true;
        RogueAbilityConfig config = _offered[index];
        _onPicked?.Invoke(config);
        Destroy(gameObject);
    }
}
