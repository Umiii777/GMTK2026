using UnityEngine;

public class SpriteReversing : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.A))
            spriteRenderer.flipX = true;
        else
            spriteRenderer.flipX = false;
    }
}
