using System.Collections.Generic;
using UnityEngine;

namespace CatGame.MovingTiles
{
    public class MovingTile : MonoBehaviour
    {
        private static readonly Dictionary<Transform, Transform> originalParents = new();

        public Vector3 endPos;

        public float speed = 1.5f;
        public float stopTime = 1f;

        private Vector3 startPos;

        private float moveTime;
        private float endTime;
        private bool isMoving;
        private bool isReversed = false;
        
        protected virtual void Start()
        {
            startPos = transform.localPosition;
        }
        
        protected virtual void Update() 
        {
            Vector3 currentStartPos = startPos;
            Vector3 currentEndPos = endPos;

            if (isReversed)
            {
                currentStartPos = endPos;
                currentEndPos = startPos;
            }

            if (isMoving)
            {
                moveTime += Time.deltaTime;

                Vector3 direction = currentEndPos - currentStartPos;
                float timeToMove = direction.magnitude / speed;
                
                Vector3 resPos = currentStartPos
                    + moveTime * speed * direction.normalized;


                // Check if we've gone longer than the length
                if (moveTime > timeToMove)
                {
                    transform.localPosition = currentEndPos;
                    isMoving = false;
                    endTime = 0;
                    isReversed = !isReversed;
                }
                else
                {
                    transform.localPosition = resPos;
                }
            }
            else
            {
                // Wait until stop time is over then move again
                endTime += Time.deltaTime;

                if (endTime > stopTime)
                {
                    isMoving = true;
                    moveTime = 0;
                }
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!originalParents.ContainsKey(collision.transform))
            {
                originalParents.Add(collision.transform, collision.transform.parent);
            }

            // Set our transform to theirs to avoid weird physics interactions
            collision.transform.parent = transform;
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            collision.transform.parent = originalParents.GetValueOrDefault(collision.transform, null);
        }
    }
}
