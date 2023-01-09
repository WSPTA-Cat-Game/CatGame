using CatGame.Interactables;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CatGame.CharacterControl
{
    [RequireComponent(typeof(CharacterMovement))]
    public class Character : MonoBehaviour
    {
        public float transformDelay;

        [Header("Human Settings")]
        public CharacterMovementConfig humanMovement = new();
        public Vector2 humanColliderSize;
        public Vector2 humanColliderOffset;

        [Header("Cat Settings")]
        public CharacterMovementConfig catMovement = new();
        public Vector2 catColliderSize;
        public Vector2 catColliderOffset;

        private CharacterMode _mode = CharacterMode.Human;
        private float lastTransformTime;

        private SpriteRenderer _renderer;
        private Animator _animator;
        private BoxCollider2D _collider;
        private CharacterMovement _movement;
        private InteractableHandler _interactableHandler;

        private bool hasPickedUp = false;

        public bool IsFacingLeft => _movement.IsFacingLeft;


        private void Start()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<BoxCollider2D>();
            _movement = GetComponent<CharacterMovement>();
            _interactableHandler = GetComponentInChildren<InteractableHandler>();
            _interactableHandler.OnPickupChange += 
                pickup => hasPickedUp = pickup != null;
        }

        private void Update()
        {
            if (InputHandler.Transition.WasPressedThisFrame())
            {
                ToggleMode();
            }

            _renderer.flipX = IsFacingLeft;


            // Update animator
            _animator.SetFloat("YMovement", _movement.Velocity.y);
            _animator.SetBool("IsGrounded", _movement.IsGrounded);
            _animator.SetBool("IsPickup", hasPickedUp);
            _animator.SetBool("IsMoving", Mathf.Abs(_movement.Velocity.x) > 0.1 && _movement.enabled);

            
            bool isPushing = false;
            // Pushing is true if we're touching any non static rigidbody on
            // either side
            List<Collider2D> touching = _movement.LeftSideContacts;
            if (_movement.RightSideContacts.Any(IsColliderTouchingNonStaticRB)
                || _movement.LeftSideContacts.Any(IsColliderTouchingNonStaticRB))
            {
                isPushing = true;
            }
            _animator.SetBool("IsPushing", isPushing);

            // Stop if pickup 
            if (_movement.enabled)
            {
                if (hasPickedUp)
                {
                    _movement.Stop();
                    _movement.enabled = false;
                }
            }
            else
            {
                AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(1);
                _movement.enabled = !state.IsName("Pick up");
            }
            hasPickedUp = false;
        }

        private void ToggleMode()
        {
            if (Time.time - lastTransformTime <= transformDelay)
            {
                return;
            }

            if (_mode == CharacterMode.Human)
            {
                //Check if new collider will hit anything
                if (Physics2D.BoxCast(
                    transform.position + (Vector3)catColliderOffset,
                    catColliderSize,
                    0, Vector2.up, 0, 
                    ~(int)(LayerMasks.IgnoreRaycast | LayerMasks.Player)).collider != null)
                {
                    return;
                }

                _mode = CharacterMode.Cat;
                _movement.SetConfig(catMovement);
                _interactableHandler.DropPickup();
                _interactableHandler.enabled = false;
                _animator.SetLayerWeight(1, 0);
                _animator.SetLayerWeight(2, 1);
                _collider.size = catColliderSize;
                _collider.offset = catColliderOffset;
            }
            else
            {
                // Check if new collider will hit anything
                if (Physics2D.BoxCast(
                    transform.position + (Vector3)humanColliderOffset,
                    humanColliderSize,
                    0, Vector2.up, 0, 
                    ~(int)(LayerMasks.IgnoreRaycast | LayerMasks.Player)).collider != null)
                {
                    return;
                }

                _mode = CharacterMode.Human;
                _movement.SetConfig(humanMovement);
                _interactableHandler.enabled = true;
                _animator.SetLayerWeight(1, 1);
                _animator.SetLayerWeight(2, 0);
                _collider.size = humanColliderSize;
                _collider.offset = humanColliderOffset;
            }

            lastTransformTime = Time.time;
        }

        private bool IsColliderTouchingNonStaticRB(Collider2D collider)
            => collider.attachedRigidbody != null && collider.attachedRigidbody.bodyType != RigidbodyType2D.Static;
    }
}
