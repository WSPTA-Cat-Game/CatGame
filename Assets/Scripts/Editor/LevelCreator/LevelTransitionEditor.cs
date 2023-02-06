using CatGame.LevelManagement;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

namespace CatGame.Editor.LevelCreator
{
    [CustomEditor(typeof(LevelTransition))]
    public class LevelTransitionEditor : UnityEditor.Editor
    {
        private SerializedProperty _nextLevelIndexProp;
        private SerializedProperty _layerNameProp;
        private SerializedProperty _hasAssociatedSpawnPointProp;
        private SerializedProperty _associatedSpawnPointProp;
        private SerializedProperty _canExitWithoutCatProp;

        private bool isAssociatedSpawnPointProp = false;

        private LevelTransition Transition => (LevelTransition)target;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // index
            EditorGUILayout.PropertyField(_nextLevelIndexProp, new GUIContent("Dest. Level Index"));

            // Layer name dropdown
            // Options will be the layer directories
            string[] options = Directory.GetDirectories(LevelCreatorWindow.LayersDirectory)
                .Select(dir => Path.GetFileName(dir)).ToArray();

            // Grab index of the current option
            int optionsIndex = Array.IndexOf(options, _layerNameProp.stringValue);
            optionsIndex = optionsIndex == -1 ? 0 : optionsIndex;

            // Get new layer directory
            int newOptionsIndex = EditorGUILayout.Popup("Dest. Layer Name", optionsIndex, options);
            _layerNameProp.stringValue = options[newOptionsIndex];

            // Collider
            BoxCollider2D collider = Transition.GetComponent<BoxCollider2D>();
            
            // Support undo
            using (EditorGUI.ChangeCheckScope scope = new())
            {
                using (new GUILayout.HorizontalScope())
                {
                    // Pos
                    Vector2 newPos = EditorGUILayout.Vector2Field("Pos", Transition.transform.position);

                    // Tool edit button
                    if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                    {
                        ToolManager.SetActiveTool(typeof(EditorWindow).Assembly.GetType("UnityEditor.MoveTool"));
                    }

                    // Support undo
                    if (scope.changed)
                    {
                        Undo.RecordObject(Transition.transform, "Changed transition position");

                        Transition.transform.position = newPos;

                    }
                }
            }
            
            using (EditorGUI.ChangeCheckScope scope = new())
            {
                // Offset
                Vector2 newOffset = EditorGUILayout.Vector2Field("Offset", collider.offset);

                // Size
                Vector2 newSize = EditorGUILayout.Vector2Field("Size", collider.size);

                if (GUILayout.Button("Edit Collider"))
                {
                    ToolManager.SetActiveTool(typeof(EditorWindow).Assembly.GetType("UnityEditor.BoxCollider2DTool"));
                }

                if (scope.changed)
                {
                    Undo.RecordObject(collider, "Changed transition collider");

                    collider.offset = newOffset;
                    collider.size = newSize;
                }
            }

            // Can exit without cat
            EditorGUILayout.PropertyField(_canExitWithoutCatProp);

            
            // Spawn point
            // Spawn point toggle
            using (EditorGUILayout.ToggleGroupScope scope = new("Has associated spawn point", _hasAssociatedSpawnPointProp.boolValue))
            {
                _hasAssociatedSpawnPointProp.boolValue = scope.enabled;

                // Set actual spawn point
                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(_associatedSpawnPointProp);
                    if (GUILayout.Button(isAssociatedSpawnPointProp ? "Return" : "Edit", EditorStyles.miniButton, GUILayout.MaxWidth(50)))
                    {
                        isAssociatedSpawnPointProp = !isAssociatedSpawnPointProp;
                    }
                }
            }

            // Bottom buttons
            EditorGUILayout.Space(50);

            if (GUILayout.Button("Back to LevelData"))
            {
                Selection.activeGameObject = Transition.GetComponentInParent<LevelData>().gameObject;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            // Default spawn point handle
            if (isAssociatedSpawnPointProp)
            {
                // Change check for undo
                using EditorGUI.ChangeCheckScope changeScope = new();

                // Draw position handle
                Vector2 associatedSpawnPoint = Handles.PositionHandle(
                    Transition.transform.TransformPoint(Transition.associatedSpawnPoint),
                    Quaternion.identity);

                // Snap to grid
                associatedSpawnPoint = new Vector2(
                    Mathf.Round(associatedSpawnPoint.x / 0.5f) * 0.5f,
                    Mathf.Round(associatedSpawnPoint.y / 0.5f) * 0.5f);

                if (changeScope.changed)
                {
                    // Record to support undo
                    Undo.RecordObject(Transition, "Changed associated spawn point");
                    Transition.associatedSpawnPoint = Transition.transform.InverseTransformPoint(associatedSpawnPoint);
                }
            }

        }

        private void OnEnable()
        {
            // Force snap to be on unless control is pressed
            SceneView.beforeSceneGui += view =>
            {
                Event.current.control = !Event.current.control;
            };

            _nextLevelIndexProp = serializedObject.FindProperty("nextLevelIndex");
            _layerNameProp = serializedObject.FindProperty("layerName");
            _hasAssociatedSpawnPointProp = serializedObject.FindProperty("hasAssociatedSpawnPoint");
            _associatedSpawnPointProp = serializedObject.FindProperty("associatedSpawnPoint");
            _canExitWithoutCatProp = serializedObject.FindProperty("canExitWithoutCat");
        }
    }
}
