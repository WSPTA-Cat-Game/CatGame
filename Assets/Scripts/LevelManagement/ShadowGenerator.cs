using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace CatGame.LevelManagement
{
    public static class ShadowGenerator
    {
        private static readonly FieldInfo pathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo pathHashField = typeof(ShadowCaster2D).GetField("m_ShapePathHash", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodInfo getPathHashMethod = Type.GetType("UnityEngine.Rendering.Universal.LightUtility, Unity.RenderPipelines.Universal.Runtime, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null").GetMethod("GetShapePathHash");

        public static void GenerateShadowForLevel(LevelData level)
        {
            CompositeCollider2D collider = level.GetComponent<CompositeCollider2D>();

            Transform existingShadowParent = level.transform.Find("Shadow Parent");
            if (existingShadowParent != null)
            {
                UnityEngine.Object.DestroyImmediate(existingShadowParent.gameObject);
            }

            GameObject shadowParent = new("Shadow Parent");
            shadowParent.transform.parent = level.transform;

            for (int i = 0; i < collider.pathCount; i++)
            {
                Vector2[] points = new Vector2[collider.GetPathPointCount(i)];

                collider.GetPath(i, points);

                Vector3[] points3D = new Vector3[collider.GetPathPointCount(i)];
                for (int j = 0; j < points.Length; j++)
                {
                    points3D[j] = points[j];
                }

                GameObject shadowCaster = new("Shadow Caster");
                shadowCaster.transform.parent = shadowParent.transform;
                ShadowCaster2D caster = shadowCaster.AddComponent<ShadowCaster2D>();

                pathField.SetValue(caster, points3D);
                int hash = (int)getPathHashMethod.Invoke(null, new object[] { caster.shapePath });
                pathHashField.SetValue(caster, hash);
            }
        }
    }
}
