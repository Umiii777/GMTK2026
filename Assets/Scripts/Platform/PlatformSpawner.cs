using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 平台生成控制器。
/// 支持按高度层生成；可跟随摄像机自动向上铺设，并回收掉出屏幕下方的平台。
/// </summary>
public class PlatformSpawner : MonoBehaviour
{
    private struct PlannedPlatform
    {
        public float X;
        public float Length;
    }

    // ==================== Inspector 字段 ====================

    [Header("对象池")]
    [Tooltip("平台所用的对象池")]
    [SerializeField] private ObjectPool _pool;

    [Tooltip("金币所用的对象池；为空则不生成金币")]
    [SerializeField] private ObjectPool _coinPool;

    [Header("摄像机")]
    [Tooltip("用于判断屏幕范围的摄像机；为空则使用 Camera.main")]
    [SerializeField] private Camera _camera;

    [Header("自动生成 / 回收")]
    [Tooltip("是否根据摄像机上升自动生成与回收")]
    [SerializeField] private bool _autoMaintain = true;

    [Tooltip("在屏幕上沿之上提前生成的距离")]
    [SerializeField] private float _spawnLookahead = 4f;

    [Tooltip("掉到屏幕下沿以下多少距离后回收")]
    [SerializeField] private float _despawnMargin = 2f;

    [Tooltip("每帧最多新生成多少层，防止卡顿")]
    [SerializeField] private int _maxRowsPerFrame = 5;

    [Header("生成位置范围（世界坐标）")]
    [Tooltip("平台覆盖区域 X 的最小 / 最大值")]
    [SerializeField] private Vector2 _spawnXRange = new Vector2(-4f, 4f);

    [Tooltip("第一层平台的生成高度（Y）")]
    [SerializeField] private float _startY = 0f;

    [Header("平台长度")]
    [Tooltip("平台长度最小值")]
    [SerializeField] private float _minLength = 1.5f;

    [Tooltip("平台长度最大值")]
    [SerializeField] private float _maxLength = 4f;

    [Header("同一高度横向生成")]
    [Tooltip("同一高度最少平台数（可为 0，表示这一层跳过）")]
    [SerializeField] private int _minPerHeight = 0;

    [Tooltip("同一高度最多平台数")]
    [SerializeField] private int _maxPerHeight = 2;

    [Tooltip("同一高度两平台边缘之间的最小间距")]
    [SerializeField] private float _minHorizontalGap = 1f;

    [Tooltip("与上一层平台边缘之间的最大横向间距（保证可跳上）")]
    [SerializeField] private float _maxLayerHorizontalGap = 2.5f;

    [Tooltip("同一高度排布失败时的重试次数")]
    [SerializeField] private int _placementRetries = 40;

    [Header("高度间隔")]
    [Tooltip("相邻「有平台」的层之间的最小高度差")]
    [SerializeField] private float _minHeightGap = 1.5f;

    [Tooltip("相邻「有平台」的层之间的最大高度差（空层不会突破此上限）")]
    [SerializeField] private float _maxHeightGap = 2.5f;

    [Tooltip("高度递增方向：true = 向上生成，false = 向下生成")]
    [SerializeField] private bool _spawnUpward = true;

    [Header("启动")]
    [Tooltip("Start 时预生成的高度层数；0 表示不预生成")]
    [SerializeField] private int _spawnOnStart = 5;

    [Header("金币")]
    [Tooltip("每个平台生成金币的概率")]
    [SerializeField, Range(0f, 1f)] private float _coinSpawnChance = 0.55f;

    [Tooltip("单个平台最少金币数（通过概率判定后）")]
    [SerializeField] private int _minCoinsPerPlatform = 1;

    [Tooltip("单个平台最多金币数")]
    [SerializeField] private int _maxCoinsPerPlatform = 1;

    [Tooltip("金币相对平台中心上移的高度")]
    [SerializeField] private float _coinHeightOffset = 0.6f;

    [Tooltip("金币相对平台左右边缘的内缩，避免贴边")]
    [SerializeField] private float _coinEdgePadding = 0.25f;

