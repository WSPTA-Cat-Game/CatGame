using CatGame.CharacterControl;
using CatGame.LevelManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace CatGame.Editor.LevelCreator
{
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : UnityEditor.Editor
    {
        private bool _isEditingDefaultSpawn = false;
        private bool _isTransitionOpen = true;
        private bool _isEditingCamera = false;

        private SerializedProperty _layerNameProp;
        private SerializedProperty _levelIndexProp;
        private SerializedProperty _defaultSpawnPointProp;
        private SerializedProperty _lockCameraXProp;
        private SerializedProperty _lockCameraYProp;
        private SerializedProperty _lockedCameraPosProp;
        private SerializedProperty _transitionsProp;
        private SerializedProperty _generateShadowsProp;

        private ReorderableList _transitionsList;

        private LevelData Data => target as LevelData;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Layer name dropdown
            // Options will be the layer directories
            string[] options = Directory.GetDirectories(LevelCreatorWindow.LayersDirectory)
                .Select(dir => Path.GetFileName(dir)).ToArray();

            // Grab index of the current option
            int optionsIndex = Array.IndexOf(options, _layerNameProp.stringValue);
            optionsIndex = optionsIndex == -1 ? 0 : optionsIndex;

            // Get new layer directory
            int newOptionsIndex = EditorGUILayout.Popup("Layer", optionsIndex, options);
            _layerNameProp.stringValue = options[newOptionsIndex];

            // Level index
            EditorGUILayout.PropertyField(_levelIndexProp, new GUIContent("Level Index"));

            // Default spawn
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_defaultSpawnPointProp, new GUIContent("Default Spawn Point"));
                if (GUILayout.Button(_isEditingDefaultSpawn ? "Return" : "Edit", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                {
                    _isEditingDefaultSpawn = !_isEditingDefaultSpawn;
                }
            }

            // Camera lock
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_lockCameraXProp, new GUIContent("Lock Camera X"));
                using (new EditorGUI.DisabledGroupScope(!_lockCameraXProp.boolValue))
                {
                    _lockedCameraPosProp.vector2Value = new Vector2(
                        EditorGUILayout.FloatField("Camera X", _lockedCameraPosProp.vector2Value.x),
                        _lockedCameraPosProp.vector2Value.y);
                }
            }

            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.PropertyField(_lockCameraYProp, new GUIContent("Lock Camera Y"));
                using (new EditorGUI.DisabledGroupScope(!_lockCameraYProp.boolValue))
                {
                    _lockedCameraPosProp.vector2Value = new Vector2(
                        _lockedCameraPosProp.vector2Value.x,
                        EditorGUILayout.FloatField("Camera Y", _lockedCameraPosProp.vector2Value.y));
                }
            }

            if (GUILayout.Button(_isEditingCamera ? "Return" : "Edit Camera Pos"))
            {
                _isEditingCamera = !_isEditingCamera;
            }

            // Generate shadows
            EditorGUILayout.PropertyField(_generateShadowsProp);

            // Transitions
            _isTransitionOpen = EditorGUILayout.BeginFoldoutHeaderGroup(_isTransitionOpen, "Transitions");
            if (_isTransitionOpen)
            {
                ValidateTransitionsList();
                _transitionsList.DoLayoutList();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            // Bottom elements
            GUILayout.Space(50);

            // Test Button
            if (GUILayout.Button("Test"))
            {
                EditorApplication.EnterPlaymode();
            }

            // Save buttons
            if (GUILayout.Button("Save"))
            {
                Common.SaveLevelAsPrefab(Data);
            }

            if (GUILayout.Button("Save All"))
            {
                foreach (Transform level in Data.transform.parent)
                {
                    LevelData levelData = level.GetComponent<LevelData>();
                    if (levelData != null)
                    {
                        Common.SaveLevelAsPrefab(levelData);
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void ValidateTransitionsList()
        {
            Transform TransitionParent = Data.transform.Find("Transitions");

            if (TransitionParent.childCount == _transitionsList.count)
            {
                return;
            }

            // Get hashset of all transitions in parent
            HashSet<LevelTransition> transitionsInParent = TransitionParent
                .Cast<Transform>()
                .Select(transform => transform.GetComponent<LevelTransition>()).ToHashSet();

            // Loop through transitions in list and remove anything not in
            // the hashset
            for (int i = _transitionsList.count - 1; i >= 0; i--)
            {
                SerializedProperty transition = _transitionsList.serializedProperty.GetArrayElementAtIndex(i);

                if (transitionsInParent.Remove((LevelTransition)transition.objectReferenceValue))
                {
                    continue;
                }

                _transitionsList.serializedProperty.DeleteArrayElementAtIndex(i);
            }

            // Loop through remaining items in hashset to remove any extra
            // transitions in parent
            foreach (LevelTransition transition in transitionsInParent)
            {
                DestroyImmediate(transition.gameObject);
            }
        }

        private void OnSceneGUI()
        {
            // Default spawn point handle
            if (_isEditingDefaultSpawn)
            {
                // Change check for undo
                using EditorGUI.ChangeCheckScope changeScope = new();

                // Draw position handle
                Vector2 newDefaultSpawn = Handles.PositionHandle(
                    Data.transform.TransformPoint(Data.defaultSpawnPoint),
                    Quaternion.identity);

                // Snap to grid
                newDefaultSpawn = new Vector2(
                    Mathf.Round(newDefaultSpawn.x / 0.5f) * 0.5f,
                    Mathf.Round(newDefaultSpawn.y / 0.5f) * 0.5f);

                if (changeScope.changed)
                {
                    // Record to support undo
                    Undo.RecordObject(Data, "Changed default spawn point");
                    Data.defaultSpawnPoint = Data.transform.InverseTransformPoint(newDefaultSpawn);
                }
            }

            // Default spawn point handle
            if (_isEditingCamera)
            {
                // Change check for undo
                using EditorGUI.ChangeCheckScope changeScope = new();

                // Draw position handle
                Vector2 newCameraPos = Handles.PositionHandle(
                    Data.transform.TransformPoint(Data.lockedCameraPos),
                    Quaternion.identity);

                // Snap to grid
                newCameraPos = new Vector2(
                    Mathf.Round(newCameraPos.x / 0.5f) * 0.5f,
                    Mathf.Round(newCameraPos.y / 0.5f) * 0.5f);

                if (changeScope.changed)
                {
                    // Record to support undo
                    Undo.RecordObject(Data, "Changed camera pos");
                    Vector2 transformedPos = Data.transform.InverseTransformPoint(newCameraPos);
                    Data.lockedCameraPos = new Vector2(
                        Data.lockCameraX ? transformedPos.x : 0,
                        Data.lockCameraY ? transformedPos.y : 0);
                }
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStart;

            _layerNameProp = serializedObject.FindProperty("layerName");
            _levelIndexProp = serializedObject.FindProperty("index");
            _lockCameraXProp = serializedObject.FindProperty("lockCameraX");
            _lockCameraYProp = serializedObject.FindProperty("lockCameraY");
            _lockedCameraPosProp = serializedObject.FindProperty("lockedCameraPos");
            _defaultSpawnPointProp = serializedObject.FindProperty("defaultSpawnPoint");
            _transitionsProp = serializedObject.FindProperty("transitions");
            _generateShadowsProp = serializedObject.FindProperty("generateShadows");

            // Custom array editing
            _transitionsList = new ReorderableList(serializedObject, _transitionsProp, false, false, true, true)
            {
                elementHeight = EditorGUIUtility.singleLineHeight * 2
            };

            // Customize how each element is drawn
            _transitionsList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                SerializedProperty transitionProp = _transitionsList.serializedProperty.GetArrayElementAtIndex(index);
                if (transitionProp.objectReferenceValue == null)
                {
                    _transitionsList.serializedProperty.DeleteArrayElementAtIndex(index);
                    _transitionsList.index--;
                    return;
                }
                
                LevelTransition transition = (LevelTransition)transitionProp.objectReferenceValue;

                // Horizontal split
                {
                    // Change label width to be smaller
                    float originalLabelWdith = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 80;

                    // nextLeveLIndex
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width / 2 - 5, rect.height / 2 - 1),
                        "Dest. Index:",
                        transition.nextLevelIndex.ToString());

                    // layerName
                    EditorGUI.LabelField(
                        new Rect(rect.x + rect.width / 2 + 5, rect.y, rect.width / 2 - 5, rect.height / 2 - 1),
                        "Dest. Layer:",
                        transition.layerName);

                    // Edit button
                    if (GUI.Button(
                        new Rect(rect.x, rect.y + rect.height / 2 + 1, rect.width, rect.height / 2 - 1),
                        "Edit"))
                    {
                        Selection.activeGameObject = transition.gameObject;
                    }

                    // Reset label size
                    EditorGUIUtility.labelWidth = originalLabelWdith;
                }
            };

            // Instantiate new transition on add
            _transitionsList.onAddCallback += (ReorderableList list) =>
            {
                GameObject transitionTemplate = AssetDatabase.LoadAssetAtPath<GameObject>(
                    Path.Combine(LevelCreatorWindow.LevelElementsAssetsPath, "Transition.prefab"));

                Transform TransitionParent = Data.transform.Find("Transitions");
                GameObject newTransitionGO = Instantiate(transitionTemplate, TransitionParent);
                LevelTransition newTransition = newTransitionGO.GetComponent<LevelTransition>();
                newTransition.layerName = _layerNameProp.stringValue;

                int index = list.serializedProperty.arraySize++;
                list.index = index;

                list.serializedProperty.GetArrayElementAtIndex(index).objectReferenceValue = newTransition;
            };

            // Make sure user actually wants to remove transition
            _transitionsList.onRemoveCallback += (ReorderableList list) =>
            {
                if (EditorUtility.DisplayDialog("Are you sure?", "Deleting this is irreversable.", "OK", "Cancel"))
                {
                    list.serializedProperty.DeleteArrayElementAtIndex(list.index);
                }
            };
        }

        private void OnPlayModeStart(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.EnteredPlayMode)
            {
                return;
            }

            // Forcibly add our level to the level loader's cache
            Dictionary<string, Dictionary<int, LevelData>> loaderCache = new();
            loaderCache[Data.layerName] = new Dictionary<int, LevelData>
            {
                [Data.index] = Data
            };

            typeof(LevelLoader).GetField("_layers", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(FindObjectOfType<LevelLoader>(), loaderCache);

            // Start the level
            FindObjectOfType<GameManager>()
                .EnterLevel(Data.layerName, Data.index, false);

            // Spawn cat with player
            GameObject catPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                Path.Combine(LevelCreatorWindow.LevelElementsAssetsPath, "Cat.prefab"));

            GameObject catPrefabCopy = Instantiate(catPrefab);
            catPrefabCopy.transform.position = FindObjectOfType<Character>().transform.position;
        }
    }
}
