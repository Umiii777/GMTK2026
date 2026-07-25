using System;
using UnityEngine;

/// <summary>
/// 可收集金币。由对象池生成，碰到 Player 后回收。
/// 预制体需带 Trigger Collider2D，玩家需有 Collider2D（建议 Tag = Player）。
/// </summary>
public class Coin : MonoBehaviour, IPoolable
{
    [SerializeField] private int _value = 1;
    [Tooltip("只对带此 Tag 的对象生效")]
    [SerializeField] private string _playerTag = "Player";

    /// <summary>收集时触发，参数为金币分值</summary>
    public static event Action<int> Collected;

    public int Value => _value;

    private bool _collected;

    public void OnSpawned()
    {
        _collected = false;
    }

    public void OnDespawned()
    {
        _collected = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected)
            return;

        if (!other.CompareTag(_playerTag))
            return;

        _collected = true;
        Collected?.Invoke(_value);
        ReturnToPool();
    }

    /// <summary>回收到所属对象池</summary>
    public void ReturnToPool()
    {
        if (TryGetComponent(out PooledObject pooled))
            pooled.ReturnToPool();
        else
            gameObject.SetActive(false);
    }
}
