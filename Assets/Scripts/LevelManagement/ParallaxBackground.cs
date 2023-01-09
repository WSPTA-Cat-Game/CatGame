using UnityEngine;

namespace CatGame.LevelManagement
{
    public class ParallaxBackground : MonoBehaviour
    {
        public Sprite startImage;
        public Sprite endImage;
        public Sprite[] backgroundImages;

        [Range(0, 10)]
        public float distance;

        public float backgroundZ;

        private Vector3 lastCameraPos = Vector3.zero;

        private void Start()
        {
            // Generate enough children to cover width of camera
            float camHeight = Camera.main.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;

            // Loop till background width wide enough
            float backgroundWidth = 0;
            for (int i = 0; backgroundWidth < camWidth; i++)
            {
                GameObject backgroundGO = new($"Background");
                backgroundGO.transform.parent = transform;

                SpriteRenderer renderer = backgroundGO.AddComponent<SpriteRenderer>();
                renderer.sprite = (i == 0 && startImage != null) ? startImage : GetRandomSprite();

                // position it edge to edge of the last one (from the left of
                // the camera bounds)
                backgroundGO.transform.position = new Vector3(
                    backgroundWidth + (renderer.sprite.bounds.size.x / 2) - (camWidth / 2), 0);
                
                backgroundWidth += renderer.sprite.bounds.size.x;
            }
        }

        private void Update()
        {
            // Move relative to camera
            transform.position = new Vector3(
                transform.position.x + (Camera.main.transform.position.x - lastCameraPos.x) * (1 / (distance + 1)),
                Camera.main.transform.position.y);
            lastCameraPos = Camera.main.transform.position;

            // Check what are within bounds
            float camHeight = Camera.main.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;
            Bounds cameraBounds = new((Vector2)Camera.main.transform.position, new Vector3(camWidth, camHeight));
            cameraBounds.center = new Vector3(cameraBounds.center.x, cameraBounds.center.y, backgroundZ);
            Bounds extraWidthCamBounds = cameraBounds;
            extraWidthCamBounds.size += new Vector3(15, 0);

            // Remove anything outside of extra wide bounds
            float backgroundWidth = 0;
            foreach (Transform child in transform)
            {
                SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                if (!extraWidthCamBounds.Intersects(renderer.bounds))
                {
                    DestroyImmediate(child.gameObject);
                    continue;
                }

                backgroundWidth += renderer.bounds.size.x;
            }

            // Get bounds of the background
            Vector3 backgroundMin = transform.GetChild(0).GetComponent<SpriteRenderer>().bounds.min;
            Vector3 backgroundMax = transform.GetChild(transform.childCount - 1).GetComponent<SpriteRenderer>().bounds.max;
            Bounds backgroundBounds = new()
            {
                min = backgroundMin,
                max = backgroundMax
            };

            // Calculate how much width we need and whether to add it before or
            // after
            float targetWidth = backgroundBounds.size.x;
            SpriteRenderer lastRenderer;
            bool addBefore;
            if (backgroundBounds.min.x > cameraBounds.min.x)
            {
                targetWidth += backgroundBounds.min.x - cameraBounds.min.x;
                lastRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
                addBefore = true;
            }
            else if (backgroundBounds.max.x < cameraBounds.max.x)
            {
                targetWidth += cameraBounds.max.x - backgroundBounds.max.x;
                lastRenderer = transform.GetChild(transform.childCount - 1).GetComponent<SpriteRenderer>();
                addBefore = false;
            }
            else
            {
                // Return early since no change needed
                return;
            }

            // Loop and fill in until width is done
            for (int i = 0; backgroundWidth < targetWidth; i++)
            {
                GameObject backgroundGO = new($"Background");
                backgroundGO.transform.parent = transform;

                SpriteRenderer renderer = backgroundGO.AddComponent<SpriteRenderer>();
                renderer.sprite = GetRandomSprite();

                // position it edge to edge of the last one
                float xPos;
                if (addBefore)
                {
                    xPos = lastRenderer.bounds.min.x - renderer.bounds.extents.x;
                    backgroundGO.transform.SetAsFirstSibling();
                }
                else
                {
                    xPos = lastRenderer.bounds.max.x + renderer.bounds.extents.x;
                    backgroundGO.transform.SetAsLastSibling();
                }
                backgroundGO.transform.position = new Vector3(xPos, lastRenderer.transform.position.y, backgroundZ);

                backgroundWidth += renderer.bounds.size.x;
                lastRenderer = renderer;
            }
        }

        private Sprite GetRandomSprite()
        {
            return backgroundImages[Random.Range(0, backgroundImages.Length)];
        }
    }
}