    // ==================== 私有字段 ====================

    private float _nextSpawnY;
    private float _lastPlatformRowY;
    private bool _hasPlatformRow;
    private bool _hasSpawned;
    private readonly List<PlannedPlatform> _planBuffer = new List<PlannedPlatform>(8);
    private readonly List<PlannedPlatform> _previousRow = new List<PlannedPlatform>(8);
    private readonly List<Platform> _spawnBuffer = new List<Platform>(8);
    private readonly List<Platform> _activePlatforms = new List<Platform>(64);
    private readonly List<Coin> _activeCoins = new List<Coin>(64);

    // ==================== 属性 ====================

    /// <summary>下一层将生成的 Y 高度</summary>
    public float NextSpawnY => _nextSpawnY;

    /// <summary>是否已经生成过至少一层</summary>
    public bool HasSpawned => _hasSpawned;

    /// <summary>当前仍在场上的平台数量</summary>
    public int ActiveCount => _activePlatforms.Count;

    /// <summary>当前仍在场上的金币数量</summary>
    public int ActiveCoinCount => _activeCoins.Count;

    // ==================== Unity 生命周期 ====================

    private void Awake()
    {
        if (_pool == null)
            _pool = GetComponent<ObjectPool>();

        if (_camera == null)
            _camera = Camera.main;

        _nextSpawnY = _startY;
        _lastPlatformRowY = _startY;
        _hasPlatformRow = false;
        ClampInspectorValues();
    }

    private void Start()
    {
        if (_spawnOnStart > 0)
            SpawnBatch(_spawnOnStart);
    }

    private void LateUpdate()
    {
        if (!_autoMaintain)
            return;

        if (_camera == null)
        {
            _camera = Camera.main;
            if (_camera == null)
                return;
        }

        MaintainAheadOfCamera();
        DespawnBelowCamera();
        DespawnCoinsBelowCamera();
    }

    // ==================== 公共方法 ====================

    /// <summary>
    /// 生成一层：同一高度随机 0~Max 个平台，然后推进到下一高度。
    /// 返回本层实际生成的平台列表（可能为空）。
    /// 注意：高度间隔按「上一层有平台的层」计算，空层不会把间距撑破 Max Height Gap。
    /// </summary>
    public List<Platform> SpawnNext()
    {
        _spawnBuffer.Clear();

        if (_pool == null)
        {
            Debug.LogError("[PlatformSpawner] 未指定 ObjectPool。", this);
            return new List<Platform>();
        }

        int desiredCount = Random.Range(_minPerHeight, _maxPerHeight + 1);

        if (desiredCount > 0)
        {
            // 保证与上一层「有平台」的层间距在 min~max 之间
            ClampSpawnYToHeightGap();

            TrySpawnRow(desiredCount, _spawnBuffer);

            if (_spawnBuffer.Count > 0)
            {
                _lastPlatformRowY = _nextSpawnY;
                _hasPlatformRow = true;

                _previousRow.Clear();
                for (int i = 0; i < _planBuffer.Count; i++)
                    _previousRow.Add(_planBuffer[i]);
            }
        }

        AdvanceSpawnHeight();
        _hasSpawned = true;
        return new List<Platform>(_spawnBuffer);
    }

    /// <summary>
    /// 连续生成多个高度层。
    /// </summary>
    public void SpawnBatch(int heightLevels)
    {
        for (int i = 0; i < heightLevels; i++)
            SpawnNext();
    }

    /// <summary>
    /// 重置生成高度到起始 Y，并可选回收全部已生成平台。
    /// </summary>
    public void ResetSpawner(bool returnAll = true)
    {
        if (returnAll)
        {
            for (int i = _activePlatforms.Count - 1; i >= 0; i--)
                ReturnPlatform(_activePlatforms[i]);

            _activePlatforms.Clear();

            for (int i = _activeCoins.Count - 1; i >= 0; i--)
                ReturnCoin(_activeCoins[i]);

            _activeCoins.Clear();

            if (_pool != null)
                _pool.ReturnAll();

            if (_coinPool != null)
                _coinPool.ReturnAll();
        }

        _nextSpawnY = _startY;
        _lastPlatformRowY = _startY;
        _hasPlatformRow = false;
        _hasSpawned = false;
        _previousRow.Clear();
    }

