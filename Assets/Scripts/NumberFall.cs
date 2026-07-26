using System.Collections;
using UnityEngine;

public class NumberFall : MonoBehaviour
{
    public static int deadHeight = -8;

    private void Start()
    {
        StartCoroutine(WaitForSelfDestroying());
        GetComponent<AfterimageSpawner>().StartSpawnAfterimage(3, transform.localScale.x > 0.99f ? 0.6f : 0.33f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player" && collision.transform.position.y > transform.position.y)
            DamageManager.instance.gettingDamage();
    }

    private IEnumerator WaitForSelfDestroying()
    {
        while (transform.position.y > deadHeight)
            yield return new WaitForSeconds(5f);

        Destroy(gameObject);
    }
}
