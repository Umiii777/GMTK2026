using TMPro;
using UnityEngine;

/// <summary>
/// 监听 Coin.Collected，把当前金币数显示到 TextMeshPro。
/// 挂到任意物体上，把场景里的 TMP 拖到 Coin Text 即可。
/// </summary>
public class CoinUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _coinText;

    [Tooltip("显示格式；{0} 会被替换成当前金币数")]
    [SerializeField] private string _format = "{0}";

    private int _count;

    public int Count => _count;

    private void Awake()
    {
        if (_coinText == null)
            _coinText = GetComponent<TMP_Text>();

        Refresh();
    }

    private void OnEnable()
    {
        Coin.Collected += OnCoinCollected;
    }

    private void OnDisable()
    {
        Coin.Collected -= OnCoinCollected;
    }

    private void OnCoinCollected(int value)
    {
        _count += value;
        Refresh();
    }

    public void ResetCount()
    {
        _count = 0;
        Refresh();
    }

    private void Refresh()
    {
        if (_coinText != null)
            _coinText.text = string.Format(_format, _count);
    }
}
