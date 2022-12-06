using UnityEngine;
using UnityEngine.Tilemaps;

namespace CatGame.LevelManagement
{
    public class LevelData : MonoBehaviour
    {
        public int index;
        public string layerName;

        public Tilemap tilemap;
        public LevelTransition[] transitions;
        public Vector2 defaultSpawnPoint;
    }
}
