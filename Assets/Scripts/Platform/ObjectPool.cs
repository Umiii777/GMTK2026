using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 通用对象池。挂到场景中，指定预制体后即可 Get / Return。
/// 适合平台等需要频繁生成、回收的物体。
/// </summary>
public class ObjectPool : MonoBehaviour
{
    // ==================== Inspector 字段 ====================

    [Header("池配置")]
    [Tooltip("要池化的预制体")]
    [SerializeField] private GameObject _prefab;

    [Tooltip("启动时预创建的数量")]
    [SerializeField] private int _initialSize = 10;

    [Tooltip("池空时是否允许继续 Instantiate 扩容")]
    [SerializeField] private bool _expandable = true;

    [Tooltip("扩容上限；0 表示不限制")]
    [SerializeField] private int _maxSize = 0;

    [Tooltip("回收时是否重置为预制体的本地缩放")]
    [SerializeField] private bool _resetScaleOnReturn = true;

    // ==================== 私有字段 ====================

    private readonly Queue<GameObject> _available = new Queue<GameObject>();
    private readonly HashSet<GameObject> _inUse = new HashSet<GameObject>();
    private Transform _poolRoot;

    // ==================== 属性 ====================

    /// <summary>当前空闲数量</summary>
    public int AvailableCount => _available.Count;

    /// <summary>当前已取出数量</summary>
    public int InUseCount => _inUse.Count;

    /// <summary>池中对象总数（空闲 + 使用中）</summary>
    public int TotalCount => AvailableCount + InUseCount;

    /// <summary>绑定的预制体</summary>
    public GameObject Prefab => _prefab;

    // ==================== Unity 生命周期 ====================

    private void Awake()
    {
        if (_prefab == null)
        {
            Debug.LogError($"[ObjectPool] {name} 未指定预制体。", this);
            enabled = false;
            return;
        }

        _poolRoot = new GameObject($"{_prefab.name}_Pool").transform;
        _poolRoot.SetParent(transform);

        Prewarm(_initialSize);
    }

    // ==================== 公共方法 ====================

    /// <summary>
    /// 从池中取出一个对象，放到指定位置与旋转。
    /// </summary>
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = TakeFromPool();
        if (obj == null)
            return null;

        Transform t = obj.transform;
        t.SetParent(null);
        t.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        NotifySpawned(obj);
        return obj;
    }

    /// <summary>
    /// 从池中取出一个对象，放到指定位置（旋转保持预制体默认）。
    /// </summary>
    public GameObject Get(Vector3 position)
    {
        return Get(position, _prefab.transform.rotation);
    }

    /// <summary>
    /// 从池中取出并获取指定组件；失败时返回 null。
    /// </summary>
    public T Get<T>(Vector3 position, Quaternion rotation) where T : Component
    {
        GameObject obj = Get(position, rotation);
        if (obj == null)
            return null;

        if (obj.TryGetComponent(out T component))
            return component;

        Debug.LogWarning($"[ObjectPool] {obj.name} 上找不到组件 {typeof(T).Name}，已回收。", obj);
        Return(obj);
        return null;
    }

    /// <summary>
    /// 将对象归还到池中。非本池创建的对象会被忽略。
    /// </summary>
    public void Return(GameObject obj)
    {
        if (obj == null)
            return;

        if (!_inUse.Remove(obj))
        {
            // 可能已经归还，或不是本池对象
            if (!_available.Contains(obj))
                Debug.LogWarning($"[ObjectPool] 尝试归还非本池对象：{obj.name}", obj);
            return;
        }

        NotifyDespawned(obj);

        Transform t = obj.transform;
        t.SetParent(_poolRoot);
        if (_resetScaleOnReturn)
            t.localScale = _prefab.transform.localScale;

        obj.SetActive(false);
        _available.Enqueue(obj);
    }

    /// <summary>
    /// 回收所有当前在使用中的对象。
    /// </summary>
    public void ReturnAll()
    {
        // 复制一份，避免遍历时集合被修改
        var snapshot = new List<GameObject>(_inUse);
        for (int i = 0; i < snapshot.Count; i++)
            Return(snapshot[i]);
    }

    /// <summary>
    /// 额外预创建若干空闲对象。
    /// </summary>
    public void Prewarm(int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (!CanCreateMore())
                break;

            GameObject obj = CreateInstance();
            obj.SetActive(false);
            _available.Enqueue(obj);
        }
    }

    // ==================== 私有方法 ====================

    private GameObject TakeFromPool()
    {
        GameObject obj = null;

        while (_available.Count > 0 && obj == null)
        {
            obj = _available.Dequeue();
            // 防止外部 Destroy 导致空引用
            if (obj == null)
                continue;
        }

        if (obj == null)
        {
            if (!CanCreateMore())
            {
                Debug.LogWarning($"[ObjectPool] {_prefab.name} 池已满且不可扩容。", this);
                return null;
            }

            obj = CreateInstance();
        }

        _inUse.Add(obj);
        return obj;
    }

    private bool CanCreateMore()
    {
        if (_maxSize > 0 && TotalCount >= _maxSize)
            return false;

        // 预热阶段允许创建；运行时扩容受 _expandable 控制
        if (TotalCount >= _initialSize && !_expandable)
            return false;

        return true;
    }

    private GameObject CreateInstance()
    {
        GameObject obj = Instantiate(_prefab, _poolRoot);
        obj.name = $"{_prefab.name}_{TotalCount:00}";

        // 方便平台脚本一键归还
        var pooled = obj.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = obj.AddComponent<PooledObject>();
        pooled.Bind(this);

        return obj;
    }

    private static void NotifySpawned(GameObject obj)
    {
        var listeners = obj.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].OnSpawned();
    }

    private static void NotifyDespawned(GameObject obj)
    {
        var listeners = obj.GetComponentsInChildren<IPoolable>(true);
        for (int i = 0; i < listeners.Length; i++)
            listeners[i].OnDespawned();
    }
}

/// <summary>
/// 可选接口：挂在预制体上，在取出 / 回收时收到回调。
/// </summary>
public interface IPoolable
{
    void OnSpawned();
    void OnDespawned();
}

/// <summary>
/// 自动挂到池化实例上，提供 ReturnToPool() 便捷归还。
/// </summary>
public class PooledObject : MonoBehaviour
{
    private ObjectPool _pool;

    public ObjectPool Pool => _pool;

    public void Bind(ObjectPool pool)
    {
        _pool = pool;
    }

    /// <summary>将自身归还到所属对象池</summary>
    public void ReturnToPool()
    {
        if (_pool != null)
            _pool.Return(gameObject);
        else
            Destroy(gameObject);
    }
}