    /// <summary>开启 / 关闭自动生成与回收</summary>
    public void SetAutoMaintain(bool enabled) => _autoMaintain = enabled;

    // ==================== 自动维护 ====================

    /// <summary>
    /// 摄像机上升后，把平台铺到屏幕上方 lookahead 处。
    /// </summary>
    private void MaintainAheadOfCamera()
    {
        if (!_spawnUpward)
            return;

        float targetY = GetCameraTop() + _spawnLookahead;
        int spawned = 0;

        while (_nextSpawnY <= targetY && spawned < _maxRowsPerFrame)
        {
            SpawnNext();
            spawned++;
        }
    }

    /// <summary>
    /// 回收落到屏幕下方 margin 之外的平台。
    /// </summary>
    private void DespawnBelowCamera()
    {
        float killY = GetCameraBottom() - _despawnMargin;

        for (int i = _activePlatforms.Count - 1; i >= 0; i--)
        {
            Platform platform = _activePlatforms[i];
            if (platform == null)
            {
                _activePlatforms.RemoveAt(i);
                continue;
            }

            if (platform.transform.position.y < killY)
                ReturnPlatform(platform);
        }
    }

    /// <summary>
    /// 回收落到屏幕下方的金币（与平台同一套时机）。
    /// </summary>
    private void DespawnCoinsBelowCamera()
    {
        float killY = GetCameraBottom() - _despawnMargin;

        for (int i = _activeCoins.Count - 1; i >= 0; i--)
        {
            Coin coin = _activeCoins[i];
            if (coin == null || !coin.gameObject.activeInHierarchy)
            {
                _activeCoins.RemoveAt(i);
                continue;
            }

            if (coin.transform.position.y < killY)
                ReturnCoin(coin);
        }
    }

    private float GetCameraTop()
    {
        return _camera.transform.position.y + GetOrthoHalfHeight();
    }

    private float GetCameraBottom()
    {
        return _camera.transform.position.y - GetOrthoHalfHeight();
    }

    private float GetOrthoHalfHeight()
    {
        if (_camera.orthographic)
            return _camera.orthographicSize;

        // 透视相机：用到 z=0 平面的近似可视半高
        float distance = Mathf.Abs(_camera.transform.position.z);
        return distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
    }

    private void ReturnPlatform(Platform platform)
    {
        if (platform == null)
            return;

        _activePlatforms.Remove(platform);

        if (platform.TryGetComponent(out PooledObject pooled))
            pooled.ReturnToPool();
        else if (_pool != null)
            _pool.Return(platform.gameObject);
        else
            Destroy(platform.gameObject);
    }

    private void ReturnCoin(Coin coin)
    {
        if (coin == null)
            return;

        _activeCoins.Remove(coin);

        if (coin.gameObject.activeInHierarchy)
            coin.ReturnToPool();
    }

    // ==================== 生成逻辑 ====================

    private void TrySpawnRow(int desiredCount, List<Platform> results)
    {
        if (desiredCount <= 0)
            return;

        for (int count = desiredCount; count >= 1; count--)
        {
            if (!TryPlanRow(count))
                continue;

            float y = _nextSpawnY;
            for (int i = 0; i < _planBuffer.Count; i++)
            {
                Platform platform = SpawnPlanned(_planBuffer[i], y);
                if (platform != null)
                    results.Add(platform);
            }

            return;
        }
    }

