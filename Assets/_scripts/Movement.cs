using System;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [SerializeField]
    private PlayerData playerData;
    private Rigidbody2D rb;
    private float horizontal_movement;
    private bool is_facing_right = true;

    [SerializeField]
    private Transform ground_check;
    private float default_gravity_scale;
    private bool is_jumping = false;
    private float coyote_time_remaining;
    private bool can_coyote_jump = false;
    private float hang_timer;
    private bool hang_time_active = false;
    private bool can_move_horizontally = true;

    [SerializeField]
    private Transform wall_check;
    private bool wall_jumped = false;
    private bool is_climbing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        coyote_time_remaining = playerData.coyote_time;
        default_gravity_scale = rb.gravityScale;
        hang_timer = playerData.hang_time;
    }

    private void FixedUpdate()
    {
        WallSlide();
        WallClimb();
        WallJump();
        HorizontalMovement();
        ApplyFriction();
    }

    private void Update()
    {
        Jump();
        Falling();
        // HangTime();
        CoyoteTimer();
        FlipPlayer();
    }

    private void HorizontalMovement()
    {
        horizontal_movement = Input.GetAxisRaw("Horizontal");

        if (can_move_horizontally)
        {
            rb.AddForce(Vector2.right * (horizontal_movement * playerData.movement_speed));

            AccelerateMovement();
            DecelerateMovement();
        }

        LimitMovementSpeed();

        //Flip the player
        if (horizontal_movement > 0.1f)
        {
            is_facing_right = true;
        }

        if (horizontal_movement < -0.1f)
        {
            is_facing_right = false;
        }
    }

    private void AccelerateMovement()
    {
        Vector2 target_speed = new Vector2(
            horizontal_movement * playerData.movement_speed,
            rb.velocity.y
        );

        if (wall_jumped)
        {
            rb.velocity = Vector2.Lerp(rb.velocity, target_speed, .5f * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector2.Lerp(
                rb.velocity,
                target_speed,
                playerData.acceleration * Time.fixedDeltaTime
            );
        }
    }

    private void DecelerateMovement()
    {
        if (Mathf.Approximately(playerData.movement_speed, 0f))
        {
            Vector2 decelerationVector = new Vector2(
                -rb.velocity.x * playerData.deceleration,
                rb.velocity.y
            );
            rb.velocity += decelerationVector * Time.fixedDeltaTime;
        }
    }

    private void LimitMovementSpeed()
    {
        rb.velocity = new Vector2(
            Mathf.Clamp(rb.velocity.x, -playerData.movement_speed, playerData.movement_speed),
            rb.velocity.y
        );
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
            float amount = Mathf.Min(
                Mathf.Abs(rb.velocity.x),
                Mathf.Abs(playerData.friction_amount)
            );
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }
    }

    private void Jump()
    {
        if (!is_climbing)
        {
            if (
                Input.GetKeyDown(KeyCode.Space) && IsGrounded()
                || Input.GetKeyDown(KeyCode.Space) && can_coyote_jump
            )
            {
                can_coyote_jump = false;
                rb.gravityScale = default_gravity_scale;
                is_jumping = true;
                rb.AddForce(Vector2.up * playerData.jump_force, ForceMode2D.Impulse);
            }

            //Variable height jump
            //To stop the player from jumping early
            if (Input.GetKeyUp(KeyCode.Space) && is_jumping)
            {
                is_jumping = false;
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    rb.velocity.y / playerData.release_jump_vel_modifier
                );
            }
        }
    }

    private void Falling()
    {
        //Character is falling code
        if (rb.velocity.y < 0 && !hang_time_active)
        {
            wall_jumped = false;
            is_jumping = false;
            rb.gravityScale = default_gravity_scale * playerData.fall_multiplier;
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
            coyote_time_remaining = playerData.coyote_time;
            can_coyote_jump = true;
        }
    }

    // public void HangTime()
    // {
    //     if (Mathf.Max(rb.velocity.y, 0) == 0 && jumped)
    //     {
    //         if (hang_timer < 0f)
    //         {
    //             hang_time_active = false;
    //             hang_timer = hang_time;
    //             jumped = false;
    //         }
    //         else
    //         {
    //             hang_timer -= Time.deltaTime;
    //             hang_time_active = true;
    //             rb.velocity = new Vector2(rb.velocity.x, 0);
    //             rb.gravityScale = hang_time_gravity;
    //         }
    //     }
    // }

    private bool IsGrounded()
    {
        Collider2D hit = Physics2D.OverlapCircle(
            ground_check.position,
            playerData.ground_check_radius,
            playerData.ground_layer
        );

        return hit != null;
    }

    // Wall climb

    private void WallClimb()
    {
        if (Input.GetKeyDown(KeyCode.K) && WallDetected())
        {
            is_climbing = true;
            can_move_horizontally = false;
            rb.gravityScale = 0;
        }

        if (Input.GetKeyUp(KeyCode.K) && is_climbing)
        {
            is_climbing = false;
            rb.gravityScale = default_gravity_scale;
        }

        if (is_climbing)
            WallMovement();
    }

    private void WallMovement()
    {
        if (is_climbing)
        {
            rb.velocity = new Vector2(
                rb.velocity.x,
                Input.GetAxisRaw("Vertical") * playerData.wall_climb_speed
            );
        }
    }

    private void WallJump()
    {
        if ((is_climbing || WallDetected()) && Input.GetKeyDown(KeyCode.Space))
        {
            is_climbing = false;
            can_move_horizontally = true;
            wall_jumped = true;
            is_facing_right = !is_facing_right;
            rb.gravityScale = default_gravity_scale;
            rb.AddForce(new Vector2(1f, 2f) * playerData.wall_jump_force, ForceMode2D.Impulse);
        }
    }

    private void WallSlide()
    {
        if (WallDetected() && !is_climbing && !IsGrounded())
        {
            rb.gravityScale = 0;
            rb.velocity = new Vector2(rb.velocity.x, -playerData.slide_speed);
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
            playerData.wall_check_radius,
            playerData.wall_layer
        );

        return hit != null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(ground_check.position, playerData.ground_check_radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(wall_check.position, playerData.wall_check_radius);
    }
}
