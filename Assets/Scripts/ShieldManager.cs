using System.Collections.Generic;
using UnityEngine;

public enum ShieldLevel { Small, Middle, Big }

public class ShieldManager : MonoBehaviour
{
    public static ShieldManager instance;

    public GameObject[] shieldPrefabs;
    [HideInInspector]
    public List<Shield> shields;

    private void Awake()
    {
        instance = this;
    }

    public void AddShield(ShieldLevel shieldLevel)
    {
        shields.Add(Instantiate(shieldPrefabs[(int)shieldLevel], transform).GetComponent<Shield>());
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
