using System.Collections;
using TMPro;
using UnityEngine;

public class NumberfallAfterimage : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(KeepDecreasingAlpha());
    }

    private IEnumerator KeepDecreasingAlpha()
    {
        TextMeshPro textMesh = GetComponent<TextMeshPro>();
        Color col = textMesh.color;
        float alpha = col.a;

        while (alpha > 0f)
        {
            yield return new WaitForSeconds(0.1f);
            alpha -= 0.03f;
            textMesh.color = new Color(col.r, col.g, col.b, alpha);
        }

        Destroy(gameObject);
    }
}
