using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerMovementProperties", menuName = "SGN/PlayerMovementProperties", order = 5)]
public class PlayerMovementProperties : ScriptableObject
{
    [Header("Movement")]

    /// <summary>
    /// The amount of force applied to the player when they jump.
    /// </summary>
    public float JumpVelocity = 10f;

    /// <summary>
    /// The worlds gravity
    /// TODO: test with 9.81f and higher jump velocity.
    /// </summary>
    public float Gravity = 10f;

    /// <summary>
    /// Fudge factor for predicting landings
    /// </summary>
    public float FFactor = 7.5f;

    /// <summary>
    /// Layer mask for obstacles.
    /// </summary>
    public LayerMask ObstacleMask;

    /// <summary>
    /// The amount by which the player's speed is changed when they move.
    /// </summary>
    public float Acceleration = 2f;

    /// <summary>
    /// The amount by which the player's speed is changed when they stop moving.
    /// </summary>
    public float Friction = 1f;

    /// <summary>
    /// The maximum speed at which the player can move.
    /// </summary>
    public float MaxSpeed = 5f;

    /// <summary>
    /// The maximum speed at which the player can move while in the air.
    /// </summary>
    //public float MaxAirborneSpeed = 10f;

    /// <summary>
    /// The amount by which the player's speed is changed when they are in sprint mode.
    /// </summary>
    public float SprintMultiplier = 2f;

    /// <summary>
    /// The amount by which the player's speed is changed when they are in shoot mode.
    /// </summary>
    public float ShootMultiplier = 0.75f;

    /// <summary>
    /// The amount by which the player's speed is changed when they are in slide mode.
    /// </summary>
    public float SlideMultiplier = 1f;

    /// <summary>
    /// The maximum angle the player can rotate at.
    public float MaxRotationDegrees = 180f;

    [Header("Jumping")]

    /// <summary>
    /// The amount of time the player has to jump for before being able to be grounded again.
    /// </summary>
    public float MinimumJumpTime = 0.1f;

    [Header("Grounded")]

    /// <summary>
    /// The height above the ground at which the player is considered to be grounded.
    /// </summary>
    public float GroundedHeight = 1.1f;

    /// <summary>
    /// The length of the ray used to override player angle checks.
    /// </summary>
    public float OverrideRayLength = 0;

}
