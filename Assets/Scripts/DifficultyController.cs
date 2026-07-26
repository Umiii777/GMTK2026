using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 难度 / 高度里程碑系统。
/// 摄像机上升到指定高度时触发事件：可改相机速度、拉起 UI 等（Inspector 里自由配置）。
/// </summary>
public class DifficultyController : MonoBehaviour
{
    public enum HeightMode
    {
        /// <summary>相对游戏开始时的相机高度（推荐）</summary>
        RiseFromStart,
        /// <summary>世界坐标绝对 Y</summary>
        AbsoluteY
    }

    [Serializable]
    public class HeightMilestone
    {
        [Tooltip("方便识别的名称")]
        public string Name = "Milestone";

        [Tooltip("触发高度（RiseFromStart = 上升了多少；AbsoluteY = 世界 Y）")]
        public float Height = 10f;

        [Tooltip("是否只触发一次")]
        public bool TriggerOnce = true;

        [Header("内置：相机速度")]
        [Tooltip("到达时是否修改 CameraRise 速度")]
        public bool SetCameraSpeed;

        [Tooltip("要设置的新速度")]
        public float CameraSpeed = 3f;

        [Header("自定义事件")]
        [Tooltip("到达时触发：可在此绑定打开 UI、播音效等")]
        public UnityEvent OnReached;

        [NonSerialized] public bool HasTriggered;
    }

    [Header("引用")]
    [Tooltip("用于读取高度的相机 Transform；为空则用 Camera.main")]
    [SerializeField] private Transform _cameraTransform;

    [Tooltip("可选：用于内置改速；为空则尝试从相机上获取")]
    [SerializeField] private CameraRise _cameraRise;

    [Header("高度判定")]
    [SerializeField] private HeightMode _heightMode = HeightMode.RiseFromStart;

    [Tooltip("按任意顺序配置即可，运行时会按高度排序检测")]
    [SerializeField] private List<HeightMilestone> _milestones = new List<HeightMilestone>();

    [Header("调试")]
    [SerializeField] private bool _logTriggers;

    private float _startY;
    private float _previousHeight = float.NegativeInfinity;
    private bool _initialized;
    private readonly List<int> _sortedIndices = new List<int>();

    /// <summary>相对起点已上升的高度</summary>
    public float RiseHeight =>
        _cameraTransform != null ? _cameraTransform.position.y - _startY : 0f;

    /// <summary>当前用于判定的高度值</summary>
    public float CurrentHeight => GetCurrentHeight();

    private void Awake()
    {
        if (_cameraTransform == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                _cameraTransform = cam.transform;
        }

        if (_cameraRise == null && _cameraTransform != null)
            _cameraRise = _cameraTransform.GetComponent<CameraRise>();

        RebuildSortedIndices();
    }

    private void Start()
    {
        if (_cameraTransform != null)
            _startY = _cameraTransform.position.y;

        _previousHeight = GetCurrentHeight();
        _initialized = true;
    }

    private void LateUpdate()
    {
        if (!_initialized || _cameraTransform == null)
            return;

        float height = GetCurrentHeight();

        // 高度上升时，检测是否跨过某个里程碑
        if (height > _previousHeight)
            EvaluateCrossedMilestones(_previousHeight, height);

        _previousHeight = height;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildSortedIndices();
    }
#endif

    /// <summary>重置所有里程碑（重开一局时调用）</summary>
    public void ResetMilestones()
    {
        for (int i = 0; i < _milestones.Count; i++)
        {
            if (_milestones[i] != null)
                _milestones[i].HasTriggered = false;
        }

        if (_cameraTransform != null)
            _startY = _cameraTransform.position.y;

        _previousHeight = GetCurrentHeight();
        RebuildSortedIndices();
    }

    /// <summary>供 UnityEvent 调用：设置相机上升速度</summary>
    public void SetCameraSpeed(float speed)
    {
        EnsureCameraRise();
        if (_cameraRise != null)
            _cameraRise.Speed = speed;
        else
            Debug.LogWarning("[DifficultyController] 未找到 CameraRise，无法改速。", this);
    }

    /// <summary>供 UnityEvent 调用：暂停相机上升</summary>
    public void PauseCamera()
    {
        EnsureCameraRise();
        if (_cameraRise != null)
            _cameraRise.Pause();
    }

    /// <summary>供 UnityEvent 调用：继续相机上升</summary>
    public void ResumeCamera()
    {
        EnsureCameraRise();
        if (_cameraRise != null)
            _cameraRise.Play();
    }

    private void EvaluateCrossedMilestones(float fromHeight, float toHeight)
    {
        for (int s = 0; s < _sortedIndices.Count; s++)
        {
            int index = _sortedIndices[s];
            if (index < 0 || index >= _milestones.Count)
                continue;

            HeightMilestone milestone = _milestones[index];
            if (milestone == null)
                continue;

            if (milestone.TriggerOnce && milestone.HasTriggered)
                continue;

            // 上一帧还没到，这一帧到了或超过
            if (fromHeight < milestone.Height && toHeight >= milestone.Height)
                TriggerMilestone(milestone);
        }
    }

    private void TriggerMilestone(HeightMilestone milestone)
    {
        milestone.HasTriggered = true;

        if (milestone.SetCameraSpeed)
            SetCameraSpeed(milestone.CameraSpeed);

        milestone.OnReached?.Invoke();

        if (_logTriggers)
            Debug.Log($"[DifficultyController] 触发里程碑「{milestone.Name}」@ {milestone.Height}", this);
    }

    private float GetCurrentHeight()
    {
        if (_cameraTransform == null)
            return 0f;

        return _heightMode == HeightMode.RiseFromStart
            ? _cameraTransform.position.y - _startY
            : _cameraTransform.position.y;
    }

    private void EnsureCameraRise()
    {
        if (_cameraRise == null && _cameraTransform != null)
            _cameraRise = _cameraTransform.GetComponent<CameraRise>();
    }

    private void RebuildSortedIndices()
    {
        _sortedIndices.Clear();
        if (_milestones == null)
            return;

        for (int i = 0; i < _milestones.Count; i++)
            _sortedIndices.Add(i);

        _sortedIndices.Sort((a, b) =>
        {
            float ha = _milestones[a] != null ? _milestones[a].Height : 0f;
            float hb = _milestones[b] != null ? _milestones[b].Height : 0f;
            return ha.CompareTo(hb);
        });
    }
}
