using System;
using System.Collections;
using UnityEngine;

namespace CatGame.CameraControl
{
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        public new Collider2D collider;
        public Transform target;

        public bool lockX = false;
        public bool lockY = false;
        public Vector2 lockedPos = Vector2.zero; 
        public float lerpSpeed = 0;

        private Camera _camera;

        public void SetCollider(Collider2D collider, Action finishMovingCallback = null)
        {
            if (this.collider == collider)
            {
                finishMovingCallback?.Invoke();
                return;
            }

            this.collider = collider;

            Debug.Log("ASefkajse;flaksjed;fakjs;efkaj;sedrkfja;sldkfjasdf");
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
            if (collider == null)
            {
                return;
            }

            Vector2 worldLockedPos = collider.transform.TransformPoint(lockedPos);

            // Create bounds that represent what the cam sees of the collider
            float camHeight = _camera.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;
            Bounds camBounds = new(target.position, new Vector3(camWidth, camHeight, 1));
            
            Vector3 adjustedPos = camBounds.center;
            // Lock pos
            if (lockX)
            {
                adjustedPos.x = worldLockedPos.x;
            }
            else
            {
                // If the size of the bounds exceeds the collider, set the pos
                // to the center of the collider's bounds
                if (camBounds.size.x > collider.bounds.size.x)
                {
                    adjustedPos.x = collider.bounds.center.x;
                }
                // If the bounds is outside the collider, then set pos to edge 
                // of the collider accounting for bounds size
                else if (camBounds.min.x < collider.bounds.min.x)
                {
                    adjustedPos.x = collider.bounds.min.x + camBounds.extents.x;
                }
                else if (camBounds.max.x > collider.bounds.max.x)
                {
                    adjustedPos.x = collider.bounds.max.x - camBounds.extents.x;
                }
            }


            // Repeat for y
            if (lockY)
            {
                adjustedPos.y = worldLockedPos.y;
            }
            else
            {
                if (camBounds.size.y > collider.bounds.size.y)
                {
                    adjustedPos.y = collider.bounds.center.y;
                }
                else if (camBounds.min.y < collider.bounds.min.y)
                {
                    adjustedPos.y = collider.bounds.min.y + camBounds.extents.y;
                }
                else if (camBounds.max.y > collider.bounds.max.y)
                {
                    adjustedPos.y = collider.bounds.max.y - camBounds.extents.y;
                }
            }


            // Unscaled delta time is delta time without time scale, allowing
            // the camera to move while time scale is 0
            adjustedPos.z = -10;
            Debug.Log(adjustedPos.ToString("N6"));
            _camera.transform.position = Vector3.Lerp(
                _camera.transform.position,
                adjustedPos, 
                lerpSpeed * 60 * Time.unscaledDeltaTime
            );
        }
    }
}
