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
                || EditorUtility.DisplayDialog("Are you sure?", $"An already existing prefab exists with the index \"{level.index}\". Are you sure you want to replace it?", "Yes", "No"))
            {
                // Save new asset
                Object.DestroyImmediate(level.GetComponent<CompositeCollider2D>());
                Object.DestroyImmediate(level.GetComponent<Rigidbody2D>());

                level.collider = level.GetComponentInChildren<TilemapCollider2D>();
                level.tilemap = level.GetComponentInChildren<Tilemap>();
                level.tilemap.CompressBounds();
                level.collideableBounds = GetCollideableBoundsFromTilemap(level.tilemap);
                PrefabUtility.SaveAsPrefabAsset(level.gameObject, path);

                return true;
            }
            else
            {
                return false;
            }
        }

        public static Bounds GetCollideableBoundsFromTilemap(Tilemap tilemap)
        {
            BoundsInt cellBounds = tilemap.cellBounds;
            Vector3Int min = new(int.MaxValue, int.MaxValue);
            Vector3Int max = new(int.MinValue, int.MinValue);

            // Temp var to save memory
            Vector3Int currentTilePos = Vector3Int.zero;

            // Get get all sides of bounds
            for (int x = cellBounds.min.x; x <= cellBounds.max.x; x++)
            {
                currentTilePos.x = x;
                for (int y = cellBounds.min.y; y <= cellBounds.max.y; y++)
                {
                    currentTilePos.y = y;

                    if (tilemap.GetColliderType(currentTilePos) != Tile.ColliderType.None)
                    {
                        if (currentTilePos.x < min.x)
                        {
                            min.x = currentTilePos.x;
                        }

                        if (currentTilePos.y < min.y)
                        {
                            min.y = currentTilePos.y;
                        }

                        if (currentTilePos.x > max.x)
                        {
                            max.x = currentTilePos.x;
                        }

                        if (currentTilePos.y > max.y)
                        {
                            max.y = currentTilePos.y;
                        }
                    }
                }
            }

            Bounds res = new();
            // The pos represents the bottom left of the tile, so we have to 
            // add one to compensate for that
            res.SetMinMax(tilemap.CellToWorld(min), tilemap.CellToWorld(max + new Vector3Int(1, 1)));
            return res;
        }
    }
}
