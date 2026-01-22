using UnityEngine;


namespace CrowRx.Utility
{
    public static class Graphic
    {
        private struct Line2D
        {
            public Vector2 A;
            public Vector2 B;


            public bool Intersect(Vector2 targetA, Vector2 targetB)
            {
                Vector3 lineTarget = (targetB - targetA).normalized;

                float vaA = Vector3.Cross(lineTarget, (A - targetA).normalized).z;
                float vaB = Vector3.Cross(lineTarget, (B - targetA).normalized).z;

                float ab = vaA * vaB;

                Vector2 lineThis = (B - A).normalized;

                float vaC = Vector3.Cross(lineThis, (targetA - A).normalized).z;
                float vaD = Vector3.Cross(lineThis, (targetB - A).normalized).z;

                float cd = vaC * vaD;

                return ab <= 0 && cd <= 0;
            }

            public bool Intersect(Line2D ab)
            {
                return Intersect(ab.A, ab.B);
            }
        }

        public static bool RectIntersect(Rect rect, Vector2 a, Vector2 b)
        {
            if (rect.Contains(a) || rect.Contains(b))
            {
                return true;
            }

            Line2D targetLine = new() { A = a, B = b };
            Line2D[] rectLines =
            {
                new() { A = new Vector2(rect.xMin, rect.yMin), B = new Vector2(rect.xMax, rect.yMin) },
                new() { A = new Vector2(rect.xMax, rect.yMin), B = new Vector2(rect.xMax, rect.yMax) },
                new() { A = new Vector2(rect.xMax, rect.yMax), B = new Vector2(rect.xMin, rect.yMax) },
                new() { A = new Vector2(rect.xMin, rect.yMax), B = new Vector2(rect.xMin, rect.yMin) },
            };

            for (int i = 0, count = rectLines.Length; i < count; ++i)
            {
                if (rectLines[i].Intersect(targetLine))
                {
                    return true;
                }
            }

            return false;
        }

        public static Mesh RotateMesh(Mesh original, Quaternion rotation)
        {
            Vector3[] verts = original.vertices;
            Vector3[] normals = original.normals;

            for (int i = 0; i < verts.Length; i++)
            {
                verts[i] = rotation * verts[i];
            }

            for (int i = 0; i < normals.Length; i++)
            {
                normals[i] = rotation * normals[i];
            }

            Mesh rotated = new()
            {
                name = original.name + "_rotated",
                vertices = verts,
                normals = normals,
                triangles = original.triangles,
                uv = original.uv
            };

            rotated.RecalculateBounds();
            rotated.RecalculateNormals();

            return rotated;
        }
    }
}