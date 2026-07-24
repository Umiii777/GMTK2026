using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BGM 播放模式
/// </summary>
public enum BGMPlayMode
{
    /// <summary>列表循环：按顺序循环播放列表中所有曲目</summary>
    ListLoop,
    /// <summary>单曲循环：反复播放当前曲目</summary>
    SingleLoop,
    /// <summary>随机播放：随机选择下一首</summary>
    Shuffle
}

/// <summary>
/// 全局音乐管理器
/// - Inspector 中自由配置 BGM 列表
/// - 支持列表循环 / 单曲循环 / 随机播放
/// - 跨场景持久化（DontDestroyOnLoad）
/// - 通过 MusicManager.Instance 在任何脚本中调用
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    // ==================== 单例 ====================

    private static MusicManager _instance;
    public static MusicManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<MusicManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("MusicManager");
                    _instance = go.AddComponent<MusicManager>();
                }
            }
            return _instance;
        }
    }

    // ==================== Inspector 字段 ====================

    [Header("BGM 列表")]
    [Tooltip("在此添加所有背景音乐文件")]
    [SerializeField] private List<AudioClip> _bgmList = new List<AudioClip>();

    [Header("播放设置")]
    [Tooltip("播放模式")]
    [SerializeField] private BGMPlayMode _playMode = BGMPlayMode.ListLoop;

    [Tooltip("默认音量 (0 ~ 1)")]
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;

    [Tooltip("场景加载后是否自动开始播放")]
    [SerializeField] private bool _playOnAwake = true;

    [Tooltip("是否在 Start 时从上次中断的位置继续播放（同一首）")]
    [SerializeField] private bool _resumeOnStart = false;

    [Header("淡入淡出")]
    [Tooltip("启用淡入淡出效果")]
    [SerializeField] private bool _enableFade = true;

    [Tooltip("淡入/淡出时长（秒）")]
    [SerializeField] private float _fadeDuration = 1.5f;

    // ==================== 私有字段 ====================

    private AudioSource _audioSource;
    private int _currentIndex = -1;
    private List<int> _shuffleOrder;
    private int _shufflePosition;
    private Coroutine _fadeCoroutine;
    private bool _isPaused;

    // ==================== 属性 ====================

    /// <summary>当前正在播放的曲目索引（-1 表示无）</summary>
    public int CurrentIndex => _currentIndex;

    /// <summary>当前正在播放的 AudioClip（可能为 null）</summary>
    public AudioClip CurrentClip =>
        (_currentIndex >= 0 && _currentIndex < _bgmList.Count) ? _bgmList[_currentIndex] : null;

    /// <summary>当前是否正在播放</summary>
    public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

    /// <summary>BGM 列表曲目数量</summary>
    public int TrackCount => _bgmList.Count;

    /// <summary>获取或设置播放模式</summary>
    public BGMPlayMode PlayMode
    {
        get => _playMode;
        set
        {
            _playMode = value;
            if (_playMode == BGMPlayMode.Shuffle)
                BuildShuffleOrder();
        }
    }

    /// <summary>获取当前音量的快照；通过 SetVolume 设置音量</summary>
    public float Volume => _volume;

    // ==================== Unity 生命周期 ====================

    private void Awake()
    {
        // 单例 + 跨场景
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = GetComponent<AudioSource>();
        _audioSource.playOnAwake = false;
        _audioSource.loop = false;  // 由脚本控制循环逻辑
        _audioSource.volume = _volume;
    }

    private void Start()
    {
        if (_bgmList.Count == 0)
        {
            Debug.LogWarning("[MusicManager] BGM 列表为空，请先在 Inspector 中添加 AudioClip。");
            return;
        }

        if (_playMode == BGMPlayMode.Shuffle)
            BuildShuffleOrder();

        if (_playOnAwake)
        {
            if (_resumeOnStart && _currentIndex >= 0)
            {
                // 继续之前中断的位置（场景切换后恢复）
                if (!IsPlaying)
                    _audioSource.UnPause();
            }
            else
            {
                Play();
            }
        }
    }

    /// <summary>
    /// 订阅场景加载事件，实现自动跨场景播放
    /// </summary>
    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene,
                               UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        // 如果当前在播放中，新场景加载后继续（AudioSource 不会因场景切换而停止，
        // 因为 MusicManager 已在 DontDestroyOnLoad 中）
        if (!IsPlaying && !_isPaused && _bgmList.Count > 0)
        {
            Play();
        }
    }

    private void Update()
    {
        // 检测当前曲目播放完毕 → 播放下一首
        if (_audioSource != null && !_audioSource.isPlaying && !_isPaused && _bgmList.Count > 0)
        {
            // AudioSource 可能还未开始播放（Start 中刚调用 Play）
            if (_audioSource.timeSamples > 0 || _audioSource.clip != null)
            {
                PlayNext();
            }
        }
    }

    // ==================== 公共方法 ====================

    /// <summary>
    /// 开始播放。如果当前已暂停则恢复；否则从列表第一个（或上次索引）开始。
    /// </summary>
    public void Play()
    {
        if (_bgmList.Count == 0)
        {
            Debug.LogWarning("[MusicManager] BGM 列表为空，无法播放。");
            return;
        }

        _isPaused = false;

        if (_currentIndex < 0)
            _currentIndex = 0;

        PlayIndex(_currentIndex);
    }

    /// <summary>
    /// 播放指定索引的曲目（0-based）
    /// </summary>
    public void PlayIndex(int index)
    {
        if (_bgmList.Count == 0) return;
        if (index < 0 || index >= _bgmList.Count) return;

        _currentIndex = index;
        _isPaused = false;

        if (_enableFade && _audioSource.isPlaying)
        {
            // 带淡入淡出的切换
            if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
            _fadeCoroutine = StartCoroutine(FadeToClip(_bgmList[index]));
        }
        else
        {
            _audioSource.clip = _bgmList[index];
            _audioSource.volume = _volume;
            _audioSource.Play();
        }
    }

    /// <summary>
    /// 暂停播放
    /// </summary>
    public void Pause()
    {
        _isPaused = true;
        _audioSource.Pause();
    }

    /// <summary>
    /// 恢复播放
    /// </summary>
    public void Resume()
    {
        _isPaused = false;
        _audioSource.UnPause();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        _isPaused = false;
        _audioSource.Stop();
        _audioSource.clip = null;
    }

    /// <summary>
    /// 播放下一首（根据播放模式自动选择）
    /// </summary>
    public void PlayNext()
    {
        if (_bgmList.Count == 0) return;

        int nextIndex;
        switch (_playMode)
        {
            case BGMPlayMode.SingleLoop:
                nextIndex = Mathf.Max(_currentIndex, 0);
                break;

            case BGMPlayMode.Shuffle:
                _shufflePosition++;
                if (_shufflePosition >= _shuffleOrder.Count)
                {
                    BuildShuffleOrder();
                    _shufflePosition = 0;
                }
                nextIndex = _shuffleOrder[_shufflePosition];
                break;

            case BGMPlayMode.ListLoop:
            default:
                nextIndex = (_currentIndex + 1) % _bgmList.Count;
                break;
        }

        PlayIndex(nextIndex);
    }

    /// <summary>
    /// 播放上一首
    /// </summary>
    public void PlayPrevious()
    {
        if (_bgmList.Count == 0) return;

        int prevIndex;
        switch (_playMode)
        {
            case BGMPlayMode.SingleLoop:
                prevIndex = Mathf.Max(_currentIndex, 0);
                break;

            case BGMPlayMode.Shuffle:
                _shufflePosition--;
                if (_shufflePosition < 0)
                {
                    BuildShuffleOrder();
                    _shufflePosition = _shuffleOrder.Count - 1;
                }
                prevIndex = _shuffleOrder[_shufflePosition];
                break;

            case BGMPlayMode.ListLoop:
            default:
                prevIndex = _currentIndex - 1;
                if (prevIndex < 0) prevIndex = _bgmList.Count - 1;
                break;
        }

        PlayIndex(prevIndex);
    }

    /// <summary>
    /// 设置音量 (0 ~ 1)
    /// </summary>
    public void SetVolume(float volume)
    {
        _volume = Mathf.Clamp01(volume);
        _audioSource.volume = _volume;
    }

    /// <summary>
    /// 获取完整的 BGM 名称列表（方便做 UI）
    /// </summary>
    public string[] GetTrackNames()
    {
        string[] names = new string[_bgmList.Count];
        for (int i = 0; i < _bgmList.Count; i++)
        {
            names[i] = _bgmList[i] != null ? _bgmList[i].name : "(null)";
        }
        return names;
    }

    /// <summary>
    /// 运行时向列表追加一首 BGM
    /// </summary>
    public void AddTrack(AudioClip clip)
    {
        if (clip == null) return;
        _bgmList.Add(clip);
        if (_playMode == BGMPlayMode.Shuffle)
            BuildShuffleOrder();
    }

    /// <summary>
    /// 运行时从列表移除指定索引的 BGM
    /// </summary>
    public void RemoveTrack(int index)
    {
        if (index < 0 || index >= _bgmList.Count) return;

        bool isCurrent = (index == _currentIndex);

        _bgmList.RemoveAt(index);

        if (_bgmList.Count == 0)
        {
            Stop();
            _currentIndex = -1;
            return;
        }

        // 调整当前索引
        if (isCurrent)
        {
            _currentIndex = Mathf.Min(index, _bgmList.Count - 1);
            PlayIndex(_currentIndex);
        }
        else if (index < _currentIndex)
        {
            _currentIndex--;
        }

        if (_playMode == BGMPlayMode.Shuffle)
            BuildShuffleOrder();
    }

    // ==================== 私有方法 ====================

    private void BuildShuffleOrder()
    {
        _shuffleOrder = new List<int>(_bgmList.Count);
        for (int i = 0; i < _bgmList.Count; i++)
            _shuffleOrder.Add(i);

        // Fisher-Yates 洗牌
        for (int i = _shuffleOrder.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            int temp = _shuffleOrder[i];
            _shuffleOrder[i] = _shuffleOrder[j];
            _shuffleOrder[j] = temp;
        }

        _shufflePosition = 0;
    }

    /// <summary>
    /// 淡入淡出切换到新曲目
    /// </summary>
    private IEnumerator FadeToClip(AudioClip newClip)
    {
        // 淡出
        float startVolume = _audioSource.volume;
        float elapsed = 0f;
        while (elapsed < _fadeDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / (_fadeDuration * 0.5f));
            yield return null;
        }

        _audioSource.volume = 0f;
        _audioSource.Stop();

        // 切换曲目
        _audioSource.clip = newClip;
        _audioSource.Play();

        // 淡入
        elapsed = 0f;
        while (elapsed < _fadeDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            _audioSource.volume = Mathf.Lerp(0f, _volume, elapsed / (_fadeDuration * 0.5f));
            yield return null;
        }

        _audioSource.volume = _volume;
        _fadeCoroutine = null;
    }

    // ==================== 调试 ====================

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Inspector 中修改音量时实时生效
        if (_audioSource != null && Application.isPlaying)
        {
            _audioSource.volume = _volume;
        }
    }
#endif
}