using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Player Movement")]
public class PlayerMovementStats : ScriptableObject
{

    // https://www.youtube.com/watch?v=zHSWG05byEc

    [Header("Walk")]
    [Range(1f, 100f)]
    public float MaxWalkSpeed = 12.5f;
    [Range(.25f, 50f)]
    public float GroundAcceleration = 5f;
    [Range(.25f, 50f)]
    public float GroundDeceleration = 20f;
    [Range(.25f, 50f)]
    public float AirAcceleration = 5f;
    [Range(.25f, 50f)]
    public float AirDeceleration = 20f;


    [Header("Run")]
    [Range(1f, 100f)]
    public float MaxRunSpeed = 20f;

    [Header("Grounded/Collision Checks")]
    public LayerMask GroundLayer;
    public float GroundDetectionRayLength = 0.02f;
    public float HeadDetectionRayLength = 0.2f;
    [Range(0f, 1f)]
    public float HeadWidth = 0.75f;


    [Header("Jump")]

    public float JumpHeight = 6.5f;
    [Range(1f, 1.1f)]
    public float JumpHeightComponsationFactor = 1.054f;
    public float TimeTillJumpApex = 0.35f;
    [Range(0.01f, 5f)]
    public float GravityOnReleaseMultiplier = 2f;
    public float MaxFallSpeed = 26f;
    [Range(1, 10)]
    public int NumberOfJumpAllowed = 2;


    [Header("Jump Cut")]
    [Range(0.02f, 0.3f)]
    public float TimeForUpwardsCancel = 0.027f;

    [Header("Jump Apex")]
    [Range(0.5f, 1f)]
    public float ApexThreshold = 0.97f;
    [Range(0.01f, 1f)]
    public float ApexHamgTime = 0.075f;


    [Header("Jump Buffer")]
    [Range(0f, 1f)]
    public float JumpBufferTime = 0.125f;

    [Header("Jump Coyote Time")]
    [Range(0f, 1f)]
    public float JumpCoyoteTime = 0.1f;


    [Header("Debug")]
    public bool DebugShowIsGroundedBox = false;
    public bool DebugShowHeadBumpBox = false;


    [Header("JumpVisualization Tool")]
    public bool ShowWalkJumpArc = false;
    public bool ShowRunJumpArc = false;
    public bool StopOnCollision = true;
    public bool DrawRight = true;
    [Range(5, 100)]
    public int ArcResolution = 20;
    [Range(5, 500)]
    public int VisualizationSteps = 90;


    public float Gravity { get; private set; }
    public float InitialJumpVelocity { get; private set; }

    public float AdjustedJumpHeight { get; private set; }


    public void OnEnable()
    {
        CalculateValues();
    }

    public void OnValidate()
    {
        CalculateValues();
    }


    private void CalculateValues()
    {
        AdjustedJumpHeight = JumpHeight * JumpHeightComponsationFactor;
        Gravity = (-2f * JumpHeight) / Mathf.Pow(TimeTillJumpApex, 2f);
        InitialJumpVelocity = Mathf.Abs(Gravity) * TimeTillJumpApex;
    }

}