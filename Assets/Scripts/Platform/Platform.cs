using UnityEngine;

/// <summary>
/// 单个平台实例。由 PlatformSpawner 取出后设置长度，回收时自动重置。
/// 预制体建议默认长度为 1（scale.x = 1），这样 SetLength 的值等于世界单位宽度。
/// </summary>
public class Platform : MonoBehaviour, IPoolable
{
    [Tooltip("预制体在长度=1 时的基准 scale.x；一般保持 1")]
    [SerializeField] private float _baseScaleX = 1f;

    private Vector3 _baseScale;
    private bool _baseScaleCached;
    private float _length = 1f;

    /// <summary>当前平台长度（世界单位）</summary>
    public float Length => _length;

    private void Awake()
    {
        CacheBaseScale();
    }

    /// <summary>
    /// 设置平台长度。通过缩放 X 实现，碰撞体随 Transform 一起变化。
    /// </summary>
    public void SetLength(float length)
    {
        CacheBaseScale();

        _length = Mathf.Max(0.01f, length);

        Vector3 scale = _baseScale;
        scale.x = _baseScaleX * _length;
        transform.localScale = scale;
    }

    public void OnSpawned()
    {
        CacheBaseScale();
        transform.localScale = _baseScale;
        _length = 1f;
    }

    public void OnDespawned()
    {
        CacheBaseScale();
        transform.localScale = _baseScale;
        _length = 1f;
    }

    private void CacheBaseScale()
    {
        if (_baseScaleCached)
            return;

        if (_baseScaleX <= 0f)
            _baseScaleX = 1f;

        _baseScale = transform.localScale;
        // 避免首次拿到异常缩放
        if (Mathf.Approximately(_baseScale.x, 0f))
            _baseScale.x = _baseScaleX;
        if (Mathf.Approximately(_baseScale.y, 0f))
            _baseScale.y = 1f;
        if (Mathf.Approximately(_baseScale.z, 0f))
            _baseScale.z = 1f;

        _baseScaleCached = true;
    }
}
