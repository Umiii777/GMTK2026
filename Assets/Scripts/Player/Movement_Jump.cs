using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Jump : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float jumpHeight = 3f;
    [SerializeField, Range(0, 5)] private int maxAirJumps = 0;
    [SerializeField, Range(0f, 5f)] private float donwardMovementMultiplier = 3f;
    [SerializeField, Range(0f, 5f)] private float upwardMovementMultiplier = 1.7f;

    public float JumpHeight
    {
        get => jumpHeight;
        set => jumpHeight = Mathf.Max(0f, value);
    }

    public int MaxAirJumps
    {
        get => maxAirJumps;
        set => maxAirJumps = Mathf.Clamp(value, 0, 10);
    }

    public void AddAirJump(int amount = 1) => MaxAirJumps = maxAirJumps + amount;

    private Controller controller;
    private Rigidbody2D rb;
    private Ground ground;
    private Vector2 veclocity;

    private int jumpPhase;
    private float defaultGravityScale, jumpSpeed;

    private bool desiredJump, isGrounding;


    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<Ground>();
        controller = GetComponent<Controller>();

        defaultGravityScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        desiredJump |= controller.input.RetrieveJumpInput();
    }

    private void FixedUpdate()
    {
        isGrounding = ground.OnGround;
        veclocity = rb.velocity;

        if (isGrounding)
        {
            jumpPhase = 0;
        }

        if (desiredJump)
        {
            desiredJump = false;
            JumpAction();
        }

        if (rb.velocity.y > 0)
        {
            rb.gravityScale = upwardMovementMultiplier;
        }
        else if (rb.velocity.y < 0)
        {
            rb.gravityScale = donwardMovementMultiplier;
        }
        else if (rb.velocity.y == 0)
        {
            rb.gravityScale = defaultGravityScale;
        }

        rb.velocity = veclocity;
    }
    private void JumpAction()
    {
        if (isGrounding || jumpPhase < maxAirJumps)
        {
            jumpPhase += 1;

            jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * jumpHeight);

            if (veclocity.y > 0f)
            {
                jumpSpeed = Mathf.Max(jumpSpeed - veclocity.y, 0f);
            }
            else if (veclocity.y < 0f)
            {
                jumpSpeed += Mathf.Abs(rb.velocity.y);
            }
            veclocity.y += jumpSpeed;
        }
    }
}

