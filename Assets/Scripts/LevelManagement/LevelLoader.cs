using System.Collections.Generic;
using UnityEngine;

namespace CatGame.LevelManagement
{
    public class LevelLoader : MonoBehaviour
    {
        public Transform levelParent;
        public int loadDepth = 2;

        private const string LayerPrefabsPath = "Layers";
        private readonly Dictionary<string, Dictionary<int, GameObject>> layers = new();

        public void LoadLayer(string layerName) 
            => LoadLevel(layerName, 0);

        public void LoadLevel(LevelData data) 
            => LoadLevel(data.layerName, data.levelIndex);
        public void LoadLevel(string layerName, int levelIndex) 
            => LoadLevelRecursive(layerName, levelIndex, loadDepth, true);

        private void LoadLevelRecursive(string layerName, int levelIndex, int depth, bool setActive)
        {
            if (!layers.TryGetValue(layerName, out Dictionary<int, GameObject> levels))
            {
                levels = new Dictionary<int, GameObject>();
                layers[layerName] = levels;
            }

            if (!levels.TryGetValue(levelIndex, out GameObject level))
            {
                level = Resources.Load<GameObject>($"{LayerPrefabsPath}/{layerName}/{levelIndex}");
            }
            GameObject levelCopy = Instantiate(level);
            levelCopy.transform.parent = levelParent;
            levelCopy.SetActive(setActive);

            if (depth == 0)
            {
                return;
            }

            LevelData data = level.GetComponent<LevelData>();
            foreach (int connectedLevelIndex in data.connectedLevels)
            {
                LoadLevelRecursive(layerName, connectedLevelIndex, depth - 1, false);
            }
        }

        private void Start()
        {
            LoadLayer("Test");
        }
    }
}
