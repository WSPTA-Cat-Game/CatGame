using System.Collections.Generic;
using UnityEngine;

namespace CatGame.MovingTiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingTile : MonoBehaviour
    {
        public Vector3 endPos;

        public float speed = 1.5f;
        public float stopTime = 1f;

        private static readonly HashSet<MovingTile> _tiles = new();
        private static readonly Dictionary<Transform, Transform> _originalParents = new();

        private readonly ContactFilter2D _aboveFilter = new()
        {
            useNormalAngle = true,
            minNormalAngle = 269,
            maxNormalAngle = 271,

            useLayerMask = true,
            layerMask = ~(int)LayerMasks.Default
        };

        private Rigidbody2D _rb;

        private Vector3 _startPos;

        private float _moveTime;
        private float _endTime;
        private bool _isMoving;
        private bool _isReversed = false;

        protected bool IsMoving => _isMoving;
        public static IReadOnlyCollection<MovingTile> Tiles => _tiles;
        
        protected virtual void Start()
        {
            _tiles.Add(this);
            _rb = GetComponent<Rigidbody2D>();
            _startPos = transform.localPosition;
        }
        
        protected virtual void Update() 
        {
            Vector3 currentStartPos = _startPos;
            Vector3 currentEndPos = endPos;

            if (_isReversed)
            {
                currentStartPos = endPos;
                currentEndPos = _startPos;
            }

            if (_isMoving)
            {
                _moveTime += Time.deltaTime;

                Vector3 direction = currentEndPos - currentStartPos;
                float timeToMove = direction.magnitude / speed;
                
                Vector3 resPos = currentStartPos
                    + _moveTime * speed * direction.normalized;


                // Check if we've gone longer than the length
                if (_moveTime > timeToMove)
                {
                    transform.localPosition = currentEndPos;
                    _isMoving = false;
                    _endTime = 0;
                    _isReversed = !_isReversed;
                }
                else
                {
                    transform.localPosition = resPos;
                }
            }
            else
            {
                // Wait until stop time is over then move again
                _endTime += Time.deltaTime;

                if (_endTime > stopTime)
                {
                    _isMoving = true;
                    _moveTime = 0;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Only bother parenting if touching the top of the collider
            if (!_rb.IsTouching(collision.collider, _aboveFilter))
            {
                return;
            }

            if (!_originalParents.ContainsKey(collision.transform))
            {
                _originalParents.Add(collision.transform, collision.transform.parent);
            }

            // Set our transform to their parent to avoid weird physics interactions
            collision.transform.parent = transform;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!_originalParents.ContainsKey(collision.transform))
            {
                return;
            }

            if (isActiveAndEnabled)
            {
                collision.transform.parent = _originalParents[collision.transform];
            }
        }

        private void OnDestroy()
        {
            _tiles.Remove(this);
        }
    }
}
