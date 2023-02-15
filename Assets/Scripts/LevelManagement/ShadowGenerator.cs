using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatGame.LevelManagement
{
    // There is no built in way to add shadow casters to tilemaps, so let's do
    // it manually.
    public static class ShadowGenerator
    {
        // The fields and methods needed for this are all private, so we need
        // to use reflection to access them.
        private static readonly FieldInfo pathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo pathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo getPathHashMethod = Type.GetType("UnityEngine.Rendering.Universal.LightUtility, Unity.RenderPipelines.Universal.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetMethod("GetShapePathHash");

        public static void GenerateShadowForCollider(CompositeCollider2D collider)
        {
            // Only allow one shadow parent to exist per collider
            Transform existingShadowParent = collider.transform.Find("Shadow Parent");
            if (existingShadowParent != null)
            {
                UnityEngine.Object.Destroy(existingShadowParent.gameObject);
            }

            GameObject shadowParent = new("Shadow Parent");
            shadowParent.transform.SetParent(collider.transform, false);

            // Loop through all paths in the collider (every continuous shape)
            for (int i = 0; i < collider.pathCount; i++)
            {
                // Grab each point on the path (which is again, a continuous
                // shape) then convert it from vector 2 to vector 3
                Vector2[] points = new Vector2[collider.GetPathPointCount(i)];

                collider.GetPath(i, points);

                Vector3[] points3D = new Vector3[collider.GetPathPointCount(i)];
                for (int j = 0; j < points.Length; j++)
                {
                    points3D[j] = points[j];
                }

                // Create a shadow caster then assign the shape and hash
                // to the caster
                GameObject shadowCaster = new("Shadow Caster");
                shadowCaster.transform.SetParent(shadowParent.transform, false);
                ShadowCaster2D caster = shadowCaster.AddComponent<ShadowCaster2D>();

                pathField.SetValue(caster, points3D);
                int hash = (int)getPathHashMethod.Invoke(null, new object[] { caster.shapePath });
                pathHashField.SetValue(caster, hash);
            }
        }
    }
}
