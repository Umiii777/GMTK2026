using System;
using UnityEngine;

public sealed class TempWarning
{
    static public object Log(string info = null)
    {
        Debug.LogWarning("temp code" + (null == info ? null : ": " + info));
        return null;
    }

    static public void Invoke(Action action)
    {
        action.Invoke();
    }

    static public void InvokeDebug(Action action)
    {
#if DEBUG
        action.Invoke();
#endif
    }
}