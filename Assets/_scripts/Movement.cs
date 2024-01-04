using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
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
    private float jump_start_time;

    [SerializeField]
    private float fall_multiplier = 2.5f;

    [SerializeField]
    private Transform ground_check;

    [SerializeField]
    private float ground_check_radius;

    [SerializeField]
    private LayerMask ground_layer;
    private float default_gravity_scale;
    private bool is_jumping = false;

    //Set this value larger than 1 for smoother transition

    [SerializeField]
    private float release_jump_vel_mod = 2f;

    [Header("Wall Settings")]
    [SerializeField]
    private Transform wall_check;

    [SerializeField]
    private float wall_check_radius;

    [SerializeField]
    private LayerMask wall_layer;

    [SerializeField]
    private float slide_speed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        default_gravity_scale = rb.gravityScale;
    }

    private void FixedUpdate()
    {
        wall_slide();
        HorizontalMovement();
        ApplyFriction();
    }

    private void Update()
    {
        Jump();
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

        rb.velocity = new Vector2(
            Mathf.Clamp(rb.velocity.x, -movement_speed, movement_speed),
            rb.velocity.y
        );
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
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
        {
            is_jumping = true;
            rb.AddForce(Vector2.up * jump_force, ForceMode2D.Impulse);
        }

        //Variable height jump
        //To stop the player from jumping early
        if (Input.GetKeyUp(KeyCode.Space) && is_jumping)
        {
            is_jumping = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / release_jump_vel_mod);
        }

        //Gravity Change
        if (rb.velocity.y < 0)
        {
            is_jumping = false;
            rb.gravityScale = default_gravity_scale * fall_multiplier;
        }
        else
        {
            rb.gravityScale = default_gravity_scale;
        }
    }

    private bool IsGrounded()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            ground_check.position,
            ground_check_radius,
            ground_layer
        );

        return hit != null;
    }

    // Wall climb

    private void wall_climb() { }

    private void wall_slide()
    {
        if (WallDetected())
        {
            rb.velocity = new Vector2(rb.velocity.x, -slide_speed);
        }
    }

    private bool WallDetected()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            wall_check.position,
            wall_check_radius,
            wall_layer
        );

        return hit != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_check.position, ground_check_radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wall_check.position, wall_check_radius);
    }
}
