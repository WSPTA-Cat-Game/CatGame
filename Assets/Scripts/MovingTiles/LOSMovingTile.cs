﻿using UnityEngine;

namespace CatGame.MovingTiles
{
    [RequireComponent(typeof(Collider2D))]
    public class LOSMovingTile : MovingTile
    {
        public Collider2D playerCollider;

        private Collider2D _collider;

        protected override void Start()
        {
            base.Start();
            _collider = GetComponent<Collider2D>();
        }

        protected override void Update()
        {
            Vector2 direction = (playerCollider != null ? playerCollider.bounds.center : transform.position)
                - _collider.transform.position;

            RaycastHit2D[] hits = new RaycastHit2D[1];
            _collider.Raycast(direction, hits, float.PositiveInfinity, ~(int)LayerMasks.IgnoreRaycast);

            if (hits[0].collider != playerCollider)
            {
                base.Update();
            }
        }
    }
}