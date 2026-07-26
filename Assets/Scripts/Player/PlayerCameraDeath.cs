using UnityEngine;

/// <summary>
/// 玩家超出摄像机可视范围（可加边距）时，触发一次 DamageManager.gettingDamage。
/// 挂到玩家身上。
/// </summary>
public class PlayerCameraDeath : MonoBehaviour
{
    [Header("引用")]
    [Tooltip("用于判定范围的摄像机；为空则用 Camera.main")]
    [SerializeField] private Camera _camera;

    [Header("死亡边界")]
    [Tooltip("超出屏幕下沿多少后判定死亡（最常见）")]
    [SerializeField] private float _belowMargin = 1f;

    [Tooltip("超出屏幕上沿多少后判定死亡；填负数表示不检测上方")]
    [SerializeField] private float _aboveMargin = -1f;

    [Tooltip("超出屏幕左右多少后判定死亡；填负数表示不检测左右")]
    [SerializeField] private float _horizontalMargin = -1f;

    [Tooltip("暂停时不检测")]
    [SerializeField] private bool _ignoreWhenPaused = true;

    private bool _triggered;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_triggered)
            return;

        if (_ignoreWhenPaused && PauseManager.IsPaused)
            return;

        if (_camera == null)
        {
            _camera = Camera.main;
            if (_camera == null)
                return;
        }

        if (!IsOutOfCameraBounds())
            return;

        _triggered = true;

        if (DamageManager.instance != null && DamageManager.instance.gettingDamage != null)
            DamageManager.instance.gettingDamage();
        else
            Debug.LogWarning("[PlayerCameraDeath] DamageManager.instance 或 gettingDamage 为空。", this);
    }

    private bool IsOutOfCameraBounds()
    {
        Vector3 pos = transform.position;
        GetCameraBounds(out float left, out float right, out float bottom, out float top);

        if (pos.y < bottom - _belowMargin)
            return true;

        if (_aboveMargin >= 0f && pos.y > top + _aboveMargin)
            return true;

        if (_horizontalMargin >= 0f)
        {
            if (pos.x < left - _horizontalMargin)
                return true;
            if (pos.x > right + _horizontalMargin)
                return true;
        }

        return false;
    }

    private void GetCameraBounds(out float left, out float right, out float bottom, out float top)
    {
        float halfH;
        float halfW;

        if (_camera.orthographic)
        {
            halfH = _camera.orthographicSize;
            halfW = halfH * _camera.aspect;
        }
        else
        {
            float distance = Mathf.Abs(_camera.transform.position.z - transform.position.z);
            halfH = distance * Mathf.Tan(_camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            halfW = halfH * _camera.aspect;
        }

        Vector3 camPos = _camera.transform.position;
        left = camPos.x - halfW;
        right = camPos.x + halfW;
        bottom = camPos.y - halfH;
        top = camPos.y + halfH;
    }

    /// <summary>重生等情况下允许再次触发</summary>
    public void ResetTrigger()
    {
        _triggered = false;
    }
}
