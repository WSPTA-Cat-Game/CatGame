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
        public Sprite humanSprite;
        public Vector2 humanColliderSize;

        [Header("Cat Settings")]
        public CharacterMovementConfig catMovement = new();
        public Sprite catSprite;
        public Vector2 catColliderSize;
        
        private CharacterMode _mode = CharacterMode.Human;
        private float lastTransformTime;

        private SpriteRenderer _renderer;
        private Animator _animator;
        private BoxCollider2D _collider;
        private CharacterMovement _movement;
        private InteractableHandler _interactableHandler;

        public bool IsFacingLeft => _movement.IsFacingLeft;

        private void Start()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<BoxCollider2D>();
            _movement = GetComponent<CharacterMovement>();
            _interactableHandler = GetComponentInChildren<InteractableHandler>();
        }

        private void Update()
        {
            if (InputHandler.Transition.WasPressedThisFrame())
            {
                ToggleMode();
            }

            _renderer.flipX = IsFacingLeft;


            // Update animator
            _animator.SetBool("IsFalling", _movement.Velocity.y < -0.01);
            _animator.SetBool("IsRising", _movement.Velocity.y > 0.01);
            _animator.SetBool("IsPickup", InputHandler.Interact.WasPressedThisFrame());
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
            AnimatorStateInfo state = _animator.GetCurrentAnimatorStateInfo(0);
            if (state.IsName("Pick up") || InputHandler.Interact.WasPressedThisFrame())
            {
                _movement.Stop();
                _movement.enabled = false;
            }
            else
            {
                // Recontinue once pickup ends
                _movement.enabled = true;
            }
        }

        private void ToggleMode()
        {
            if (Time.time - lastTransformTime <= transformDelay)
            {
                return;
            }

            float spriteSizeDiff = catSprite.bounds.extents.y - humanSprite.bounds.extents.y;
            if (_mode == CharacterMode.Human)
            {
                // Check if new collider will hit anything
                if (Physics2D.BoxCast(
                    transform.position + new Vector3(0, spriteSizeDiff + 0.0125f),
                    catColliderSize - new Vector2(0.025f, 0.0125f), 
                    0, Vector2.up, 0, 1).collider != null)
                {
                    return;
                }

                _mode = CharacterMode.Cat;
                _movement.SetConfig(catMovement);
                _interactableHandler.DropPickup();
                _interactableHandler.enabled = false;
                _renderer.sprite = catSprite;
                _collider.size = catColliderSize;
                transform.position += new Vector3(0, spriteSizeDiff);
            }
            else
            {
                // Check if new collider will hit anything
                if (Physics2D.BoxCast(
                    transform.position + new Vector3(0, -spriteSizeDiff + 0.0125f),
                    humanColliderSize - new Vector2(0.025f, 0.0125f),
                    0, Vector2.up, 0, 1).collider != null)
                {
                    return;
                }

                _mode = CharacterMode.Human;
                _movement.SetConfig(humanMovement);
                _interactableHandler.enabled = true;
                _renderer.sprite = humanSprite;
                transform.position += new Vector3(0, -spriteSizeDiff);
                _collider.size = humanColliderSize;
            }

            lastTransformTime = Time.time;
        }

        private bool IsColliderTouchingNonStaticRB(Collider2D collider)
            => collider.attachedRigidbody != null && collider.attachedRigidbody.bodyType != RigidbodyType2D.Static;
    }
}
