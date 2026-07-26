using System.Collections;
using UnityEngine;

public class Shield : MonoBehaviour
{
    public float speed = 8f;

    private void Start()
    {
        StartCoroutine(KeepSpinning());
    }

    private IEnumerator KeepSpinning()
    {
        Collider2D[] colliders = GetComponentsInChildren<Collider2D>();

        while (true)
        {
            transform.RotateAround(transform.parent.position, Vector3.forward, speed);

            foreach (var c in colliders)
            {
                if (c.transform.position.y - transform.parent.position.y < 0.2f && !c.isTrigger)
                    c.isTrigger = true;
                else if (c.transform.position.y - transform.parent.position.y >= 0.2f && c.isTrigger)
                    c.isTrigger = false;
            }

            yield return new WaitForSeconds(0.03f);
        }
    }
}
