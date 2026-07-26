using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>肉鸽可选能力类型（对应现有脚本）</summary>
public enum RogueAbilityId
{
    MoveSpeed,   // Movement_Move
    ExtraJump,   // Movement_Jump
    DragNumbers, // AbilityDrag
    Shield,      // ShieldManager 伞
    Dash,        // Movement_Dash
    Shrink       // Movement_Scale
}

[Serializable]
public class RogueAbilityConfig
{
    public RogueAbilityId Id;
    public string DisplayName;
    [TextArea] public string Description;
    public Sprite Icon;

    [Tooltip("选过后是否从奖池移除（解锁类建议勾选）")]
    public bool RemoveAfterPick = true;

    [Tooltip("可重复选择的最大次数；RemoveAfterPick 时通常为 1")]
    public int MaxStacks = 1;
}

/// <summary>
/// 肉鸽能力管理：打开三选一界面、应用能力、与高度里程碑对接。
/// DifficultyController 的 OnReached 可直接绑 ShowAbilityChoice()。
/// </summary>
public class RogueAbilityManager : MonoBehaviour
{
    public static RogueAbilityManager Instance { get; private set; }

    [Header("引用")]
    [SerializeField] private Transform _player;
    [SerializeField] private AbilityDrag _abilityDrag;

    [Header("奖池")]
    [SerializeField] private List<RogueAbilityConfig> _abilities = new List<RogueAbilityConfig>
    {
        new RogueAbilityConfig
        {
            Id = RogueAbilityId.MoveSpeed,
            DisplayName = "疾跑",
            Description = "移动速度提升",
            RemoveAfterPick = false,
            MaxStacks = 5
        },
        new RogueAbilityConfig
        {
            Id = RogueAbilityId.ExtraJump,
            DisplayName = "二段跳",
            Description = "额外跳跃次数 +1",
            RemoveAfterPick = false,
            MaxStacks = 3
        },
        new RogueAbilityConfig
        {
            Id = RogueAbilityId.DragNumbers,
            DisplayName = "数字牵引",
            Description = "可用鼠标拖动掉落的数字",
            RemoveAfterPick = true,
            MaxStacks = 1
        },
        new RogueAbilityConfig
        {
            Id = RogueAbilityId.Shield,
            DisplayName = "防护伞",
            Description = "获得一层旋转护盾",
            RemoveAfterPick = false,
            MaxStacks = 3
        },
        new RogueAbilityConfig
        {
            Id = RogueAbilityId.Dash,
            DisplayName = "冲刺",
            Description = "解锁冲刺（Left Shift）",
            RemoveAfterPick = true,
            MaxStacks = 1
        },
        new RogueAbilityConfig
        {
            Id = RogueAbilityId.Shrink,
            DisplayName = "缩小",
            Description = "解锁缩小（Tab 切换体型）",
            RemoveAfterPick = true,
            MaxStacks = 1
        }
    };

    [Header("升级数值")]
    [SerializeField] private float _moveSpeedBonus = 1.5f;
    [SerializeField] private int _choicesPerOffer = 3;

    private readonly Dictionary<RogueAbilityId, int> _stacks = new Dictionary<RogueAbilityId, int>();
    private Movement_Move _move;
    private Movement_Jump _jump;
    private Movement_Dash _dash;
    private Movement_Scale _scale;
    private bool _choiceOpen;

