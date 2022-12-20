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
        
        private CharacterMode mode = CharacterMode.Human;
        private float lastTransformTime;

        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;
        private CharacterMovement movement;
        private InteractableHandler interactableHandler;

        private void Start()
        {
            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();
            movement = GetComponent<CharacterMovement>();
            interactableHandler = GetComponentInChildren<InteractableHandler>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
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
            if (mode == CharacterMode.Human)
            {
                // Check if new collider will hit anything
                if (Physics2D.BoxCast(
                    transform.position + new Vector3(0, spriteSizeDiff + 0.0125f),
                    catColliderSize - new Vector2(0.025f, 0.0125f), 
                    0, Vector2.up, 0, 1).collider != null)
                {
                    return;
                }

                mode = CharacterMode.Cat;
                movement.SetConfig(catMovement);
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

                mode = CharacterMode.Human;
                movement.SetConfig(humanMovement);
                _renderer.sprite = humanSprite;
                transform.position += new Vector3(0, -spriteSizeDiff);
                _collider.size = humanColliderSize;
            }

            lastTransformTime = Time.time;
        }
        private void OnDrawGizmos()
        {
            float spriteSizeDiff = catSprite.bounds.extents.y - humanSprite.bounds.extents.y;

            if (mode == CharacterMode.Human)
            {    
                Gizmos.DrawCube(transform.position + new Vector3(0, spriteSizeDiff + 0.0125f),
                    catColliderSize - new Vector2(0.025f, 0.0125f));
            }
            else
            {
                Gizmos.DrawCube(transform.position + new Vector3(0, -spriteSizeDiff + 0.0125f),
                    humanColliderSize - new Vector2(0.025f, 0.0125f));
            }
            
        }
    }
}
