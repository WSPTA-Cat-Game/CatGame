using CatGame.Interactables;
using CatGame.MovingTiles;
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
        public float humanMass;

        [Header("Cat Settings")]
        public CharacterMovementConfig catMovement = new();
        public Vector2 catColliderSize;
        public Vector2 catColliderOffset;
        public float catMass;

        private CharacterMode _mode = CharacterMode.Human;
        private float lastTransformTime = float.MinValue;

        private SpriteRenderer _renderer;
        private Animator _animator;
        private BoxCollider2D _collider;
        private CharacterMovement _movement;
        private InteractableHandler _interactableHandler;
        private Rigidbody2D _rb;
        private AudioSource _audioSource;

        private Dictionary<string, AudioClip> _sfxClips;
        private bool lastPlayedStep1 = false;

        private bool hasPickedUp = false;

        public CharacterMovement Movement => _movement;
        public InteractableHandler InteractableHandler => _interactableHandler;
        public bool IsFacingLeft => _movement.IsFacingLeft;

        // This is so the steps alternate
        public void PlayStep()
        {
            if (lastPlayedStep1)
            {
                PlaySFX("Steps 2");
            }
            else
            {
                PlaySFX("Steps 1");
            }

            lastPlayedStep1 = !lastPlayedStep1;
        }

        public void PlaySFX(string sfxName)
        {
            _audioSource.PlayOneShot(_sfxClips[sfxName]);
        }

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            _collider = GetComponent<BoxCollider2D>();
            _movement = GetComponent<CharacterMovement>();
            _interactableHandler = GetComponentInChildren<InteractableHandler>();
            _interactableHandler.OnPickupChange += 
                pickup => hasPickedUp = pickup != null;
            _rb = GetComponent<Rigidbody2D>();
            _audioSource = GetComponent<AudioSource>();

            AudioClip[] loadedClips = Resources.LoadAll<AudioClip>("SFX");
            _sfxClips = loadedClips.ToDictionary(clip => clip.name, clip => clip);

            _movement.OnJump += () =>
            {
                if (_mode == CharacterMode.Cat)
                {
                    PlaySFX("Cat Jump");
                }
                else
                {
                    PlaySFX("Jump");
                }
            };

            // This ensures the values on the player are actually right
            SetMode(_mode);
        }

        private void Update()
        {
#if UNITY_EDITOR
            AudioClip[] loadedClips = Resources.LoadAll<AudioClip>("SFX");
            _sfxClips = loadedClips.ToDictionary(clip => clip.name, clip => clip);
#endif

            if (InputHandler.Transition.WasPressedThisFrame())
            {
                ToggleMode();
            }

            _renderer.flipX = IsFacingLeft;

            // Play sound if landed
            // Cat has a dedicated land animation so we need to check if we're
            if (!_animator.GetBool("IsGrounded") && _movement.IsGrounded)
            {
                if (_mode == CharacterMode.Human)
                {
                    PlaySFX("Land");
                }
                else
                {
                    PlaySFX("Cat Land");
                }
            }

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

            // If not grounded, then we can't pickup
            _interactableHandler.CanPickup = _movement.IsGrounded && _mode == CharacterMode.Human;

            // Pickup or drop
            if (InputHandler.Interact.WasPressedThisFrame())
            {
                if (_interactableHandler.CurrentPickup == null)
                {
                    _interactableHandler.PickupOrInteract();
                }
                else
                {
                    PlaySFX("Put Down");
                    _interactableHandler.DropPickup(_movement.IsFacingLeft);
                }
            }
            
            // Toggle tentacles
            if (InputHandler.ToggleTentacles.WasPressedThisFrame() && _mode == CharacterMode.Cat)
            {
                foreach (MovingTile tile in MovingTile.Tiles)
                {
                    if (tile is ToggleMovingTile toggleTile)
                    {
                        toggleTile.Toggle();
                    }
                }
            }
        }

        private void SetMode(CharacterMode mode)
        {
            if (Time.time - lastTransformTime <= transformDelay)
            {
                return;
            }

            int layerMask = Physics2D.GetLayerCollisionMask(LayerMasks.Player.ToLayer()) 
                & ~(int)LayerMasks.Player & ~(int)LayerMasks.IgnoreRaycast;

            switch (mode)
            {
                case CharacterMode.Human:
                    // Check if new collider will hit anything
                    if (Physics2D.BoxCast(
                        transform.position + (Vector3)humanColliderOffset,
                        new Vector2(0.1f, humanColliderSize.y),
                        0, Vector2.up, 0, layerMask).collider != null)
                    {
                        return;
                    }

                    _movement.SetConfig(humanMovement);
                    _interactableHandler.CanPickup = true;
                    _animator.SetLayerWeight(1, 1);
                    _animator.SetLayerWeight(2, 0);
                    _collider.size = humanColliderSize;
                    _collider.offset = humanColliderOffset;
                    _rb.mass = humanMass;
                    break;

                case CharacterMode.Cat:
                default:
                    //Check if new collider will hit anything
                    if (Physics2D.BoxCast(
                        transform.position + (Vector3)catColliderOffset,
                        new Vector2(0.1f, catColliderSize.y),
                        0, Vector2.up, 0, layerMask).collider != null)
                    {
                        return;
                    }

                    _movement.SetConfig(catMovement);
                    _interactableHandler.DropPickup(!_movement.IsFacingLeft);
                    _interactableHandler.CanPickup = false;
                    _animator.SetLayerWeight(1, 0);
                    _animator.SetLayerWeight(2, 1);
                    _collider.size = catColliderSize;
                    _collider.offset = catColliderOffset;
                    _rb.mass = catMass;
                    break;
            }

            _mode = mode;

            lastTransformTime = Time.time;
        }

        private void ToggleMode()
        {
            if (_mode == CharacterMode.Human)
            {
                SetMode(CharacterMode.Cat);
            }
            else
            {
                SetMode(CharacterMode.Human);
            }
        }

        private bool IsColliderTouchingNonStaticRB(Collider2D collider)
            => collider.attachedRigidbody != null && collider.attachedRigidbody.bodyType != RigidbodyType2D.Static;
    }
}
