using System;

namespace CatGame.CharacterControl
{
    [Serializable]
    public class CharacterMovementConfig
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
        public float wallHangTime = 0;
    }
}
