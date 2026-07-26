using UnityEngine;

/// <summary>
/// 单个平台实例。由 PlatformSpawner 取出后设置长度，回收时自动重置。
/// 预制体建议默认长度为 1（scale.x = 1），这样 SetLength 的值等于世界单位宽度。
/// </summary>
public class Platform : MonoBehaviour, IPoolable
{
    [Tooltip("预制体在长度=1 时的基准 scale.x；一般保持 1")]
    [SerializeField] private float _baseScaleX = 1f;

    [Header("外观")]
    [Tooltip("生成时从中随机选一个 Sprite；留空则不改")]
    [SerializeField] private Sprite[] _spriteVariants;

    [Tooltip("要换图的渲染器；为空则自动找自身或子物体")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("单向平台")]
    [Tooltip("自动配置 PlatformEffector2D，可从下方穿过")]
    [SerializeField] private bool _oneWay = true;

    [SerializeField, Range(1f, 360f)] private float _surfaceArc = 180f;

    private Vector3 _baseScale;
    private bool _baseScaleCached;
    private float _length = 1f;

    /// <summary>当前平台长度（世界单位）</summary>
    public float Length => _length;

    private void Awake()
    {
        CacheBaseScale();
        EnsureOneWayPlatform();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        PickRandomSprite();
    }

    public void OnDespawned()
    {
        CacheBaseScale();
        transform.localScale = _baseScale;
        _length = 1f;
    }

    private void PickRandomSprite()
    {
        if (_spriteRenderer == null || _spriteVariants == null || _spriteVariants.Length == 0)
            return;

        int index = Random.Range(0, _spriteVariants.Length);
        Sprite sprite = _spriteVariants[index];
        if (sprite != null)
            _spriteRenderer.sprite = sprite;
    }

    private void EnsureOneWayPlatform()
    {
        if (!_oneWay)
            return;

        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
            return;

        col.usedByEffector = true;

        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (effector == null)
            effector = gameObject.AddComponent<PlatformEffector2D>();

        effector.useOneWay = true;
        effector.surfaceArc = _surfaceArc;
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
