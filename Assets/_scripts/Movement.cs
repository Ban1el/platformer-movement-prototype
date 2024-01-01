using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rb;

    [Header("Movement Settings")]
    [SerializeField]
    private float movement_speed = 7f;

    [SerializeField]
    private float acceleration;

    [SerializeField]
    private float deceleration;

    [SerializeField]
    private float friction_amount;
    private float horizontal_movement;

    [Header("Jump Settings")]
    [SerializeField]
    private float jump_force;

    [SerializeField]
    private float fall_multiplier = 2.5f;

    [SerializeField]
    private float low_jump_multiplier = 2f;

    [SerializeField]
    private Transform ground_check;

    [SerializeField]
    private float ground_check_radius;

    [SerializeField]
    private LayerMask ground_layer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        HorizontalMovement();
        ApplyFriction();
    }

    private void Update()
    {
        Jump();
        GravityChange();
    }

    private void HorizontalMovement()
    {
        horizontal_movement = Input.GetAxisRaw("Horizontal");
        rb.AddForce(Vector2.right * (horizontal_movement * movement_speed));
        Vector2 target_speed = new Vector2(horizontal_movement * movement_speed, rb.velocity.y);
        rb.velocity = Vector2.Lerp(rb.velocity, target_speed, acceleration * Time.fixedDeltaTime);

        if (Mathf.Approximately(movement_speed, 0f))
        {
            Vector2 decelerationVector = new Vector2(-rb.velocity.x * deceleration, rb.velocity.y);
            rb.velocity += decelerationVector * Time.fixedDeltaTime;
        }

        rb.velocity = Vector2.ClampMagnitude(rb.velocity, movement_speed);
    }

    private void ApplyFriction()
    {
        if (Mathf.Abs(horizontal_movement) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(friction_amount));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    private void Jump()
    {
        if (Input.GetKey(KeyCode.Space) && IsGrounded())
        {
            rb.AddForce(Vector2.up * jump_force, ForceMode2D.Impulse);
        }

        // //If player is falling
        // if (rb.velocity.y < 0)
        // {
        //     //The reason why we need to subtract to 1, because Unity already applying 1 multiple of the gravity.
        //     // Multiplied by Time.deltaTime because the gravtiy is per second. We want to apply it, per frame.
        //     rb.velocity +=
        //         Vector2.up * Physics2D.gravity.y * (fall_multiplier - 1) * Time.deltaTime;
        // }
        // else if (rb.velocity.y > 0 && Input.GetKeyDown(KeyCode.Space))
        // {
        //     rb.velocity +=
        //         Vector2.up * Physics2D.gravity.y * (low_jump_multiplier - 1) * Time.deltaTime;
        // }
    }

    private void GravityChange()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = 1 * fall_multiplier;
        }
        else
        {
            rb.gravityScale = 1;
        }
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.CircleCast(
            ground_check.position,
            ground_check_radius,
            Vector2.zero,
            0f,
            ground_layer
        );

        return hit;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_check.position, ground_check_radius);
    }
}
