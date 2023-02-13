using System.Collections;
using UnityEngine;

namespace CatGame.MovingTiles
{
    internal class ToggleMovingTile : MovingTile
    {
        private SpriteMask _spriteMask;
        private SpriteRenderer _renderer;
        private BoxCollider2D _collider;

        public void Toggle()
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

            StopAllCoroutines();
            StartCoroutine(ToggleCoroutine());
        }

        protected override void Update()
        {
            // Make size of collider same size as masked in area
            if (_spriteMask != null && _renderer != null && _collider != null)
            {
                if (!_spriteMask.bounds.Intersects(_renderer.bounds))
                {
                    _collider.size = Vector2.zero;
                    return;
                }

                Bounds overlap = new();

                // This assumes 2d movement
                float x1 = Mathf.Max(_spriteMask.bounds.min.x, _renderer.bounds.min.x);
                float x2 = Mathf.Min(_spriteMask.bounds.max.x, _renderer.bounds.max.x);

                float y1 = Mathf.Max(_spriteMask.bounds.min.y, _renderer.bounds.min.y);
                float y2 = Mathf.Min(_spriteMask.bounds.max.y, _renderer.bounds.max.y);

                float minX;
                float maxX;
                if (x1 < x2)
                {
                    minX = x1;
                    maxX = x2;
                }
                else
                {
                    minX = x2;
                    maxX = x1;
                }

                if (y1 < y2)
                {
                    overlap.min = new Vector3(minX, y1);
                    overlap.max = new Vector3(maxX, y2);
                }
                else
                {
                    overlap.min = new Vector3(minX, y2);
                    overlap.max = new Vector3(maxX, y1);
                }

                Vector3 localSize = _collider.transform.InverseTransformVector(overlap.size);
                localSize.Set(Mathf.Abs(localSize.x), Mathf.Abs(localSize.y), Mathf.Abs(localSize.z));
                localSize -= new Vector3(_collider.edgeRadius * 2, _collider.edgeRadius * 2);

                _collider.offset = _collider.transform.InverseTransformPoint(overlap.center);
                _collider.size = localSize;
            }
        }

        private void Awake()
        {
            if (transform.parent != null)
            {
                _spriteMask = transform.parent.GetComponent<SpriteMask>();
            }

            _renderer = GetComponent<SpriteRenderer>();
            _collider = GetComponent<BoxCollider2D>();

            stopTime = 0;
        }

        private IEnumerator ToggleCoroutine()
        {
            // We have to update once to start the platform again
            base.Update();
            stopTime = 0;
            yield return null;

            while (IsMoving)
            {
                base.Update();
                yield return null;
            }
        }
    }
}
