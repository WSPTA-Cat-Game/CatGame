﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CatGame.Interactables
{
    [RequireComponent(typeof(Collider2D))]
    public class InteractableHandler : MonoBehaviour
    {
        public Collider2D playerCollider;
        public SpriteRenderer playerRenderer;
        public event Action<PickupBase> OnPickupChange;
        public PickupBase CurrentPickup => _currentPickup;
        public bool CanPickup { get; set; }

        private readonly RaycastHit2D[] _hitsArray = new RaycastHit2D[5];

        private readonly List<InteractableBase> _touchingInteractables = new();
        private PickupBase _currentPickup;
        private int _originalPickupLayer;
        private float _lastPickupXPosition;
        private float _lastPickupXVelocity;

        private Bounds PlayerBounds => playerCollider != null ? playerCollider.bounds : new Bounds();
        private Bounds PlayerSpriteBounds => playerRenderer != null ? playerRenderer.bounds : new Bounds();

        public void PickupOrInteract()
        {
            // This prioritizes interacting/picking up a pickup over using the current pickup.
            if (_touchingInteractables.Count > 0)
            {
                PickupBase firstPickup = null;
                InteractableBase firstInteractable = null;
                foreach (InteractableBase interactable in _touchingInteractables)
                {
                    // Check if there is anything between the pickup and player
                    Vector2 direction = (playerCollider != null ? playerCollider.bounds.center : transform.position)
                        - interactable.transform.position;

                    int mask = Physics2D.GetLayerCollisionMask(LayerMasks.Player.ToLayer())
                        & ~(int)LayerMasks.IgnoreRaycast;
                    int hitCount = interactable.Collider.Raycast(direction, _hitsArray, 7f, mask);

                    // If there is something the player can collide with
                    // inbetween the pickup and player, don't pick up
                    bool isPickupBlocked = false;
                    for (int i = 0; i < hitCount && i < _hitsArray.Length; i++)
                    {
                        if (!_hitsArray[i].collider.isTrigger)
                        {
                            isPickupBlocked = _hitsArray[i] != playerCollider;
                            break;
                        }
                    }

                    if (isPickupBlocked)
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

                // Pickup if theres one in range
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
                    // Else interact
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

            _currentPickup.transform.localRotation = Quaternion.identity;

            // Default to left side if no character exists
            // If one does exist, default to opposite of whatever side it's
            // facing
            Vector3 dropSide = isDropDirectionLeft ? Vector2.left : Vector2.right;

            bool dropped = false;
            for (int i = 0; i < 2; i++)
            {
                // Check if the dropped pickup will collide
                Vector3 origin = new(PlayerBounds.center.x, PlayerBounds.min.y + _currentPickup.Collider.bounds.size.y);
                origin += dropSide * (PlayerBounds.extents.x + _currentPickup.Collider.bounds.extents.x - 0.25f);

                int mask = Physics2D.GetLayerCollisionMask(_originalPickupLayer) 
                    & ~(int)LayerMasks.IgnoreRaycast & ~(int)LayerMasks.Player;

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
                _currentPickup.transform.position = new Vector3(
                    PlayerBounds.center.x,
                    PlayerBounds.max.y + _currentPickup.Collider.bounds.extents.y);
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

        private void FixedUpdate()
        { 
            if (_currentPickup != null)
            {
                // Lerp to position right above head
                _currentPickup.transform.position = Vector3.Lerp(
                    _currentPickup.transform.position,
                    PlayerSpriteBounds.center
                        + new Vector3(0, PlayerSpriteBounds.extents.y + _currentPickup.Collider.bounds.extents.y),
                    0.7f * Time.fixedDeltaTime * 60); 

                // Rotate based on x acceleration
                // This gives the pickups the springy movement on the player's
                // head
                float xVel = _lastPickupXPosition - _currentPickup.transform.position.x;
                float xAccel = _lastPickupXVelocity - xVel;
                float targetRot = Mathf.Clamp(xAccel * 20 / Time.deltaTime, -10, 10);

                Vector3 rot = _currentPickup.transform.localRotation.eulerAngles;
                rot.z = Mathf.LerpAngle(rot.z, targetRot, 0.075f * Time.fixedDeltaTime * 60);
                _currentPickup.transform.localEulerAngles = rot;

                _lastPickupXPosition = _currentPickup.transform.position.x;
                _lastPickupXVelocity = xVel;
            }
        }
    }
}