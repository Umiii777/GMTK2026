using UnityEngine;

public class Movement_Scale : MonoBehaviour
{
    [Tooltip("缩小后的缩放倍率（相对原始 scale）")]
    [SerializeField, Range(0.1f, 1f)] private float shrinkMultiplier = 0.5f;

    [Tooltip("切换缩放的冷却时间")]
    [SerializeField, Range(0f, 2f)] private float toggleCooldown = 0.1f;

    private Controller controller;
    private Vector3 originalScale;
    private bool isShrunk;
    private bool desiredToggle;
    private float cooldownRemaining;

    public bool IsShrunk => isShrunk;

    private void Awake()
    {
        controller = GetComponent<Controller>();
        originalScale = transform.localScale;
    }

    private void Update()
    {
        desiredToggle |= controller.input.RetrieveScaleInput();
    }

    private void FixedUpdate()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;

        if (!desiredToggle)
            return;

        desiredToggle = false;
        TryToggleScale();
    }

    private void TryToggleScale()
    {
        if (cooldownRemaining > 0f)
            return;

        isShrunk = !isShrunk;
        transform.localScale = isShrunk
            ? originalScale * shrinkMultiplier
            : originalScale;

        cooldownRemaining = toggleCooldown;
    }
}
