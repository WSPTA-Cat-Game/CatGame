﻿using System.Collections.Generic;
using UnityEngine;

namespace CatGame.LevelManagement
{
    public class LevelLoader : MonoBehaviour
    {
        private const string LayerPrefabsPath = "Layers";
        
        public Transform levelParent;

        private readonly Dictionary<string, Dictionary<int, LevelData>> _layers = new();

        private HashSet<LevelData> _lastLoaded = new();

        public void ResetCache()
        {
            _layers.Clear();
            _lastLoaded.Clear();
        }

        public LevelData LoadLevel(string layerName, int levelIndex, int loadDepth = 2, bool autoUnload = true)
        {
#if UNITY_EDITOR
            // Yet another workaround for Unity clearing dictionaries on
            // rebuild
            foreach (Transform child in levelParent)
            {
                LevelData data = child.GetComponent<LevelData>();

                _lastLoaded.Add(data);
                if (!_layers.TryGetValue(data.layerName, out Dictionary<int, LevelData> layer))
                {
                    layer = new Dictionary<int, LevelData>();
                    _layers[data.layerName] = layer;
                }

                _layers[data.layerName][data.index] = data;
            }
#endif

            HashSet<LevelData> cachedLevels = new();
            // Actually load level
            // Yes I realize that ref is redundant. It's mostly there to make
            // the code clearer.
            LevelData loadedLevel = LoadLevelRecursive(layerName, levelIndex, loadDepth, ref cachedLevels);

            if (autoUnload)
            {
                // Remove any levels that weren't just loaded
                foreach (LevelData cachedLevel in _lastLoaded)
                {
                    if (cachedLevels.Contains(cachedLevel))
                    {
                        continue;
                    }

                    if (!_layers.ContainsKey(cachedLevel.layerName))
                    {
                        continue;
                    }
                    
                    _layers[cachedLevel.layerName].Remove(cachedLevel.index);
                    if (_layers[cachedLevel.layerName].Count == 0)
                    {
                        _layers.Remove(cachedLevel.layerName);
                    }

                    Destroy(cachedLevel.gameObject);
                }
            }

            _lastLoaded = cachedLevels;
            return loadedLevel;
        }

        private LevelData LoadLevelRecursive(string layerName, int levelIndex, int depth, ref HashSet<LevelData> cachedLevels)
        {
            // Try get layer dictionary or add it if it doesn't exist
            if (!_layers.TryGetValue(layerName, out Dictionary<int, LevelData> levels))
            {
                levels = new Dictionary<int, LevelData>();
                _layers[layerName] = levels;
            }

            // Try get level or initialize it if it hasn't been cached
            if (!levels.TryGetValue(levelIndex, out LevelData level))
            {
                GameObject go = Resources.Load<GameObject>($"{LayerPrefabsPath}/{layerName}/{levelIndex}");
                GameObject levelCopy = Instantiate(go);
                
                level = levelCopy.GetComponent<LevelData>();
                level.transform.parent = levelParent;
                level.gameObject.SetActive(true);

                levels[levelIndex] = level;
            }

            cachedLevels.Add(level);

            // Recursion exit condition
            if (depth == 0)
            {
                return level;
            }

            // Recurse all connected levels
            foreach (LevelTransition levelTransition in level.transitions)
            {
                LoadLevelRecursive(layerName, levelTransition.nextLevelIndex, depth - 1, ref cachedLevels);
            }

            return level;
        }
    }
}
