using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement_Move : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float maxSpeed = 4f;
    [SerializeField, Range(0f, 100f)] private float maxAcceleration = 35f;
    [SerializeField, Range(0f, 100f)] private float maxAirAcceleration = 20f;

    private Controller controller;
    private Vector2 direction, desiredVelocity, velocity;
    private Rigidbody2D rb;
    private Ground ground;

    private float maxSpeedChange, accleration;
    private bool isGrounding;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        ground = GetComponent<Ground>();
        controller = GetComponent<Controller>();
    }

    private void Update()
    {
        direction.x = controller.input.RetrieveMoveInput();
        desiredVelocity = new Vector2(direction.x, 0f) * Mathf.Max(maxSpeed - ground.Friction, 0f);
    }

    private void FixedUpdate()
    {
        isGrounding = ground.OnGround;
        velocity = rb.velocity;

        accleration = isGrounding ? maxAcceleration : maxAirAcceleration;
        maxSpeedChange = accleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);

        rb.velocity = velocity;
    }
}

