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

        private void Start()
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
            Vector3 tilemapLockedPos = tilemap.WorldToLocal(lockedPos);
            // Lock pos
            if (lockX)
            {
                adjustedPos.x = tilemapLockedPos.x;
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
                adjustedPos.y = tilemapLockedPos.y;
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

            camera.transform.position = Vector3.Lerp(
                camera.transform.position,
                adjustedPos, 
                lerpSpeed * 60 * Time.deltaTime
            );
        }
    }
}
