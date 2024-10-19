using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    [Header("References")]
    public PlayerMovementStats MoveStats;
    [SerializeField]
    private Collider2D _feetColl;
    [SerializeField]
    private Collider2D _bodyColl;

    private Rigidbody2D _rb;

    private Vector2 _moveVelocity;
    private bool _isFacingRight;

    private RaycastHit2D _groundHit;
    private RaycastHit2D _headHit;
    private bool _isGrounded;
    private bool _bumpedHead;


    // jump vars
    public float VerticcalVelocity { get; private set; }
    private bool _isJumping;
    private bool _isFastFalling;
    private bool _isFalling;
    private float _fastFallTime;
    private float _fastFallReleaseSpeed;
    private float _numberOfJumpUsed;

    // apex vars
    private float _apexPoint;
    private float _timePastApexThreshold;
    private bool _isPastApexThreshold;


    // jump buffer vars
    private float _JumpBufferTimer;
    private bool _JumpReleaseDuringBuffer;

    // coyote time vars

    private float _coyoteTimer;


    private void Awake()
    {
        _isFacingRight = true;
        _rb = GetComponent<Rigidbody2D>();
    }

    #region Movement

    private void Move(float acceleration, float deceleration, Vector2 moveInput)
    {
        if (moveInput != Vector2.zero) { 

            TurnCheck(moveInput);

            Vector2 targetVelocity = Vector2.zero;
            if (InputManager.RunIsHeld)
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxRunSpeed;
            } else
            {
                targetVelocity = new Vector2(moveInput.x, 0f) * MoveStats.MaxWalkSpeed;
            }

            _moveVelocity = Vector2.Lerp(_moveVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);

        }

        else if(moveInput == Vector2.zero)
        {
            _moveVelocity = Vector2.Lerp(_moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            _rb.velocity = new Vector2(_moveVelocity.x, _rb.velocity.y);

        }

    }

    private void TurnCheck(Vector2 moveInput)
    {
        if (_isFacingRight && moveInput.x < 0)
        {
            Turn(false);
        } else if (!_isFacingRight && moveInput.x > 0)
        {
            Turn(true);
        }
    }

    private void Turn(bool turnRight)
    {
        _isFacingRight = turnRight;
        if (turnRight)
        {
            transform.Rotate(0f, 180f, 0f);
        } else
        {
            transform.Rotate(0f, -180f, 0f);
        }
    }


    #endregion

    #region Collision checks


    private void IsGrounded()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.center.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x, _feetColl.bounds.size.y);

        _groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down, MoveStats.GroundDetectionRayLength, MoveStats.GroundLayer);

        if(_groundHit.collider != null)
        {
            _isGrounded = true;
        } else
        {
            _isGrounded = false;
        }

        #region Debug Visualization

        if(MoveStats.DebugShowIsGroundedBox)
        {
            Color rayColor;
            if(_isGrounded)
            {
                rayColor = Color.green;
            }
            else
            {
                rayColor = Color.red;
            }
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y), Vector2.down * MoveStats.GroundDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y - MoveStats.GroundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
        }


        #endregion

    }

    private void BumpedHead()
    {
        Vector2 boxCastOrigin = new Vector2(_feetColl.bounds.center.x, _bodyColl.bounds.max.y);
        Vector2 boxCastSize = new Vector2(_feetColl.bounds.size.x * MoveStats.HeadWidth, MoveStats.HeadDetectionRayLength);

        _headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, MoveStats.HeadDetectionRayLength, MoveStats.GroundLayer);
        if (_headHit.collider != null)
        {
            _bumpedHead = true;
        }
        else { 
            _bumpedHead = false; 
        }

        #region Debug Visualization

        if (MoveStats.DebugShowHeadBumpBox)
        {
            float headWidth = MoveStats.HeadWidth;

            Color rayColor;
            if (_bumpedHead)
            {
                rayColor = Color.green;
            }
            else { rayColor = Color.red; }

            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * MoveStats.HeadDetectionRayLength, rayColor);
            Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y + MoveStats.HeadDetectionRayLength), Vector2.right * boxCastSize.x * headWidth, rayColor);

        }
        #endregion

    }

    private void CollisionChecks()
    {
        IsGrounded();
        BumpedHead();
    }
    #endregion


    private void JumpCheck()
    {
        if (InputManager.JumpWasPressed)
        {
            _JumpBufferTimer = MoveStats.JumpBufferTime;
            _JumpReleaseDuringBuffer = false;
        }
        
        // When we release the jump button
        if (InputManager.JumpWasRelease)
        {
            if (_JumpBufferTimer > 0f)
            {
                _JumpReleaseDuringBuffer = true;
            }

            if (_isJumping && VerticcalVelocity > 0f )
            {
                if (_isPastApexThreshold)
                { 
                    _isPastApexThreshold = false;
                    _isFastFalling = true;
                    _fastFallTime = MoveStats.TimeForUpwardsCancel;
                    VerticcalVelocity = 0f;
                }
                else
                {
                    _isFastFalling = true;
                    _fastFallReleaseSpeed = VerticcalVelocity;
                }

            }
        }

        // initiate jump
        if ( _JumpBufferTimer > 0f && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if(_JumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticcalVelocity;
            }
        }

        // double jump
        else if (_JumpBufferTimer > 0f && _isJumping && _numberOfJumpUsed < MoveStats.NumberOfJumpAllowed - 1)
        {
            _isFastFalling = false;
            InitiateJump(1);
        }

        // air jump
        else if (_JumpBufferTimer > 0f && _isFalling && _numberOfJumpUsed < MoveStats.NumberOfJumpAllowed)
        {
            InitiateJump(2);
            _isFastFalling = false;
        }

        // landed
        if ((_isJumping || _isFalling) && _isGrounded && VerticcalVelocity <= 0)
        {
            _isJumping = false;
            _isFalling = false;
            _isFastFalling = false;
            _fastFallTime = 0f;
            _isPastApexThreshold = false;

            //VerticcalVelocity = Physics2D.gravity.y;
        }


    }

    private void InitiateJump(int numberOfJumpUsed)
    {
        if (!_isJumping)
        {
            _isJumping = true;
        }

        _JumpBufferTimer = 0f;
        _numberOfJumpUsed += numberOfJumpUsed;
        VerticcalVelocity = MoveStats.InitialJumpVelocity;


    }

    private void Jump()
    {

        if(_isJumping)
        {
            if(_bumpedHead)
            {
                _isFastFalling = true;
            }
        }

        if (VerticcalVelocity >= 0f)
        {
            _apexPoint = Mathf.InverseLerp(MoveStats.InitialJumpVelocity, 0f, VerticcalVelocity);

            if (_apexPoint > MoveStats.ApexThreshold && ! _isPastApexThreshold)
            {
                _isPastApexThreshold = true;
                _timePastApexThreshold = 0f;

            }


            if( _isPastApexThreshold)
            {
                _timePastApexThreshold += Time.fixedDeltaTime;
                if (_timePastApexThreshold < MoveStats.ApexHangTime)
                {
                    VerticcalVelocity = 0f;
                }
                else
                {
                    VerticcalVelocity = -0.5f;
                    _isFastFalling = true;
                }
            }
            else
            {
                VerticcalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;

                if(_isPastApexThreshold)
                {
                    _isPastApexThreshold = false;
                }

            }

        }
        else if (!_isFastFalling)
        {
            VerticcalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
        }
        else if (VerticcalVelocity < 0f)
        {
            if(!_isFastFalling)
            {
                _isFastFalling = true;
            }
        }


        // jump cut
        if ( _isFastFalling)
        {
            if (_fastFallTime >= MoveStats.TimeForUpwardsCancel)
            {
                VerticcalVelocity += MoveStats.Gravity * MoveStats.GravityOnReleaseMultiplier * Time.fixedDeltaTime;
            }
            else if (_fastFallTime < MoveStats.TimeForUpwardsCancel)
            {
                VerticcalVelocity = Mathf.Lerp(_fastFallReleaseSpeed, 0f, (_fastFallTime / MoveStats.TimeForUpwardsCancel));
            }
            _fastFallTime += Time.fixedDeltaTime; 

        }

        if (_isGrounded && !_isJumping)
        {
            if ( !_isFalling)
            {
                _isFalling = true;
            }
            VerticcalVelocity += MoveStats.Gravity * Time.fixedDeltaTime;
        }

        VerticcalVelocity = Mathf.Clamp(VerticcalVelocity, -MoveStats.MaxFallSpeed, 50f);

        _rb.velocity = new Vector2(_rb.velocity.x, VerticcalVelocity);


    }


    #region Gizmo


    private void DrawJumpArc(float moveSpeed, Color gizmoColor)
    {
        Vector2 startPosition = new Vector2(_feetColl.bounds.center.x, _feetColl.bounds.min.y);
        Vector2 previousPosition = startPosition;
        float speed = 0f;
        if (MoveStats.DrawRight)
        {
            speed = moveSpeed;
        }
        else
        {
            speed = -moveSpeed;
        }

        Vector2 velocity = new Vector2(speed, MoveStats.InitialJumpVelocity);

        Gizmos.color = gizmoColor;

        float timeStep = 2 * MoveStats.TimeTillJumpApex / MoveStats.ArcResolution; // time step for the simulation
        //float totalTime = (2 * MoveStats -imeTillJumpApex) + MoveStats.ApexHangTime; // total time of the arc including hang time

        for (int i = 0; i < MoveStats.VisualizationSteps; i++)
        {
            float simulationTime = i * timeStep;
            Vector2 displacement;
            Vector2 drawPoint;

            if (simulationTime < MoveStats.TimeTillJumpApex) // Ascending
            {
                displacement = velocity * simulationTime + 0.5f * new Vector2(0, MoveStats.Gravity) * simulationTime * simulationTime;
            }
            else if (simulationTime < MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime) // Apex hang time
            {
                float apexTime = simulationTime - MoveStats.TimeTillJumpApex;
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * apexTime; // No vertical move-ent during hang time
            }
            else // Descending
            {
                float descendTime = simulationTime - (MoveStats.TimeTillJumpApex + MoveStats.ApexHangTime);
                displacement = velocity * MoveStats.TimeTillJumpApex + 0.5f * new Vector2(0, MoveStats.Gravity) * MoveStats.TimeTillJumpApex * MoveStats.TimeTillJumpApex;
                displacement += new Vector2(speed, 0) * MoveStats.ApexHangTime; // Horizontal mo     ent during hang time
                displacement += new Vector2(speed, 0) * descendTime + 0.5f * new Vector2(0, MoveStats.Gravity) * descendTime * descendTime;
            }

            drawPoint = startPosition + displacement;

            if (MoveStats.StopOnCollision)
            {
                RaycastHit2D hit = Physics2D.Raycast(previousPosition, drawPoint - previousPosition, Vector2.Distance(previousPosition, drawPoint), MoveStats.GroundLayer);
                if (hit.collider != null)
                {
                    // If a hit is detected, stop drawing the arc at the hit point
                    Gizmos.DrawLine(previousPosition, hit.point);
                    break;
                }
            }

            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;

        }
    }


    #endregion

    // Start is called before the first frame update
    void Start()
    {
                        
    }

    private void OnDrawGizmos()
    {
        if (MoveStats.ShowWalkJumpArc)
        {
            DrawJumpArc(MoveStats.MaxWalkSpeed, Color.white);
            DrawJumpArc(MoveStats.MaxRunSpeed, Color.red);
        }
    }

    private void Update()
    {
        JumpCheck();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CollisionChecks();
        Jump();

        if(_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        } 
        else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }

    }
}
