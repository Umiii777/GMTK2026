using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 游戏暂停管理：暂停物理 / 玩家输入 / 相机上升。
/// 弹出界面时调用 Pause()，关闭界面时调用 Resume()。
/// 支持嵌套暂停（多次 Pause 需同等次数 Resume）。
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    /// <summary>当前是否处于暂停（静态，供输入脚本查询）</summary>
    public static bool IsPaused => Instance != null && Instance._pauseCount > 0;

    [Header("引用")]
    [Tooltip("需要暂停的相机上升组件；为空则从主相机查找")]
    [SerializeField] private CameraRise _cameraRise;

    [Header("选项")]
    [Tooltip("暂停时把玩家速度清零，避免恢复时残留冲量")]
    [SerializeField] private bool _clearPlayerVelocity = true;

    [Tooltip("玩家物体；为空则按 Tag=Player 查找")]
    [SerializeField] private Rigidbody2D _playerBody;

    [Header("事件")]
    public UnityEvent OnPaused;
    public UnityEvent OnResumed;

    private int _pauseCount;
    private float _cachedTimeScale = 1f;
    private bool _cameraWasRising;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[PauseManager] 场景中存在多个 PauseManager，保留第一个。", this);
            return;
        }

        Instance = this;

        if (_cameraRise == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                _cameraRise = cam.GetComponent<CameraRise>();
        }

        if (_playerBody == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
                _playerBody = player.GetComponent<Rigidbody2D>();
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            // 防止退出时卡在 timeScale = 0
            if (_pauseCount > 0)
                Time.timeScale = _cachedTimeScale > 0f ? _cachedTimeScale : 1f;

            Instance = null;
        }
    }

    /// <summary>暂停游戏（可重复调用，需配对 Resume）</summary>
    public void Pause()
    {
        _pauseCount++;
        if (_pauseCount > 1)
            return;

        _cachedTimeScale = Time.timeScale > 0f ? Time.timeScale : 1f;
        Time.timeScale = 0f;

        if (_cameraRise != null)
        {
            _cameraWasRising = _cameraRise.IsRising;
            _cameraRise.Pause();
        }

        if (_clearPlayerVelocity && _playerBody != null)
            _playerBody.velocity = Vector2.zero;

        OnPaused?.Invoke();
    }

    /// <summary>恢复游戏</summary>
    public void Resume()
    {
        if (_pauseCount <= 0)
            return;

        _pauseCount--;
        if (_pauseCount > 0)
            return;

        Time.timeScale = _cachedTimeScale > 0f ? _cachedTimeScale : 1f;

        if (_cameraRise != null && _cameraWasRising)
            _cameraRise.Play();

        OnResumed?.Invoke();
    }

    /// <summary>强制恢复（清空嵌套计数）</summary>
    public void ForceResume()
    {
        if (_pauseCount <= 0)
            return;

        _pauseCount = 1;
        Resume();
    }

    /// <summary>供 UI Toggle / 难度事件使用</summary>
    public void SetPaused(bool paused)
    {
        if (paused)
            Pause();
        else
            ForceResume();
    }
}