    private bool TryPlanRow(int count)
    {
        for (int attempt = 0; attempt < _placementRetries; attempt++)
        {
            _planBuffer.Clear();

            // 先放一个靠近上一层的锚点平台，再随机补其余平台
            if (!TryAddAnchorPlatform())
                continue;

            bool planned = true;
            for (int i = _planBuffer.Count; i < count; i++)
            {
                float length = Random.Range(_minLength, _maxLength);
                if (!TryPickSpawnX(length, out float x))
                {
                    planned = false;
                    break;
                }

                _planBuffer.Add(new PlannedPlatform { X = x, Length = length });
            }

            if (!planned)
                continue;

            if (IsSameLayerValid(_planBuffer) && IsReachableFromPrevious(_planBuffer))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 在上一层可达范围内放第一个平台；若无上一层则全范围随机。
    /// </summary>
    private bool TryAddAnchorPlatform()
    {
        float length = Random.Range(_minLength, _maxLength);

        if (_previousRow.Count == 0)
        {
            if (!TryPickSpawnX(length, out float x))
                return false;

            _planBuffer.Add(new PlannedPlatform { X = x, Length = length });
            return true;
        }

        PlannedPlatform prev = _previousRow[Random.Range(0, _previousRow.Count)];
        if (!TryPickSpawnXNear(prev, length, out float nearX))
            return false;

        _planBuffer.Add(new PlannedPlatform { X = nearX, Length = length });
        return true;
    }

    /// <summary>同一层内：边缘最小间距</summary>
    private bool IsSameLayerValid(List<PlannedPlatform> plan)
    {
        if (plan.Count <= 1)
            return true;

        plan.Sort((a, b) => (a.X - a.Length * 0.5f).CompareTo(b.X - b.Length * 0.5f));

        for (int i = 0; i < plan.Count - 1; i++)
        {
            float right = plan[i].X + plan[i].Length * 0.5f;
            float nextLeft = plan[i + 1].X - plan[i + 1].Length * 0.5f;
            if (nextLeft - right < _minHorizontalGap)
                return false;
        }

        return true;
    }

    /// <summary>
    /// 层间：至少有一个新平台与上一层某个平台的边缘间距 ≤ 最大横向间距。
    /// </summary>
    private bool IsReachableFromPrevious(List<PlannedPlatform> plan)
    {
        if (_previousRow.Count == 0 || plan.Count == 0)
            return true;

        for (int i = 0; i < plan.Count; i++)
        {
            for (int j = 0; j < _previousRow.Count; j++)
            {
                if (HorizontalEdgeGap(plan[i], _previousRow[j]) <= _maxLayerHorizontalGap)
                    return true;
            }
        }

        return false;
    }

    private static float HorizontalEdgeGap(PlannedPlatform a, PlannedPlatform b)
    {
        float aLeft = a.X - a.Length * 0.5f;
        float aRight = a.X + a.Length * 0.5f;
        float bLeft = b.X - b.Length * 0.5f;
        float bRight = b.X + b.Length * 0.5f;

        if (aRight < bLeft)
            return bLeft - aRight;
        if (bRight < aLeft)
            return aLeft - bRight;
        return 0f; // X 投影重叠
    }

    private Platform SpawnPlanned(PlannedPlatform planned, float y)
    {
        Vector3 position = new Vector3(planned.X, y, 0f);
        GameObject go = _pool.Get(position, Quaternion.identity);
        if (go == null)
            return null;

        if (!go.TryGetComponent(out Platform platform))
        {
            Debug.LogError("[PlatformSpawner] 平台预制体上缺少 Platform 组件，请挂上后再生成。", go);
            _pool.Return(go);
            return null;
        }

        platform.SetLength(planned.Length);

        if (!_activePlatforms.Contains(platform))
            _activePlatforms.Add(platform);

        TrySpawnCoinsOnPlatform(platform);
        return platform;
    }

    /// <summary>
    /// 按概率在平台顶上生成若干金币。
    /// </summary>
    private void TrySpawnCoinsOnPlatform(Platform platform)
    {
        if (_coinPool == null || platform == null)
            return;

        if (Random.value > _coinSpawnChance)
            return;

        int count = Random.Range(_minCoinsPerPlatform, _maxCoinsPerPlatform + 1);
        if (count <= 0)
            return;

        float half = platform.Length * 0.5f;
        float pad = Mathf.Min(_coinEdgePadding, Mathf.Max(0f, half - 0.05f));
        float minX = platform.transform.position.x - half + pad;
        float maxX = platform.transform.position.x + half - pad;
        float y = platform.transform.position.y + _coinHeightOffset;

        if (minX > maxX)
        {
            minX = maxX = platform.transform.position.x;
        }

        for (int i = 0; i < count; i++)
        {
            float x = count == 1
                ? (minX + maxX) * 0.5f
                : Mathf.Lerp(minX, maxX, (i + 0.5f) / count);

            // 多个时略微随机，避免完全等距呆板
            if (count > 1)
                x = Mathf.Clamp(x + Random.Range(-pad * 0.25f, pad * 0.25f), minX, maxX);

            Vector3 pos = new Vector3(x, y, 0f);
            GameObject go = _coinPool.Get(pos, Quaternion.identity);
            if (go == null)
                continue;

            if (!go.TryGetComponent(out Coin coin))
            {
                Debug.LogError("[PlatformSpawner] 金币预制体上缺少 Coin 组件。", go);
                _coinPool.Return(go);
                continue;
            }

            if (!_activeCoins.Contains(coin))
                _activeCoins.Add(coin);
        }
    }

    private bool TryPickSpawnX(float length, out float x)
    {
        if (!GetSpawnXBounds(length, out float minX, out float maxX))
        {
            x = (_spawnXRange.x + _spawnXRange.y) * 0.5f;
            return length <= (_spawnXRange.y - _spawnXRange.x) + 0.0001f;
        }

        x = Random.Range(minX, maxX);
        return true;
    }

    /// <summary>
    /// 在上一层平台附近选 X，使两平台边缘间距不超过 Max Layer Horizontal Gap。
    /// </summary>
    private bool TryPickSpawnXNear(PlannedPlatform prev, float length, out float x)
    {
        if (!GetSpawnXBounds(length, out float rangeMin, out float rangeMax))
        {
            x = (_spawnXRange.x + _spawnXRange.y) * 0.5f;
            return false;
        }

        float half = length * 0.5f;
        float prevLeft = prev.X - prev.Length * 0.5f;
        float prevRight = prev.X + prev.Length * 0.5f;

        // 新平台区间需与「上一层外扩 maxGap」后的区间相交
        float nearMin = prevLeft - _maxLayerHorizontalGap - half;
        float nearMax = prevRight + _maxLayerHorizontalGap + half;

        float minX = Mathf.Max(rangeMin, nearMin);
        float maxX = Mathf.Min(rangeMax, nearMax);

        if (minX > maxX)
        {
            // 可达区与生成范围无交集时，退回到最接近上一层中心的合法点
            x = Mathf.Clamp(prev.X, rangeMin, rangeMax);
            return HorizontalEdgeGap(
                       new PlannedPlatform { X = x, Length = length },
                       prev) <= _maxLayerHorizontalGap + 0.0001f;
        }

        x = Random.Range(minX, maxX);
        return true;
    }

    private bool GetSpawnXBounds(float length, out float minX, out float maxX)
    {
        float half = length * 0.5f;
        minX = _spawnXRange.x + half;
        maxX = _spawnXRange.y - half;
        return minX <= maxX;
    }

    private void AdvanceSpawnHeight()
    {
        float gap = Random.Range(_minHeightGap, _maxHeightGap);
        _nextSpawnY += _spawnUpward ? gap : -gap;
    }

    /// <summary>
    /// 将下一层生成高度钳制到与上一层有平台层的合法间距内。
    /// </summary>
    private void ClampSpawnYToHeightGap()
    {
        if (!_hasPlatformRow)
            return;

        float dir = _spawnUpward ? 1f : -1f;
        float delta = (_nextSpawnY - _lastPlatformRowY) * dir;

        // 空层叠加或生成失败导致间距过大 / 过小 → 重新随机一个合法间隔
        if (delta > _maxHeightGap + 0.0001f || delta < _minHeightGap - 0.0001f)
        {
            float gap = Random.Range(_minHeightGap, _maxHeightGap);
            _nextSpawnY = _lastPlatformRowY + dir * gap;
        }
    }

    private void ClampInspectorValues()
    {
        if (_spawnXRange.x > _spawnXRange.y)
        {
            float tmp = _spawnXRange.x;
            _spawnXRange.x = _spawnXRange.y;
            _spawnXRange.y = tmp;
        }

        if (_minLength > _maxLength)
        {
            float tmp = _minLength;
            _minLength = _maxLength;
            _maxLength = tmp;
        }

        if (_minHeightGap > _maxHeightGap)
        {
            float tmp = _minHeightGap;
            _minHeightGap = _maxHeightGap;
            _maxHeightGap = tmp;
        }

        _minLength = Mathf.Max(0.01f, _minLength);
        _maxLength = Mathf.Max(_minLength, _maxLength);
        _minHeightGap = Mathf.Max(0f, _minHeightGap);
        _maxHeightGap = Mathf.Max(_minHeightGap, _maxHeightGap);

        _minPerHeight = Mathf.Max(0, _minPerHeight);
        _maxPerHeight = Mathf.Max(_minPerHeight, _maxPerHeight);
        _minHorizontalGap = Mathf.Max(0f, _minHorizontalGap);
        _maxLayerHorizontalGap = Mathf.Max(0f, _maxLayerHorizontalGap);
        _placementRetries = Mathf.Max(1, _placementRetries);

        _spawnLookahead = Mathf.Max(0f, _spawnLookahead);
        _despawnMargin = Mathf.Max(0f, _despawnMargin);
        _maxRowsPerFrame = Mathf.Max(1, _maxRowsPerFrame);

        _coinSpawnChance = Mathf.Clamp01(_coinSpawnChance);
        _minCoinsPerPlatform = Mathf.Max(0, _minCoinsPerPlatform);
        _maxCoinsPerPlatform = Mathf.Max(_minCoinsPerPlatform, _maxCoinsPerPlatform);
        _coinEdgePadding = Mathf.Max(0f, _coinEdgePadding);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ClampInspectorValues();
    }

    private void OnDrawGizmosSelected()
    {
        Camera cam = _camera != null ? _camera : Camera.main;
        float y = Application.isPlaying ? _nextSpawnY : _startY;
        float midX = (_spawnXRange.x + _spawnXRange.y) * 0.5f;
        float width = Mathf.Abs(_spawnXRange.y - _spawnXRange.x);

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.35f);
        Gizmos.DrawCube(new Vector3(midX, y, 0f), new Vector3(width, 0.1f, 0.1f));

        Gizmos.color = new Color(0.2f, 0.85f, 1f, 0.9f);
        Gizmos.DrawLine(new Vector3(_spawnXRange.x, y - 0.25f, 0f),
                        new Vector3(_spawnXRange.x, y + 0.25f, 0f));
        Gizmos.DrawLine(new Vector3(_spawnXRange.y, y - 0.25f, 0f),
                        new Vector3(_spawnXRange.y, y + 0.25f, 0f));

        if (cam != null)
        {
            float halfH = cam.orthographic
                ? cam.orthographicSize
                : Mathf.Abs(cam.transform.position.z) *
                  Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);

            float top = cam.transform.position.y + halfH;
            float bottom = cam.transform.position.y - halfH;
            float spawnLine = top + _spawnLookahead;
            float killLine = bottom - _despawnMargin;

            Gizmos.color = new Color(0.3f, 1f, 0.4f, 0.9f);
            Gizmos.DrawLine(new Vector3(_spawnXRange.x, spawnLine, 0f),
                            new Vector3(_spawnXRange.y, spawnLine, 0f));

            Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.9f);
            Gizmos.DrawLine(new Vector3(_spawnXRange.x, killLine, 0f),
                            new Vector3(_spawnXRange.y, killLine, 0f));
        }
    }
#endif
}
