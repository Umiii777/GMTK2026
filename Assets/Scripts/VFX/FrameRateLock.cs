using UnityEngine;

public class FrameRateLock : MonoBehaviour
{
    [Range(1, 144)] public ushort targetFrameRate = 16;

    void OnEnable()
    {
        LockFrameRate(targetFrameRate);
    }

    static public void LockFrameRate(ushort targetFrameRate = 16)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFrameRate;
    }

    static public void UnlockFrameRate()
    {
        Application.targetFrameRate = -1;
    }

    void OnDisable()
    {
        Application.targetFrameRate = -1;
    }
}
