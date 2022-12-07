using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CatGame.CameraControl
{
    [RequireComponent(typeof(Camera))]
    public class FollowCamera : MonoBehaviour
    {
        public Tilemap tilemap;
        public Transform target;

        public bool lockX = false;
        public bool lockY = false;
        public Vector2 lockedPos = Vector2.zero; 
        public float lerpSpeed = 0;

        private new Camera camera;

        public void SetTilemap(Tilemap tilemap, Action finishMovingCallback = null)
        {
            if (this.tilemap == tilemap)
            {
                finishMovingCallback?.Invoke();
                return;
            }

            this.tilemap = tilemap;

            if (finishMovingCallback != null)
            {
                StartCoroutine(SetTilemapCoroutine(finishMovingCallback));
            }
        }

        private IEnumerator SetTilemapCoroutine(Action callback)
        {
            // Wait till camera actually starts moving
            yield return null;

            // Keep running while camera is moving
            while (camera.velocity.magnitude > 0.001)
            {
                yield return null;
            }

            callback?.Invoke();
        }

        private void Awake()
        {
            camera = GetComponent<Camera>();
        }

        private void Update()
        {
            // Get target pos
            Vector3 targetPos = target.position;

            // Create bounds that represent what the cam sees of the tilemap
            float camHeight = camera.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;

            Vector3 boundsSize = tilemap.transform.InverseTransformVector(new Vector3(camWidth, camHeight));
            boundsSize.z = 1;
            Vector3 boundsPos = tilemap.WorldToLocal(targetPos);
            Bounds camBounds = new(boundsPos, boundsSize);
            
            Vector3 adjustedPos = boundsPos;
            // Lock pos
            if (lockX)
            {
                adjustedPos.x = lockedPos.x;
            }
            else
            {
                // If the size of the bounds exceeds the tilemap, set the pos
                // to the center of the tilemap's bounds
                if (camBounds.size.x > tilemap.localBounds.size.x)
                {
                    adjustedPos.x = tilemap.localBounds.center.x;
                }
                // If the bounds is outside the tilemap, then set pos to edge 
                // of the tilemap accounting for bounds size
                else if (camBounds.min.x < tilemap.localBounds.min.x)
                {
                    adjustedPos.x = tilemap.localBounds.min.x + camBounds.extents.x;
                }
                else if (camBounds.max.x > tilemap.localBounds.max.x)
                {
                    adjustedPos.x = tilemap.localBounds.max.x - camBounds.extents.x;
                }
            }


            // Repeat for y
            if (lockY)
            {
                adjustedPos.y = lockedPos.y;
            }
            else
            {
                if (camBounds.size.y > tilemap.localBounds.size.y)
                {
                    adjustedPos.y = tilemap.localBounds.center.y;
                }
                else if (camBounds.min.y < tilemap.localBounds.min.y)
                {
                    adjustedPos.y = tilemap.localBounds.min.y + camBounds.extents.y;
                }
                else if (camBounds.max.y > tilemap.localBounds.max.y)
                {
                    adjustedPos.y = tilemap.localBounds.max.y - camBounds.extents.y;
                }
            }

            // Convert pos back to world space
            adjustedPos = tilemap.LocalToWorld(adjustedPos);
            adjustedPos.z = -10;

            // Unscaled delta time is delta time without time scale, allowing
            // the camera to move while time scale is 0
            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                adjustedPos, 
                lerpSpeed * 60 * Time.unscaledDeltaTime
            );
        }
    }
}
