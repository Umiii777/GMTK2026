using System.Collections;
using TMPro;
using UnityEngine;

public class AfterimageSpawner : MonoBehaviour
{
    public GameObject afterimagePrefab;

    public void StartSpawnAfterimage(int count, float intervalSeconds)
    {
        StartCoroutine(SpawnAfterimage(count, intervalSeconds));
    }

    private IEnumerator SpawnAfterimage(int count, float intervalSeconds)
    {
        int c = count;

        while (c > 0)
        {
            yield return new WaitForSeconds(intervalSeconds);

            GameObject afterimage = Instantiate(afterimagePrefab, transform.position, Quaternion.identity, transform.parent.GetChild(0));
            afterimage.transform.localScale = transform.localScale;

            TextMeshPro afterimageMesh = afterimage.GetComponent<TextMeshPro>();
            Color col = afterimageMesh.color;
            afterimageMesh.color = new Color(col.r, col.g, col.b, 0.2f);

            c--;
        }
    }
}
