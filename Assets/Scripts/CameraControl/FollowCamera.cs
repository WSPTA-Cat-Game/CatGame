using System;
using System.Collections;
using UnityEngine;

namespace CatGame.CameraControl
{
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        public Bounds bounds;
        public Transform target;

        public bool lockX = false;
        public bool lockY = false;
        public Vector2 lockedPos = Vector2.zero; 
        public float lerpSpeed = 0;

        private Camera _camera;

        public void SetBounds(Bounds bounds, Action finishMovingCallback = null)
        {
            if (this.bounds == bounds)
            {
                finishMovingCallback?.Invoke();
                return;
            }

            this.bounds = bounds;

            StartCoroutine(SetTilemapCoroutine(finishMovingCallback));
        }

        private IEnumerator SetTilemapCoroutine(Action callback)
        {
            // Wait till camera actually starts moving
            yield return null;

            // Keep running while camera is moving
            while (_camera.velocity.magnitude > 0.001)
            {
                yield return null;
            }

            callback?.Invoke();
        }

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Update()
        {
            // Create bounds that represent what the cam sees of the collider
            float camHeight = _camera.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;
            Bounds camBounds = new(target.position, new Vector3(camWidth, camHeight, 1));
            
            Vector3 adjustedPos = camBounds.center;
            // Lock pos
            if (lockX)
            {
                adjustedPos.x = lockedPos.x;
            }
            else
            {
                // If the size of the bounds exceeds the collider, set the pos
                // to the center of the collider's bounds
                if (camBounds.size.x > bounds.size.x)
                {
                    adjustedPos.x = bounds.center.x;
                }
                // If the bounds is outside the collider, then set pos to edge 
                // of the collider accounting for bounds size
                else if (camBounds.min.x < bounds.min.x)
                {
                    adjustedPos.x = bounds.min.x + camBounds.extents.x;
                }
                else if (camBounds.max.x > bounds.max.x)
                {
                    adjustedPos.x = bounds.max.x - camBounds.extents.x;
                }
            }


            // Repeat for y
            if (lockY)
            {
                adjustedPos.y = lockedPos.y - 0.25f;
            }
            else
            {
                if (camBounds.size.y > bounds.size.y)
                {
                    adjustedPos.y = bounds.center.y;
                }
                else if (camBounds.min.y < bounds.min.y)
                {
                    adjustedPos.y = bounds.min.y + camBounds.extents.y;
                }
                else if (camBounds.max.y > bounds.max.y)
                {
                    adjustedPos.y = bounds.max.y - camBounds.extents.y;
                }
            }


            // Unscaled delta time is delta time without time scale, allowing
            // the camera to move while time scale is 0
            adjustedPos.z = -10;
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                adjustedPos, 
                lerpSpeed * 60 * Time.unscaledDeltaTime
            );
        }
    }
}
