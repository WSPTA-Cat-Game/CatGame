using CatGame.Interactables;
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
        private BoxCollider2D _collider;
        private CharacterMovement _movement;
        private InteractableHandler _interactableHandler;

        private void Start()
        {
            _renderer = GetComponent<SpriteRenderer>();
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
    }
}
