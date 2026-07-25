using UnityEngine;

public class NumberFallManager : MonoBehaviour
{
    public GameObject[] numberPrefabs = new GameObject[10]; // 数字预制体，下标 0~9 对应数字
    public Transform second2;                               // 秒位（十位）数字的生成位置
    public Timer timer;                                     // 计时器，从中读取剩余时间
    
    void Start()
    {
        // 从格式化时间(如 "1:23:456")中取出秒的个位数字
        int index = int.Parse(timer.countdown.FormattedRemainingTime[3].ToString());
        // 在 second2 位置生成对应数字，父对象设为 Manager
        Instantiate(numberPrefabs[index], second2.position, Quaternion.identity, transform);
    }
}