using UnityEngine;

namespace CharacterControl
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

        private Rigidbody2D rb;
        private new Collider2D collider;

        private bool isGrounded = true;
        private bool isOnWall = false;
        private int wallDirection = 0;
        private bool canStartWallHang = true;
        private bool hasWallHangEnded = false;
        private float lastWallHangTime = 0;
        private float lastJumpTime = 0;
        private bool spaceDown = false;
        private bool spaceLetGo = false;

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
            rb = GetComponent<Rigidbody2D>();
            collider = rb.GetComponent<Collider2D>();
        }

        private void Update()
        {
            // Check if grounded and on a wall
            float edgeRadius = (collider is BoxCollider2D boxCollider ? boxCollider.edgeRadius * 2 : 0);
            Vector2 colliderSize = collider.bounds.size + new Vector3(edgeRadius, edgeRadius);

            RaycastHit2D downRaycast = Physics2D.BoxCast(transform.position, colliderSize, 0, Vector2.down, 0.08f);
            isGrounded = downRaycast.collider != null;

            RaycastHit2D sideRaycast = Physics2D.CapsuleCast(transform.position - new Vector3(0.08f, 0), colliderSize, CapsuleDirection2D.Horizontal, 0, Vector2.right, 0.16f);
            isOnWall = sideRaycast.collider != null && canWallHang;
            wallDirection = !isOnWall ? 0 : (sideRaycast.fraction < 0.5 ? -1 : 1);

            if (Input.GetKeyDown(KeyCode.Space))
            {
                spaceDown = true;
            }
            else if (Input.GetKeyUp(KeyCode.Space))
            {
                spaceLetGo = true;
            }
        }

        // Fixed update should be used to handle physics things, but input keydown/up
        // won't function properly in it, which is why I also have update
        private void FixedUpdate()
        {
            float horzInput = Input.GetAxisRaw("Horizontal");

            // Create own acceleration because RigidBody2D doesn't have it for some reason
            float currentAcceleration = horzInput * (isGrounded ? acceleration : airAcceleration);
            rb.velocity += new Vector2(currentAcceleration * Time.fixedDeltaTime, 0);

            if (Mathf.Abs(horzInput) > 0.01)
            {
                // Limit speed only when trying to move
                if (Mathf.Abs(rb.velocity.x) > maxSpeed)
                {
                    rb.velocity = new Vector2(maxSpeed * Mathf.Sign(rb.velocity.x), rb.velocity.y);
                }

                // Check if horz input is in the same direction as the wall
                if (!isGrounded && Mathf.Sign(horzInput) == wallDirection && canStartWallHang)
                {
                    // Start wall hang
                    lastWallHangTime = Time.realtimeSinceStartup;
                    canStartWallHang = false;
                    hasWallHangEnded = false;

                    rb.gravityScale = 0;
                    rb.velocity = new Vector2(rb.velocity.x, 0);
                }
                else if (Time.realtimeSinceStartup - lastWallHangTime > wallHangTime || !isOnWall)
                {
                    // If not on a wall, or we've been hanging for longer than
                    // wall hang time, then end wall hang
                    rb.gravityScale = 1;
                    hasWallHangEnded = true;
                }
            }
            else
            {
                // If horz input is too small, end wall hang
                rb.gravityScale = 1;
                hasWallHangEnded = true;

                // If not pressing buttons and grounded, decelerate
                if (isGrounded)
                {
                    float adjSpeed = rb.velocity.x * deceleration;
                    rb.velocity = new Vector2(Mathf.Abs(adjSpeed) < 0.01 ? 0 : adjSpeed, rb.velocity.y);
                }
            }

            // Allow wall hangs once grounded
            if (isGrounded)
            {
                lastWallHangTime = 0;
                canStartWallHang = true;
            }

            float timeSinceLastJump = Time.realtimeSinceStartup - lastJumpTime;
            if (spaceDown)
            {
                // If grounded or current wall hanging and jump cooldown has worn off
                if ((isGrounded || (isOnWall && !hasWallHangEnded)) && timeSinceLastJump > jumpCooldown)
                {
                    // Jump. Add horz force if on a wall and not grounded
                    rb.AddForce(new Vector2(isGrounded ? 0 : -wallDirection * jumpHeight * 0.7f, jumpHeight));
                    lastJumpTime = Time.realtimeSinceStartup;
                    
                    // Stop wall hang
                    rb.gravityScale = 1;
                    hasWallHangEnded = true;
                }
                spaceDown = false;
                spaceLetGo = false;
            }
            else if (!spaceLetGo && timeSinceLastJump <= additionalJumpHeightTime && !isOnWall)
            {
                // Stop jump extension if let go of space
                rb.AddForce(new Vector2(0, additionalJumpHeight * Time.fixedDeltaTime));
            }
        }
    }
}