    private void Awake()
    {
        Instance = this;

        if (_player == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _player = player.transform;
        }

        CachePlayerComponents();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 暂停游戏并打开能力选择界面。
    /// 可挂到 DifficultyController 里程碑的 OnReached。
    /// </summary>
    public void ShowAbilityChoice()
    {
        if (_choiceOpen)
            return;

        List<RogueAbilityConfig> pool = BuildAvailablePool();
        if (pool.Count == 0)
        {
            Debug.Log("[RogueAbilityManager] 奖池已空，跳过选择。", this);
            return;
        }

        int count = Mathf.Min(_choicesPerOffer, pool.Count);
        List<RogueAbilityConfig> offered = PickRandomUnique(pool, count);

        _choiceOpen = true;
        PauseManager.Instance?.Pause();
        Timer.instance?.PauseTimer();

        if (UIManager.instance != null)
            UIManager.instance.LoadAbilityChoiceUI(offered, OnAbilityPicked);
        else
            Debug.LogError("[RogueAbilityManager] 场景中没有 UIManager。", this);
    }

    public void ApplyAbility(RogueAbilityId id)
    {
        CachePlayerComponents();

        switch (id)
        {
            case RogueAbilityId.MoveSpeed:
                if (_move != null)
                    _move.AddMaxSpeed(_moveSpeedBonus);
                break;

            case RogueAbilityId.ExtraJump:
                if (_jump != null)
                    _jump.AddAirJump(1);
                break;

            case RogueAbilityId.DragNumbers:
                if (_abilityDrag != null)
                    _abilityDrag.Unlock();
                else
                    FindAnyObjectByType<AbilityDrag>()?.Unlock();
                break;

            case RogueAbilityId.Shield:
                if (ShieldManager.instance != null)
                    ShieldManager.instance.AddNextShieldUpgrade();
                else
                    Debug.LogWarning("[RogueAbilityManager] 未找到 ShieldManager。", this);
                break;

            case RogueAbilityId.Dash:
                if (_dash != null)
                    _dash.Unlock();
                break;

            case RogueAbilityId.Shrink:
                if (_scale != null)
                    _scale.Unlock();
                break;
        }

        if (!_stacks.ContainsKey(id))
            _stacks[id] = 0;
        _stacks[id]++;
    }

    public int GetStack(RogueAbilityId id) =>
        _stacks.TryGetValue(id, out int n) ? n : 0;

    private void OnAbilityPicked(RogueAbilityConfig config)
    {
        if (config != null)
            ApplyAbility(config.Id);

        _choiceOpen = false;
        PauseManager.Instance?.Resume();
        Timer.instance?.ResumeTimer();
    }

    private List<RogueAbilityConfig> BuildAvailablePool()
    {
        var pool = new List<RogueAbilityConfig>();
        for (int i = 0; i < _abilities.Count; i++)
        {
            RogueAbilityConfig a = _abilities[i];
            if (a == null)
                continue;

            int stacks = GetStack(a.Id);
            if (stacks >= a.MaxStacks)
                continue;

            if (a.RemoveAfterPick && stacks > 0)
                continue;

            // 已解锁类：若组件已解锁则不再进池
            if (a.Id == RogueAbilityId.Dash && _dash != null && _dash.IsUnlocked)
                continue;
            if (a.Id == RogueAbilityId.Shrink && _scale != null && _scale.IsUnlocked)
                continue;
            if (a.Id == RogueAbilityId.DragNumbers)
            {
                AbilityDrag drag = _abilityDrag != null ? _abilityDrag : FindAnyObjectByType<AbilityDrag>();
                if (drag != null && drag.IsUnlocked)
                    continue;
            }

            pool.Add(a);
        }

        return pool;
    }

    private static List<RogueAbilityConfig> PickRandomUnique(List<RogueAbilityConfig> pool, int count)
    {
        var copy = new List<RogueAbilityConfig>(pool);
        var result = new List<RogueAbilityConfig>(count);

        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int index = UnityEngine.Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return result;
    }

    private void CachePlayerComponents()
    {
        if (_player == null)
            return;

        if (_move == null)
            _move = _player.GetComponent<Movement_Move>();
        if (_jump == null)
            _jump = _player.GetComponent<Movement_Jump>();
        if (_dash == null)
            _dash = _player.GetComponent<Movement_Dash>();
        if (_scale == null)
            _scale = _player.GetComponent<Movement_Scale>();
    }
}
