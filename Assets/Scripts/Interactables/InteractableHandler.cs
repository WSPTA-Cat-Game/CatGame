using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableHandler : MonoBehaviour
    {
        public Collider2D playerCollider;
        public event Action<PickupBase> OnPickupChange;
        public bool CanPickup { get; set; }

        private readonly List<InteractableBase> _touchingInteractables = new();
        private PickupBase _currentPickup;
        private int _originalPickupLayer;

        private Bounds PlayerBounds => playerCollider != null ? playerCollider.bounds : new Bounds();

        public void PickupOrInteract()
        {
            // This prioritizes interacting/picking up a pickup over using the current pickup.
            if (_touchingInteractables.Count > 0)
            {
                PickupBase firstPickup = null;
                InteractableBase firstInteractable = null;
                foreach (InteractableBase interactable in _touchingInteractables)
                {
                    Vector2 direction = (playerCollider != null ? playerCollider.bounds.center : transform.position)
                        - interactable.transform.position;

                    RaycastHit2D[] hits = new RaycastHit2D[1];
                    interactable.Collider.Raycast(direction, hits, 7f, ~(int)LayerMasks.IgnoreRaycast);

                    if (hits[0].collider != playerCollider)
                    {
                        continue;
                    }

                    if (firstInteractable == null)
                    {
                        firstInteractable = interactable;
                    }

                    if (interactable is PickupBase pickup)
                    {
                        firstPickup = pickup;
                        break;
                    }
                }

                if (_currentPickup == null && firstPickup != null && CanPickup)
                {
                    _currentPickup = firstPickup;
                    _originalPickupLayer = _currentPickup.gameObject.layer;
                    _currentPickup.GetComponent<Rigidbody2D>().simulated = false;
                    _currentPickup.gameObject.layer = LayerMasks.IgnoreRaycast.ToLayer();
                    firstPickup.Pickup(transform);
                    OnPickupChange?.Invoke(firstPickup);
                }
                else if (firstInteractable != null)
                {
                    firstInteractable.Interact();
                }
            }
            // Use pickup
            else if (_currentPickup != null)
            {
                _currentPickup.Interact();
            }
        }

        public void DropPickup(bool isDropDirectionLeft = false)
        {
            if (_currentPickup == null)
            {
                return;
            }

            _currentPickup.gameObject.SetActive(true);

            // Default to left side if no character exists
            // If one does exist, default to opposite of whatever side it's
            // facing
            Vector3 dropSide = isDropDirectionLeft ? Vector2.left : Vector2.right;

            bool dropped = false;
            for (int i = 0; i < 2; i++)
            {
                // Check if the dropped pickup will collide
                Vector3 origin = PlayerBounds.center + dropSide
                    * (PlayerBounds.extents.x + _currentPickup.Collider.bounds.extents.x);
                int mask = (int)(LayerMasks.All ^ LayerMasks.IgnoreRaycast ^ LayerMasks.Player);

                if (Physics2D.BoxCast(
                    origin, _currentPickup.Collider.bounds.size, 0, dropSide, 0, mask).collider == null)
                {
                    dropped = true;
                    // If it did, then place and break
                    _currentPickup.transform.position = PlayerBounds.center
                        + dropSide * (PlayerBounds.extents.x + _currentPickup.Collider.bounds.extents.x + 0.02f);

                    break;
                }

                // If it didn't then switch sides
                dropSide.x *= -1;
            }

            // If it couldn't drop then just drop it above the player
            if (!dropped)
            {
                _currentPickup.transform.position = PlayerBounds.center + new Vector3(0, PlayerBounds.size.y);
            }

            _currentPickup.gameObject.layer = _originalPickupLayer;
            _currentPickup.GetComponent<Rigidbody2D>().simulated = true;
            _currentPickup.Drop();
            _currentPickup = null;
            OnPickupChange?.Invoke(null);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            InteractableBase interactable = collision.GetComponent<InteractableBase>();

            // If thing collided with has an interactable, add to list
            if (interactable != null)
            {
                _touchingInteractables.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            InteractableBase interactable = collision.GetComponent<InteractableBase>();

            // If thing exited has an interactable, remove from list
            if (interactable != null)
            {
                _touchingInteractables.Remove(interactable);
            }
        }

        private void Update()
        { 
            if (_currentPickup != null)
            {
                _currentPickup.transform.position = PlayerBounds.center
                    + new Vector3(0, PlayerBounds.size.y);
            }
        }
    }
}