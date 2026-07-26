using UnityEngine;

/// <summary>
/// 摄像机上升：
/// - 每帧至少按基础速度上移，不会原地卡住
/// - 角色偏高时额外向上平滑跟上
/// </summary>
public class CameraRise : MonoBehaviour
{
    [Header("目标")]
    [Tooltip("跟随的角色；为空则只做匀速上升")]
    [SerializeField] private Transform _target;

    [Header("基础上升")]
    [Tooltip("相机最低上升速度（单位/秒），保证镜头不会停住")]
    [SerializeField] private float _speed = 2f;

    [Tooltip("是否在 Start 时自动开始上升")]
    [SerializeField] private bool _playOnStart = true;

    [Tooltip("使用非缩放时间（不受 Time.timeScale 影响）")]
    [SerializeField] private bool _useUnscaledTime = false;

    [Header("跳跃跟随")]
    [Tooltip("角色相对相机中心偏下的距离")]
    [SerializeField] private float _playerOffsetY = 1.5f;

    [Tooltip("额外上跟的平滑时间，越小切换越快")]
    [SerializeField] [Min(0.01f)] private float _smoothTime = 0.08f;

    [Tooltip("额外上跟的最大速度，0 表示不限制")]
    [SerializeField] private float _maxFollowSpeed = 40f;

    private bool _isRising;
    private float _velocityY;

    public float Speed
    {
        get => _speed;
        set => _speed = Mathf.Max(0f, value);
    }

    public bool IsRising => _isRising;

    /// <summary>供 UnityEvent / 难度系统调用</summary>
    public void SetSpeed(float speed) => Speed = speed;

    private void Start()
    {
        _velocityY = 0f;

        if (_target == null)
            TryFindPlayer();

        if (_playOnStart)
            Play();
    }

    private void LateUpdate()
    {
        if (!_isRising)
            return;

        float dt = _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        if (dt <= 0f)
            return;

        float currentY = transform.position.y;

        // 本帧最低要到达的高度：保证持续上移，不会原地不动
        float minY = currentY + _speed * dt;

        // 角色跳高时的跟随目标
        float targetY = minY;
        if (_target != null)
        {
            float followY = _target.position.y - _playerOffsetY;
            if (followY > targetY)
                targetY = followY;
        }

        float newY = minY;
        if (targetY > minY)
        {
            // 只对「超出基础上升」的部分做平滑，并保证不低于 minY
            float maxSpeed = _maxFollowSpeed > 0f ? _maxFollowSpeed : Mathf.Infinity;
            newY = Mathf.SmoothDamp(currentY, targetY, ref _velocityY, _smoothTime, maxSpeed, dt);
            if (newY < minY)
                newY = minY;
        }
        else
        {
            _velocityY = 0f;
        }

        Vector3 pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }

    public void Play() => _isRising = true;

    public void Pause() => _isRising = false;

    public void Toggle() => _isRising = !_isRising;

    public void SetTarget(Transform target) => _target = target;

    public void SnapToY(float y)
    {
        Vector3 pos = transform.position;
        pos.y = y;
        transform.position = pos;
        _velocityY = 0f;
    }

    private void TryFindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
            return;
        }

        var jump = FindObjectOfType<Movement_Jump>();
        if (jump != null)
            _target = jump.transform;
    }
}
