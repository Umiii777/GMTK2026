using UnityEngine;


[System.Serializable]
public struct Timer
{
    // ---- 配置 ----
    /// <summary>计时总时长（秒）</summary>
    public float Duration;

    /// <summary>已经过的时间（秒），由 Tick() 驱动累加</summary>
    public float Elapsed;

    // ---- 运行时状态 ----
    /// <summary>是否正在计时</summary>
    public bool IsRunning { get; private set; }

    /// <summary>计时是否已结束（Elapsed >= Duration）</summary>
    public bool IsFinished => Elapsed >= Duration;

    /// <summary>进度百分比 [0, 1]；Duration 为 0 时直接返回 1</summary>
    public float Progress => Duration > 0f ? Mathf.Clamp01(Elapsed / Duration) : 1f;

    /// <summary>剩余时间（秒），最小返回 0</summary>
    public float Remaining => Mathf.Max(0f, Duration - Elapsed);

    // ============================================================
    //  构造函数
    // ============================================================

    /// <summary>
    /// 创建一个新计时器，默认不启动，需要手动调用 Start()
    /// </summary>
    /// <param name="duration">计时总时长，单位：秒</param>
    public Timer(float duration)
    {
        Duration = duration;
        Elapsed = 0f;
        IsRunning = false;
    }

    // ============================================================
    //  控制方法
    // ============================================================

    /// <summary>
    /// 启动计时器。会将 Elapsed 重置为 0，适合复用场景
    /// </summary>
    public void Start()
    {
        Elapsed = 0f;
        IsRunning = true;
    }

    /// <summary>
    /// 暂停计时器。再次调用 Start() 可从头开始，或直接改 IsRunning = true 从当前位置继续
    /// </summary>
    public void Stop() => IsRunning = false;

    /// <summary>
    /// 每帧调用一次，推动计时器前进。通常传入 Time.deltaTime 或 Time.unscaledDeltaTime
    /// </summary>
    /// <param name="deltaTime">本帧的时间增量</param>
    /// <returns>true 表示计时器在本帧刚好走完；false 表示仍在计时或未启动</returns>
    public bool Tick(float deltaTime)
    {
        if (!IsRunning)
            return false;

        Elapsed += deltaTime;

        // 达到或超过目标时长 — 钳制 Elapsed 防止溢出，并自动停止
        if (Elapsed >= Duration)
        {
            Elapsed = Duration;
            IsRunning = false;
            return true;
        }

        return false;
    }

    /// <summary>
    /// 完全重置计时器：归零计时且停止运行
    /// </summary>
    public void Reset()
    {
        Elapsed = 0f;
        IsRunning = false;
    }
}