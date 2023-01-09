using System;
using UnityEngine;

namespace CatGame.LevelManagement
{
    public class ParallaxBackground : MonoBehaviour
    {
        public Sprite startImage;
        public Sprite[] backgroundImages;

        [Range(0, 10)]
        public float distance;

        public float backgroundZ;

        private Vector3 lastCameraPos = Vector3.zero;

        // Use pseudo-random so it's reversable
        private static readonly int[] randomSeeds = new int[] { 1274953534, 10968400, 1421919999, 888764721, 123520471, 1803020880, 1168632243, 667162517, 2060686023, 1870240000, 898748342, 1184724733, 473665368, 1564163596, 139958020, 155857049, 296254423, 1553641499, 1861771837, 182760313, 1539209165, 598865036, 1947029237, 1374986690, 270860160, 1238357919, 2013100581, 409591901, 1790701108, 92433383, 1479404271, 358536751, 1559156373, 1093775371, 2042893907, 431367349, 21440794, 2024309168, 1928172286, 405955844, 310317885, 1231701309, 1914455511, 1686510817, 33756832, 35961415, 1581721814, 501443438, 815747126, 1202338088, 913672990, 2055295735, 1081955584, 208763969, 2137286633, 922484526, 661740911, 762391977, 971677360, 1574367145, 21740511, 1058051642, 1449778013, 465268577, 1470422577, 823182841, 1464308502, 1673440506, 215503373, 1422902349, 1035966806, 1781933104, 822440721, 1578555846, 545030819, 810936017, 764862067, 65932765, 1134820438, 857910326, 1765179526, 1328957114, 1615341579, 213378849, 166246141, 1913006661, 1495413578, 460798013, 172171572, 1385904560, 782274869, 1249964624, 252220780, 1373502997, 370527756, 1064740267, 1772545168, 711876810, 1067760476, 368399976, 1314922188, 81191818, 375137927, 2092639502, 1729733731, 1478599661, 743612571, 1185801697, 2065935370, 834413659, 1409665717, 358213017, 1159076451, 1543809367, 761448324, 190350581, 1086030464, 749685423, 262889104, 1424533726, 1740732075, 969248973, 968601301, 1714528164, 1483644334, 273400702, 2081496830, 1447146505, 803601988, 1108541103, 325078381, 841970124, 68440491, 497467420, 1498139355, 1313129490, 1262292952, 923083367, 1122031684, 1431823170, 2000331907, 1684610515, 281333911, 34370406, 327383039, 533103248, 681017241, 43358124, 1500553600, 206383730, 1485287011, 329354717, 437427629, 755606020, 1964826237, 167126803, 684888232, 291763602, 44801204, 1654090491, 1091274970, 110123155, 1169750252, 1653469154, 94086890, 599177252, 1998128304, 163199177, 1942303082, 352457258, 777055906, 37034425, 1675155913, 1822360002, 467917385, 47712320, 1624295306, 1940290802, 652776322, 1675976798, 893064856, 2112172024, 3736652, 142973217, 414185431, 628001629, 50953402, 367610602, 621428852, 901266201, 1511790207, 108885092, 2037256723, 493774799, 1204228822, 20285805, 1061599923, 607708271, 533753173, 465297143 };
        private int lowerIndex = 0;
        private int upperIndex = -1;

        private void Start()
        {
            // Generate enough children to cover width of camera
            float camHeight = Camera.main.orthographicSize * 2;
            float camWidth = camHeight * Screen.width / Screen.height;
            float camLeft = Camera.main.transform.position.x - (camWidth / 2);

            // Loop till background width wide enough
            float backgroundWidth = 0;
            for (int i = 0; backgroundWidth < camWidth; i++)
            {
                GameObject backgroundGO = new($"Background");
                backgroundGO.transform.parent = transform;

                SpriteRenderer renderer = backgroundGO.AddComponent<SpriteRenderer>();
                renderer.sprite = GetRandomSprite(forceStart: i == 0);

                // position it edge to edge of the last one (from the left of
                // the camera bounds)
                backgroundGO.transform.position = new Vector3(
                     camLeft + backgroundWidth + (renderer.sprite.bounds.size.x / 2),
                     0,
                     backgroundZ);

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
            Bounds cameraBounds = new(
                new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y, backgroundZ),
                new Vector3(camWidth, camHeight));

            Bounds extendedCamBounds = cameraBounds;
            extendedCamBounds.size += new Vector3(15, 0);

            // Remove anything outside of extra wide bounds
            float backgroundWidth = 0;
            foreach (Transform child in transform)
            {
                SpriteRenderer renderer = child.GetComponent<SpriteRenderer>();
                if (!extendedCamBounds.Intersects(renderer.bounds))
                {
                    if (renderer.bounds.min.x < cameraBounds.min.x)
                    {
                        lowerIndex++;
                    }
                    else
                    {
                        upperIndex--;
                    }

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
                renderer.sprite = GetRandomSprite(!addBefore);

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

        private Sprite GetRandomSprite(bool forward = true, bool forceStart = false)
        {
            int randomIndex;
            if (forward)
            {
                randomIndex = ++upperIndex;
            }
            else
            {
                randomIndex = --lowerIndex;
            }

            if (randomIndex == 0 || forceStart)
            {
                return startImage;
            }

            double rand = (double)randomSeeds[Math.Abs(randomIndex % randomSeeds.Length)] / int.MaxValue;
            return backgroundImages[(int)(rand * backgroundImages.Length)];
        }
    }
}