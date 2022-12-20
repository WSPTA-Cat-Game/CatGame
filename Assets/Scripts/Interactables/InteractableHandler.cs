using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableHandler : MonoBehaviour
    {
        private readonly List<InteractableBase> _touchingInteractables = new();
        private PickupBase _currentPickup;

        public void PickupOrInteract()
        {
            // This prioritizes interacting/picking up a pickup over using the current pickup.
            if (_touchingInteractables.Count > 0)
            {
                PickupBase firstPickup = (PickupBase)_touchingInteractables.FirstOrDefault(val => val is PickupBase);

                if (_currentPickup == null && firstPickup != null)
                {
                    _currentPickup = firstPickup;
                    firstPickup.Pickup(transform);
                }
                else
                {
                    _touchingInteractables[0].Interact();
                }
            }
            // Use pickup
            else if (_currentPickup != null)
            {
                _currentPickup.Interact();
            }
        }

        public void DropPickup()
        {
            if (_currentPickup == null)
            {
                return;
            }

            _currentPickup.Drop();
            _currentPickup = null;
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
            // Pickup or use current pickup/interactable
            if (InputHandler.Interact.WasPressedThisFrame())
            {
                PickupOrInteract();
            } 
            // Drop current pickup
            else if (InputHandler.Drop.WasPressedThisFrame())
            {
                DropPickup();
            }
        }
    }
}