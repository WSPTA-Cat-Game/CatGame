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
            // Interpolate
            Vector3 finalPos = Vector3.Lerp(camera.transform.position, target.position, lerpSpeed * 60 * Time.deltaTime);

            // Create bounds that represent what the cam sees of the tilemap
            float camHeight = camera.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;

            Vector3 boundsSize = tilemap.WorldToLocal(new Vector3(camWidth, camHeight));
            boundsSize.z = 1;
            Vector3 boundsPos = tilemap.WorldToLocal(finalPos);
            Bounds bounds = new(boundsPos, boundsSize);

            Vector3 adjustedPos = boundsPos;
            Vector3 tilemapLocalLockedPos = tilemap.WorldToLocal(lockedPos);
            // Lock axis
            if (lockX)
            {
                adjustedPos.x = tilemapLocalLockedPos.x;
            }
            else
            {
                // If the size of the bounds exceeds the tilemap, set the pos
                // to the center of the tilemap's bounds
                if (bounds.size.x > tilemap.localBounds.size.x)
                {
                    adjustedPos.x = tilemap.localBounds.center.x;
                }
                // If the bounds is outside the tilemap, then set pos to edge 
                // of the tilemap accounting for bounds size
                else if (bounds.min.x < tilemap.localBounds.min.x)
                {
                    adjustedPos.x = tilemap.localBounds.min.x + bounds.extents.x;
                }
                else if (bounds.max.x > tilemap.localBounds.max.x)
                {
                    adjustedPos.x = tilemap.localBounds.max.x - bounds.extents.x;
                }
            }

            if (lockY)
            {
                adjustedPos.y = tilemapLocalLockedPos.y;
            }
            else
            {
                if (bounds.size.y > tilemap.localBounds.size.y)
                {
                    adjustedPos.y = tilemap.localBounds.center.y;
                }
                else if (bounds.min.y < tilemap.localBounds.min.y)
                {
                    adjustedPos.y = tilemap.localBounds.min.y + bounds.extents.y;
                }
                else if (bounds.max.y > tilemap.localBounds.max.y)
                {
                    adjustedPos.y = tilemap.localBounds.max.y - bounds.extents.y;
                }
            }

            // Set final pos to adjusted pos
            finalPos = tilemap.LocalToWorld(adjustedPos);
            finalPos.z = -10;


            camera.transform.position = finalPos;
        }
    }
}
