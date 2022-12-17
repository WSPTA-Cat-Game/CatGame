using CatGame.LevelManagement;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace CatGame.Editor.LevelCreator
{
    internal static class Common
    {
        public static bool SaveLevelAsPrefab(LevelData level)
        {
            string path = Path.Combine(
                LevelCreatorWindow.LayersAssetsPath,
                level.layerName,
                level.index.ToString() + ".prefab");

            // Check if we're overwriting an existing one, and ask
            // for confirmation if so
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) == null
                || EditorUtility.DisplayDialog("Are you sure?", "An already existing prefab exists with the same index. Are you you want to replace it?", "Yes", "No"))
            {
                // Save new asset
                level.tilemap = level.GetComponentInChildren<Tilemap>();
                level.tilemap.CompressBounds();
                PrefabUtility.SaveAsPrefabAsset(level.gameObject, path);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
