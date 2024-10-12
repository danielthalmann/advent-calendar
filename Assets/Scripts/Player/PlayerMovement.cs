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

    private void CollisionChecks()
    {
        IsGrounded();
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
        if ( _JumpBufferTimer > _numberOfJumpUsed && !_isJumping && (_isGrounded || _coyoteTimer > 0f))
        {
            InitiateJump(1);

            if(_JumpReleaseDuringBuffer)
            {
                _isFastFalling = true;
                _fastFallReleaseSpeed = VerticcalVelocity;
            }
        }

        // double jump
        else if (_JumpBufferTimer > 0f && _isJumping && _numberOfJumpUsed < MoveStats.NumberOfJumpAllowed)
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

        if ((_isJumping || _isFalling))
        {

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

    // Start is called before the first frame update
    void Start()
    {

                
    }

    private void Update()
    {
        JumpCheck();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CollisionChecks();

        if(_isGrounded)
        {
            Move(MoveStats.GroundAcceleration, MoveStats.GroundDeceleration, InputManager.Movement);
        } else
        {
            Move(MoveStats.AirAcceleration, MoveStats.AirDeceleration, InputManager.Movement);
        }

    }
}
