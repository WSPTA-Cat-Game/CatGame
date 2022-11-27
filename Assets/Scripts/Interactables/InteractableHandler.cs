using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableHandler : MonoBehaviour
    {
        public PickupBase currentPickup;
        
        private readonly List<InteractableBase> touchingInteractables = new();

        public void PickupOrInteract()
        {
            // This prioritizes interacting/picking up a pickup over using the current pickup.
            if (touchingInteractables.Count > 0)
            {
                InteractableBase firstInteractable = touchingInteractables[0];
                PickupBase firstPickup = (PickupBase)touchingInteractables.FirstOrDefault(val => val is PickupBase);

                if (currentPickup == null && firstPickup != null)
                {
                    currentPickup = firstPickup;
                    firstPickup.Pickup(transform);
                }
                else
                {
                    firstInteractable.Interact();
                }
            }
            // Use pickup
            else if (currentPickup != null)
            {
                currentPickup.Interact();
            }
        }

        public void DropPickup()
        {
            if (currentPickup == null)
            {
                return;
            }

            currentPickup.Drop();
            currentPickup = null;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            InteractableBase interactable = collision.GetComponent<InteractableBase>();

            // If thing collided with has an interactable and is on the same layer, add to list
            if (interactable != null && interactable.gameObject.layer == gameObject.layer)
            {
                touchingInteractables.Add(interactable);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            InteractableBase interactable = collision.GetComponent<InteractableBase>();

            // If thing exited has an interactable and is on the same layer, remove from list
            if (interactable != null && interactable.gameObject.layer == gameObject.layer)
            {
                touchingInteractables.Remove(interactable);
            }
        }

        private void Update()
        { 
            // Pickup or use current pickup/interactable
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PickupOrInteract();
            } 
            // Drop current pickup
            else if (Input.GetKeyDown(KeyCode.F))
            {
                DropPickup();
            }
        }
    }
}