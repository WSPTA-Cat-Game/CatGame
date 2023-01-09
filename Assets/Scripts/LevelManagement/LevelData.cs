using UnityEngine;
using UnityEngine.Tilemaps;

namespace CatGame.LevelManagement
{
    public class LevelData : MonoBehaviour
    {
        public int index;
        public string layerName;

        public bool lockCameraX;
        public bool lockCameraY;
        public Vector2 lockedCameraPos;

        public Tilemap tilemap;
        public new CompositeCollider2D collider;
        public LevelTransition[] transitions;
        public Vector2 defaultSpawnPoint;

        public bool generateShadows;

        private void Awake()
        {
            tilemap.CompressBounds();

            if (generateShadows)
            {
                ShadowGenerator.GenerateShadowForLevel(this);
            }
        }
    }
}
