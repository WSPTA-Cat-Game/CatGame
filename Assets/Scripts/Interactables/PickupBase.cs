using CatGame.CharacterControl;
using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PickupBase : InteractableBase
    {
        protected bool isHeld = false;
        protected Transform holder = null;

        protected new Collider2D collider;

        private int originalLayer;

        private void Start()
        {
            collider = GetComponent<Collider2D>();
            originalLayer = gameObject.layer;
        }

        public virtual void Pickup(Transform holder) 
        {
            this.holder = holder;
            gameObject.layer = 2;
            gameObject.SetActive(false);
            isHeld = true;
        }

        public virtual void Drop()
        {
            gameObject.SetActive(true);
            isHeld = false;

            Character character = holder.GetComponentInParent<Character>();
            
            // Default to left side if no character exists
            // If one does exist, default to opposite of whatever side it's
            // facing
            Vector3 dropSide = character != null && character.IsFacingLeft ? Vector2.right : Vector2.left;

            Collider2D characterCollider = character != null ? character.GetComponent<Collider2D>() : null;
            Bounds characterColliderBounds = characterCollider != null ? characterCollider.bounds : new Bounds();

            bool dropped = false;
            for (int i = 0; i < 2; i++)
            {
                // Check if the dropped pickup will collider
                if (Physics2D.BoxCast(
                    holder.position,
                    collider.bounds.size,
                    0,
                    dropSide,
                    characterColliderBounds.extents.x + collider.bounds.extents.x + 0.05f).collider == null)
                {
                    dropped = true;
                    // If it did, then place and break
                    transform.position = holder.position
                        + dropSide * (characterColliderBounds.extents.x + collider.bounds.extents.x + 0.05f);


                    break;
                }

                // If it didn't then switch sides
                dropSide.x *= -1;
            }
            
            // If it couldn't drop then just drop it above the player
            if (!dropped)
            {
                transform.position = holder.position + new Vector3(0, characterColliderBounds.size.y);
            }

            gameObject.layer = originalLayer;
            holder = null;
        }
    }
}
