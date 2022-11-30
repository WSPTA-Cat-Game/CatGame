using UnityEngine;

namespace CatGame.LevelManagement
{
    public class LevelData : MonoBehaviour
    {
        public int levelIndex;
        public string layerName;

        public int[] connectedLevels;
    }
}
