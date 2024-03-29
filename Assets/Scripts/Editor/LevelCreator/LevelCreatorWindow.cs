﻿using CatGame.LevelManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CatGame.Editor.LevelCreator
{
    public class LevelCreatorWindow : EditorWindow
    {
        internal string _currentLoadedLayer;

        private readonly Dictionary<string, bool> _foldoutStates = new();
        private Vector2 _scrollViewPosition = new();

        private int _currentTabIndex;
        private Grid _grid;

        private string _newLevelLayerName;

        public static string ProjectPath 
        { 
            get => Path.GetFullPath(Path.Combine(Application.dataPath, "../"));
        }

        public static string LayersDirectory
        {
            get => Path.Combine(ProjectPath, LayersAssetsPath);
        }
        public static string LayersAssetsPath
        {
            get => EditorPrefs.GetString("CatGame_LayersAssetsPath");
            private set => EditorPrefs.SetString("CatGame_LayersAssetsPath", value);
        }

        public static string LevelElementsDirectory
        {
            get => Path.Combine(ProjectPath, LevelElementsAssetsPath);
        }
        public static string LevelElementsAssetsPath
        {
            get => EditorPrefs.GetString("CatGame_LevelElementsAssetsPath");
            private set => EditorPrefs.SetString("CatGame_LevelElementsAssetsPath", value);
        }

        [MenuItem("Cat Game/Level Creator")]
        private static void ShowWindow()
        {
            GetWindow<LevelCreatorWindow>();
        }

        private void CreateGUI()
        {
            hideFlags = HideFlags.HideAndDontSave;

            Grid[] gridsInScene = FindObjectsOfType<Grid>(true);
            if (gridsInScene.Length == 1)
            {
                _grid = gridsInScene[0];
            }

            // Set default value of layersDirectory and level elements dir
            string tempLayersDirectory = Path.Combine(ProjectPath, @"Assets\Prefabs\Resources\Layers");
            if (Directory.Exists(tempLayersDirectory) && string.IsNullOrWhiteSpace(LayersAssetsPath))
            {
                LayersAssetsPath = @"Assets\Prefabs\Resources\Layers";
            }

            string tempElementsDirectory = Path.Combine(ProjectPath, @"Assets\Prefabs\Level Elements");
            if (Directory.Exists(tempElementsDirectory) && string.IsNullOrWhiteSpace(LevelElementsAssetsPath))
            {
                LevelElementsAssetsPath = @"Assets\Prefabs\Level Elements";
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStart;
        }

        private void OnGUI()
        {
            // Don't run in play mode
            if (EditorApplication.isPlaying)
            {
                EditorGUILayout.LabelField("Exit play mode to edit levels");
                return;
            }

            // Toolbar
            _currentTabIndex = GUILayout.Toolbar(_currentTabIndex, new string[] { "Options", "Edit", "New" });

            if (_currentTabIndex == 0)
            {
                OnSettings();
            }
            else
            {
                // Disable edit and new tabs if settings aren't completed
                bool settingsComplete = Directory.Exists(LayersDirectory);
                if (!settingsComplete)
                {
                    EditorGUILayout.LabelField("Please complete the settings page!");
                }
                
                using (new EditorGUI.DisabledGroupScope(!settingsComplete))
                {
                    switch (_currentTabIndex)
                    {
                        case 1:
                            OnEdit();
                            break;
                        case 2:
                            OnNew();
                            break;
                    }
                }
            }
        }

        private void OnSettings()
        {
            // Grid parent
            _grid = (Grid)EditorGUILayout.ObjectField("Grid parent:", _grid, typeof(Grid), true);

            // Layers dir path
            EditorGUILayout.LabelField("Layers folder path:", LayersAssetsPath);
            if (GUILayout.Button("Edit path"))
            {
                string fullPath = EditorUtility.OpenFolderPanel(
                    "Select layers directory",
                    Application.dataPath,
                    LayersDirectory);
                LayersAssetsPath = string.IsNullOrWhiteSpace(fullPath) 
                    ? "" 
                    : Path.GetRelativePath(ProjectPath, fullPath);
            }

            // Level elements dir path
            EditorGUILayout.LabelField("Level elements folder path:", LevelElementsAssetsPath);
            if (GUILayout.Button("Edit path"))
            {
                string fullPath = EditorUtility.OpenFolderPanel(
                    "Select level elements directory",
                    Application.dataPath,
                    LevelElementsDirectory);
                LevelElementsAssetsPath = string.IsNullOrWhiteSpace(fullPath) 
                    ? ""
                    : Path.GetRelativePath(ProjectPath, fullPath);
            }
        }

        private void OnEdit()
        {
            // Create scroll view
            using EditorGUILayout.ScrollViewScope scrollScope = new(_scrollViewPosition);

            // Make the layer drop downs
            foreach (string layerDirectory in Directory.GetDirectories(LayersDirectory))
            {
                string layerName = Path.GetFileName(layerDirectory);

                _scrollViewPosition = scrollScope.scrollPosition;

                // Create foldout
                if (!_foldoutStates.TryGetValue(layerName, out bool foldout))
                {
                    foldout = false;
                }

                _foldoutStates[layerName] = EditorGUILayout.BeginFoldoutHeaderGroup(foldout, layerName);
                if (_foldoutStates[layerName])
                {
                    // Populate dropdowns with level
                    PopulateLayerFoldout(layerName);
                }
                EditorGUILayout.EndFoldoutHeaderGroup();
            }
        }

        private void OnNew()
        {
            // Layer name dropdown
            // Options will be the layer directories
            string[] options = Directory.GetDirectories(LayersDirectory)
                .Select(dir => Path.GetFileName(dir)).ToArray();

            // Grab index of the current option
            int optionsIndex = Array.IndexOf(options, _newLevelLayerName);
            optionsIndex = optionsIndex == -1 ? 0 : optionsIndex;

            // Get new layer directory
            int newOptionsIndex = EditorGUILayout.Popup("Layer", optionsIndex, options);
            _newLevelLayerName = options[newOptionsIndex];

            // Level index
            int levelIndex = GetLevelPrefabs(_newLevelLayerName)
                .Select(path => int.Parse(Path.GetFileNameWithoutExtension(path)))
                .Max() + 1;


            // Bottom bits
            EditorGUILayout.Space(50);
            if (GUILayout.Button("Create"))
            {
                GameObject levelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(LevelElementsAssetsPath, "Level.prefab"));

                GameObject levelCopy = Instantiate(levelPrefab);
                // Hide because this is just a temp object
                levelCopy.hideFlags = HideFlags.HideInHierarchy;
                LevelData data = levelCopy.GetComponent<LevelData>();
                data.layerName = _newLevelLayerName;
                data.index = levelIndex;

                // Save as a prefab so that it can be loaded by the level loader
                if (Common.SaveLevelAsPrefab(data))
                {
                    // Finally load the layer
                    LoadLayerAndSelect(data);
                }

                // Destroy temp object                
                DestroyImmediate(levelCopy);
            }
        }

        private void PopulateLayerFoldout(string layerName)
        {
            foreach (string levelPrefabPath in GetLevelPrefabs(layerName))
            {
                using(new EditorGUILayout.HorizontalScope())
                {
                    // Get image
                    string prefabAssetPath = Path.GetRelativePath(ProjectPath, levelPrefabPath);
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                    GUILayout.Box(AssetPreview.GetAssetPreview(prefab));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.LabelField("Level Index:", prefab.GetComponent<LevelData>().index.ToString());

                        // Buttons
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            if (GUILayout.Button("Edit"))
                            {
                                LevelData data = prefab.GetComponent<LevelData>();

                                // Set inspector's current object as our prefab
                                LoadLayerAndSelect(data);
                            }
                            if (GUILayout.Button("Delete")
                                && EditorUtility.DisplayDialog("Are you sure?", "Are you sure you want to delete this level?", "Yes", "No"))
                            {
                                AssetDatabase.DeleteAsset(prefabAssetPath);
                            }
                        }
                    }
                }
            }
        }

        private LevelData LoadLayerAndSelect(LevelData data)
        {
            // Check to see if we're loading into a diff layer
            if (_currentLoadedLayer != data.layerName)
            {
                // If we're loading from nothing, then go ahead, else ask user
                if (string.IsNullOrWhiteSpace(_currentLoadedLayer)
                    || EditorUtility.DisplayDialog("Are you sure?", "Loading a new layer will destroy all open levels. Make sure to save any changes you don't want lost.", "Yes", "No"))
                {
                    // Clear out the already existing levels
                    for (int i = _grid.transform.childCount - 1; i >= 0; i--)
                    {
                        DestroyImmediate(_grid.transform.GetChild(i).gameObject);
                    }

                    // Load global data
                    string globalPath = Path.GetRelativePath(
                        ProjectPath, 
                        Path.Combine(LayersAssetsPath, data.layerName, "Global.prefab"));
                    GameObject globalPrefabPath = AssetDatabase.LoadAssetAtPath<GameObject>(globalPath);
                    GameObject globalPrefab = (GameObject)PrefabUtility.InstantiatePrefab(globalPrefabPath);
                    globalPrefab.transform.parent = _grid.transform;

                    // Load all levels in layer
                    LevelData[] copies = LoadLevels(GetLevelPrefabs(data.layerName)
                        .Select(path =>
                        {
                            string prefabAssetPath = Path.GetRelativePath(ProjectPath, path);
                            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabAssetPath);
                            return prefab.GetComponent<LevelData>();
                        }).ToArray());

                    LevelData copyData = copies.First(val => val.index == data.index);
                    
                    Selection.activeObject = copyData;
                    _currentLoadedLayer = data.layerName;

                    return copyData;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // Check if the level is already loaded
                // Can't really trust the loader cache (since it's not persistent)
                foreach (Transform child in _grid.transform)
                {
                    LevelData childData = child.GetComponent<LevelData>();
                    if (childData != null
                        && childData.layerName == data.layerName
                        && childData.index == data.index)
                    {
                        Selection.activeObject = childData;
                        return childData;
                    }
                }

                // If it's not loaded, then load it ourselves
                LevelData copyData = LoadLevels(data)[0];
                Selection.activeObject = copyData;
                _currentLoadedLayer = data.layerName;

                return copyData;
            }
        }

        private LevelData[] LoadLevels(params LevelData[] levels)
        {
            LevelData[] copys = new LevelData[levels.Length];

            for (int i = 0; i < levels.Length; i++)
            {
                LevelData prefab = (LevelData)PrefabUtility.InstantiatePrefab(levels[i]);
                PrefabUtility.UnpackPrefabInstance(prefab.gameObject, PrefabUnpackMode.OutermostRoot, InteractionMode.AutomatedAction);

                // Add temp composite collider and rigidbody to make level 
                // boundaries easier to see
                prefab.gameObject.AddComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
                prefab.gameObject.AddComponent<CompositeCollider2D>();

                prefab.transform.parent = _grid.transform;

                copys[i] = prefab;
            }

            return copys;
        }

        private void OnPlayModeStart(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            // Destroy temp composite collider and rigidbody
            foreach (Transform child in _grid.transform)
            {
                if (child.GetComponent<LevelData>() != null)
                {
                    DestroyImmediate(child.GetComponent<CompositeCollider2D>());
                    DestroyImmediate(child.GetComponent<Rigidbody2D>());
                }
            }
        }

        private IEnumerable<string> GetLevelPrefabs(string layerName)
            => Directory.GetFiles(Path.Combine(LayersDirectory, layerName))
                .Where(path => !path.EndsWith(".meta") && Path.GetFileName(path) != "Global.prefab");
    }
}