using System.Collections.Generic;
using UnityEngine;

public enum ShieldLevel { Small, Middle, Big }

public class ShieldManager : MonoBehaviour
{
    public static ShieldManager instance;

    public GameObject[] shieldPrefabs;
    [HideInInspector]
    public List<Shield> shields = new List<Shield>();

    private void Awake()
    {
        instance = this;
        if (shields == null)
            shields = new List<Shield>();
    }

    public void AddShield(ShieldLevel shieldLevel)
    {
        if (shieldPrefabs == null || shieldPrefabs.Length == 0)
        {
            Debug.LogWarning("[ShieldManager] 未配置 shieldPrefabs。", this);
            return;
        }

        int index = Mathf.Clamp((int)shieldLevel, 0, shieldPrefabs.Length - 1);
        if (shieldPrefabs[index] == null)
        {
            Debug.LogWarning($"[ShieldManager] shieldPrefabs[{index}] 为空。", this);
            return;
        }

        shields.Add(Instantiate(shieldPrefabs[index], transform).GetComponent<Shield>());
    }

    /// <summary>按已有层数递进添加：无→小，有小→中，有中→大；已有大则再加一个大</summary>
    public void AddNextShieldUpgrade()
    {
        if (shields == null || shields.Count == 0)
            AddShield(ShieldLevel.Small);
        else if (shields.Count == 1)
            AddShield(ShieldLevel.Middle);
        else
            AddShield(ShieldLevel.Big);
    }

    public void RemoveAllShields()
    {
        foreach (var s in shields)
            Destroy(s.gameObject);
    }

    public void RemoveShield(uint index)
    {
        Destroy(shields[(int)index].gameObject);
    }
}
