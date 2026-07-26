using System;
using UnityEngine;

/// <summary>
/// 倒计时器：从创建时刻开始，对指定时长进行倒计时
/// </summary>
public class Countdown
{
    /// <summary>
    /// 计时开始时刻（游戏启动以来的真实秒数）
    /// </summary>
    private readonly float _startTime;

    /// <summary>
    /// 倒计时总时长（秒）
    /// </summary>
    private readonly float _duration;

    /// <summary>
    /// 创建计时器并立即开始倒计时
    /// </summary>
    /// <param name="duration">倒计时总时长（秒）</param>
    public Countdown(float duration)
    {
        _duration = duration;
        _startTime = Time.realtimeSinceStartup;
    }

    /// <summary>
    /// 已经过的秒数
    /// </summary>
    public float ElapsedSeconds => Time.realtimeSinceStartup - _startTime;

    /// <summary>
    /// 剩余秒数（不会小于 0）
    /// </summary>
    public float RemainingSeconds => Mathf.Max(0f, _duration - ElapsedSeconds);

    /// <summary>
    /// 倒计时是否结束
    /// </summary>
    public bool IsFinished => RemainingSeconds <= 0f;

    /// <summary>
    /// 剩余时间的显示文本，格式：MssSSS
    /// </summary>
    public string FormattedRemainingTime
    {
        get
        {
            TimeSpan ts = TimeSpan.FromSeconds(RemainingSeconds);
            return string.Format("{0}{1:00}{2:000}",(int)ts.TotalMinutes, ts.Seconds, ts.Milliseconds);
        }
    }
}