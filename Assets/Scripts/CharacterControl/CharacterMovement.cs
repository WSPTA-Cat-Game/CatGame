using System.Linq;
using UnityEngine;

namespace CatGame.CharacterControl
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class CharacterMovement : MonoBehaviour
    {
        public float acceleration = 0;
        public float airAcceleration = 0;
        public float deceleration = 0;
        public float maxSpeed = 0;
        public float jumpHeight = 0;
        public float jumpCooldown = 0;
        public float additionalJumpHeight = 0;
        public float additionalJumpHeightTime = 0;
        public bool canWallHang = false;
        public float wallHangTime = 2.5f;

        private static readonly ContactFilter2D groundFilter = new()
        {
            useLayerMask = true,
            layerMask = (int)~LayerMasks.IgnoreRaycast,
            useNormalAngle = true,
            minNormalAngle = 89,
            maxNormalAngle = 91
        };

        private static readonly ContactFilter2D rightSideFilter = new()
        {
            useLayerMask = true,
            layerMask = (int)~LayerMasks.IgnoreRaycast,
            useNormalAngle = true,
            minNormalAngle = 175,
            maxNormalAngle = 185
        };
        private static readonly ContactFilter2D leftSideFilter = new()
        {
            useLayerMask = true,
            layerMask = (int)~LayerMasks.IgnoreRaycast,
            useNormalAngle = true,
            minNormalAngle = -5,
            maxNormalAngle = 5
        };

        private Rigidbody2D _rb;

        private bool _hasWallHangEnded = false;
        private bool _canStartWallHang = false;
        private float _lastWallHangTime = 0;
        private float _timeWallHanging = 0;
        private float _lastJumpTime = 0;
        private bool _spaceDown = false;
        private bool _spaceLetGo = false;

        public bool IsFacingLeft { get; private set; }

        private bool IsGrounded => _rb.IsTouching(groundFilter);
        private bool IsOnWall => canWallHang && (_rb.IsTouching(leftSideFilter) || _rb.IsTouching(rightSideFilter));
        private int WallDirection => !IsOnWall ? 0 : (_rb.IsTouching(leftSideFilter) ? -1 : 1);

        public void SetConfig(CharacterMovementConfig config)
        {
            acceleration = config.acceleration;
            airAcceleration = config.airAcceleration;
            deceleration = config.deceleration;
            maxSpeed = config.maxSpeed;
            jumpHeight = config.jumpHeight;
            jumpCooldown = config.jumpCooldown;
            additionalJumpHeight = config.additionalJumpHeight;
            additionalJumpHeightTime = config.additionalJumpHeightTime;
            canWallHang = config.canWallHang;
            wallHangTime = config.wallHangTime;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (InputHandler.Jump.WasPressedThisFrame())
            {
                _spaceDown = true;
            }
            else if (InputHandler.Jump.WasReleasedThisFrame())
            {
                _spaceLetGo = true;
            }
        }

        // Fixed update should be used to handle physics things, but input keydown/up
        // won't function properly in it, which is why I also have update
        private void FixedUpdate()
        {
            float horzInput = InputHandler.Move.ReadValue<Vector2>().x;
            IsFacingLeft = horzInput < 0;
            bool isInputPressed = Mathf.Abs(horzInput) > 0.01;

            // Create own acceleration because RigidBody2D doesn't have it for some reason
            float currentAcceleration = horzInput * (IsGrounded ? acceleration : airAcceleration);
            _rb.velocity += new Vector2(currentAcceleration * Time.fixedDeltaTime, 0);

            if (isInputPressed && Mathf.Abs(_rb.velocity.x) > maxSpeed)
            {
                // Limit speed only when trying to move
                _rb.velocity = new Vector2(maxSpeed * Mathf.Sign(_rb.velocity.x), _rb.velocity.y);
            }
            else if (IsGrounded)
            {
                // If not pressing buttons and grounded, decelerate
                float adjSpeed = _rb.velocity.x * deceleration;
                _rb.velocity = new Vector2(Mathf.Abs(adjSpeed) < 0.01 ? 0 : adjSpeed, _rb.velocity.y);
            }


            // Check if horz input is in the same direction as the wall
            if (isInputPressed && !IsGrounded && IsOnWall && Mathf.Sign(horzInput) == WallDirection && _canStartWallHang)
            {
                // Start wall hang
                _lastWallHangTime = Time.realtimeSinceStartup;
                _hasWallHangEnded = false;
                _canStartWallHang = false;

                _rb.gravityScale = 0;
                _rb.velocity = new Vector2(_rb.velocity.x, 0);
            }
            else
            {
                if (Time.realtimeSinceStartup - _lastWallHangTime + _timeWallHanging > wallHangTime)
                {
                    // If not on a wall, or we've been hanging for longer than
                    // wall hang time, then end wall hang
                    _hasWallHangEnded = true;
                    _rb.gravityScale = 1;
                }
                else if ((!IsOnWall || !isInputPressed) && !_canStartWallHang)
                {
                    _timeWallHanging += Time.realtimeSinceStartup - _lastWallHangTime;
                    _canStartWallHang = true;
                    _rb.gravityScale = 1;
                }
            }

            // Allow wall hangs once grounded
            if (IsGrounded)
            {
                _lastWallHangTime = 0;
                _timeWallHanging = 0;
                _canStartWallHang = true;
                _hasWallHangEnded = true;
            }

            float timeSinceLastJump = Time.realtimeSinceStartup - _lastJumpTime;
            if (_spaceDown)
            {
                // If grounded or current wall hanging and jump cooldown has worn off
                if ((IsGrounded || (IsOnWall && !_hasWallHangEnded)) && timeSinceLastJump > jumpCooldown)
                {
                    // Jump. Add horz force if on a wall and not grounded
                    _rb.AddForce(new Vector2(IsGrounded ? 0 : -WallDirection * jumpHeight * 0.7f, jumpHeight));
                    _lastJumpTime = Time.realtimeSinceStartup;
                    
                    // Stop wall hang
                    _rb.gravityScale = 1;
                    _hasWallHangEnded = true;
                }
                _spaceDown = false;
                _spaceLetGo = false;
            }
            else if (!_spaceLetGo && timeSinceLastJump <= additionalJumpHeightTime && !IsOnWall)
            {
                // Stop jump extension if let go of space
                _rb.AddForce(new Vector2(0, additionalJumpHeight * Time.fixedDeltaTime));
            }
        }
    }
}