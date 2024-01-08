using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
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
    private bool is_facing_right = true;

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

    [SerializeField]
    private float coyote_time = 1f;
    private float coyote_time_remaining;
    private bool can_coyote_jump = false;
    private float hang_time = 1f;

    //Set this value larger than 1 for smoother transition

    [SerializeField]
    private float release_jump_vel_modifier = 2f;

    [Header("Wall Settings")]
    [SerializeField]
    private Transform wall_check;

    [SerializeField]
    private float wall_check_radius;

    [SerializeField]
    private LayerMask wall_layer;

    [SerializeField]
    private float wall_climb_speed = 3f;

    [SerializeField]
    private float slide_speed;

    private bool is_climbing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        coyote_time_remaining = coyote_time;
        default_gravity_scale = rb.gravityScale;
    }

    private void FixedUpdate()
    {
        wall_slide();
        wall_climb();
        HorizontalMovement();
        ApplyFriction();
    }

    private void Update()
    {
        Jump();
        CoyoteTimer();
        HangTime();
        FlipPlayer();
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

        if (horizontal_movement > 0.1f)
        {
            is_facing_right = true;
        }

        if (horizontal_movement < -0.1f)
        {
            is_facing_right = false;
        }
    }

    private void FlipPlayer()
    {
        if (is_facing_right)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, -180f, 0f);
        }
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
        if (
            Input.GetKeyDown(KeyCode.Space) && IsGrounded()
            || Input.GetKeyDown(KeyCode.Space) && can_coyote_jump
        )
        {
            can_coyote_jump = false;
            rb.gravityScale = default_gravity_scale;
            is_jumping = true;
            rb.AddForce(Vector2.up * jump_force, ForceMode2D.Impulse);
        }

        //Variable height jump
        //To stop the player from jumping early
        if (Input.GetKeyUp(KeyCode.Space) && is_jumping)
        {
            is_jumping = false;
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / release_jump_vel_modifier);
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

    private void CoyoteTimer()
    {
        if (!IsGrounded() && Math.Abs(rb.velocity.y) > 0f)
        {
            coyote_time_remaining -= Time.deltaTime;
        }

        if (!IsGrounded())
        {
            if (coyote_time_remaining <= 0f)
                can_coyote_jump = false;
        }
        else if (IsGrounded() && !is_jumping)
        {
            coyote_time_remaining = coyote_time;
            can_coyote_jump = true;
        }
    }

    public void HangTime()
    {
        if (is_jumping && rb.velocity.y <= 0)
        {
            Debug.Log("start hangtime");
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

    private void wall_climb()
    {
        if (Input.GetKeyDown(KeyCode.K) && WallDetected())
        {
            is_climbing = true;
        }

        if (Input.GetKey(KeyCode.K) && WallDetected())
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(
                rb.velocity.x,
                Input.GetAxisRaw("Vertical") * wall_climb_speed
            );
        }

        if (Input.GetKeyUp(KeyCode.K))
        {
            is_climbing = false;
            rb.gravityScale = default_gravity_scale;
        }
    }

    private void wall_slide()
    {
        if (WallDetected() && !is_climbing)
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(rb.velocity.x, -slide_speed);
        }

        if (!WallDetected() && !is_climbing)
        {
            rb.gravityScale = default_gravity_scale;
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
