using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData")]
public class PlayerData : ScriptableObject
{
    [Header("Movement Settings")]
    public float movement_speed = 9f;
    public float acceleration = 13f;
    public float deceleration = 16f;
    public float friction_amount = 0.22f;

    [Header("Jump Settings")]
    public float jump_force = 13f;
    public float jump_start_time = 0.25f;
    public float fall_multiplier = 3f;
    public float ground_check_radius = 0.1f;
    public LayerMask ground_layer;
    public float coyote_time = 0.1f;
    public float hang_time_gravity = 0.1f;
    public float hang_time = 0.5f;

    //Set this value larger than 1 for smoother transition
    public float release_jump_vel_modifier = 2f;

    [Header("Wall Settings")]
    public LayerMask wall_layer;
    public float wall_check_radius;
    public float wall_jump_force = 8f;
    public float wall_climb_speed = 5f;
    public float slide_speed = 5f;
}
