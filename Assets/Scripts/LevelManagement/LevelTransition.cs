﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace CatGame.LevelManagement
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class LevelTransition : MonoBehaviour
    {
        public int nextLevelIndex;
        public string layerName;
        public bool hasAssociatedSpawnPoint;
        public Vector2 associatedSpawnPoint;
        public bool canExitWithoutCat;

        public event Action<LevelTransition, Collider2D> OnTransitionEntered;

        private Collider2D _collider;
        // Need to keep track of what is colliding with the transition 
        // manually
        private readonly HashSet<Collider2D> _currentColliders = new();

        private void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
        }

        private void OnTriggerStay2D(Collider2D collision)
        {
            // Only trigger if collision is completely within bounds
            if (_collider.bounds.max.x > collision.bounds.max.x
                && _collider.bounds.max.y > collision.bounds.max.y
                && _collider.bounds.min.x < collision.bounds.min.x
                && _collider.bounds.min.y < collision.bounds.min.y
                && !_currentColliders.Contains(collision)) 
            {
                OnTransitionEntered?.Invoke(this, collision);
                _currentColliders.Add(collision);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            _currentColliders.Remove(collision);
        }
    }
}
