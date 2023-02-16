using UnityEngine;

namespace CatGame.MovingTiles
{
    [RequireComponent(typeof(Collider2D))]
    public class LOSMovingTile : MovingTile
    {
        public Collider2D playerCollider;

        private readonly RaycastHit2D[] _hits = new RaycastHit2D[2];
        private Collider2D _collider;


        protected override void Start()
        {
            base.Start();
            _collider = GetComponent<Collider2D>();
        }

        protected override void Update()
        {
            if (playerCollider == null)
            {
                base.Update();
                return;
            }

            // Check if we have line of sight of the player collider
            Vector2 direction = playerCollider.bounds.center - _collider.transform.position;

            Physics2D.RaycastNonAlloc(_collider.transform.position, direction, _hits, direction.magnitude + 0.5f, ~(int)LayerMasks.IgnoreRaycast);

            // Ignore hitting ourselves, then check if we hit the player
            if (_hits[0].collider == _collider)
            {
                if (_hits[1].collider != playerCollider)
                {
                    base.Update();
                }
            }
            else if (_hits[0].collider != playerCollider)
            {
                base.Update();
            }
        }
    }
}
