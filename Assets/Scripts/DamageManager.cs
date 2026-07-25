using System;
using UnityEngine;

public class DamageManager : MonoBehaviour
{
    public static DamageManager instance;

    public Action gettingDamage;

    private void Awake()
    {
        instance = this;
        gettingDamage += OnGetDamage;
    }
    
    public void OnGetDamage()
    {
    }
}
