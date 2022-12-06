using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.LevelManagement
{
    [RequireComponent(typeof(Collider2D))]
    public class LevelTransition : MonoBehaviour
    {
        public int nextLevelIndex;
        public string layerName;
        public Vector2? associatedSpawnPoint;

        public event Action<LevelTransition, Collider2D> OnTransitionEntered;

        private new Collider2D collider;
        private HashSet<Collider2D> currentColliders = new();

        private void Start()
        {
            collider = GetComponent<Collider2D>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // Only trigger if collision is completely within bounds
            if (collider.bounds.max.x > collision.bounds.max.x
                && collider.bounds.max.y > collision.bounds.max.y
                && collider.bounds.min.x < collision.bounds.min.x
                && collider.bounds.min.y < collision.bounds.min.y
                && !currentColliders.Contains(collision)
            ) {
                OnTransitionEntered?.Invoke(this, collision);
                currentColliders.Add(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            currentColliders.Remove(collision);
        }
    }
}
