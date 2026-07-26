using UnityEngine;

public class Movement_Dash : MonoBehaviour
{
    [SerializeField, Range(0f, 50f)] private float dashSpeed = 14f;
    [SerializeField, Range(0.05f, 1f)] private float dashDuration = 0.15f;
    [SerializeField, Range(0f, 5f)] private float dashCooldown = 0.35f;
    [Tooltip("冲刺时是否锁定垂直速度为 0")]
    [SerializeField] private bool freezeVertical = true;

    private Controller controller;
    private Rigidbody2D rb;
    private Vector2 velocity;

    private float facingDirection = 1f;
    private float dashTimeRemaining;
    private float cooldownRemaining;
    private bool desiredDash;

    public bool IsDashing { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<Controller>();
    }

    private void Update()
    {
        float moveInput = controller.input.RetrieveMoveInput();
        if (Mathf.Abs(moveInput) > 0.01f)
            facingDirection = Mathf.Sign(moveInput);

        desiredDash |= controller.input.RetrieveDashInput();
    }

    private void FixedUpdate()
    {
        if (cooldownRemaining > 0f)
            cooldownRemaining -= Time.deltaTime;

        if (IsDashing)
        {
            ApplyDashVelocity();
            dashTimeRemaining -= Time.deltaTime;
            if (dashTimeRemaining <= 0f)
                EndDash();
            return;
        }

        if (desiredDash)
        {
            desiredDash = false;
            TryStartDash();
        }
    }

    private void TryStartDash()
    {
        if (cooldownRemaining > 0f)
            return;

        IsDashing = true;
        dashTimeRemaining = dashDuration;
        ApplyDashVelocity();
    }

    private void ApplyDashVelocity()
    {
        velocity = rb.velocity;
        velocity.x = facingDirection * dashSpeed;
        if (freezeVertical)
            velocity.y = 0f;
        rb.velocity = velocity;
    }

    private void EndDash()
    {
        IsDashing = false;
        dashTimeRemaining = 0f;
        cooldownRemaining = dashCooldown;
    }
}
