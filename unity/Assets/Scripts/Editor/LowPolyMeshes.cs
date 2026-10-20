using System;
using System.Collections.Generic;
using UnityEngine;

// Procedural low-poly meshes. All of them are flat-shaded (see FlatMeshBuilder).
public static class LowPolyMeshes
{
    // A cylinder with a flat base at y = 0. A top radius of 0 makes a cone.
    public static Mesh Frustum(string name, int segments, float bottomRadius, float topRadius, float height)
    {
        if (segments < 3) throw new ArgumentOutOfRangeException(nameof(segments), "A frustum needs at least 3 segments.");

        var builder = new FlatMeshBuilder();
        Vector3 top = Vector3.up * height;
        for (int i = 0; i < segments; i++)
        {
            Vector3 bottom0 = RingPoint(i, segments, bottomRadius, 0f);
            Vector3 bottom1 = RingPoint(i + 1, segments, bottomRadius, 0f);
            Vector3 top0 = RingPoint(i, segments, topRadius, height);
            Vector3 top1 = RingPoint(i + 1, segments, topRadius, height);
            Vector3 sideOutward = Horizontal(bottom0 + bottom1);

            builder.AddTriangle(Vector3.zero, bottom0, bottom1, Vector3.down);
            builder.AddTriangle(bottom0, bottom1, top1, sideOutward);
            if (topRadius > 0f)
            {
                builder.AddTriangle(bottom0, top1, top0, sideOutward);
                builder.AddTriangle(top, top0, top1, Vector3.up);
            }
        }
        return builder.Build(name);
    }

    // An icosahedron-based sphere centered at the origin. Jitter moves each vertex radially
    // by up to ±jitter of the radius, which turns the sphere into a rock or a bush.
    public static Mesh Sphere(string name, int subdivisions, float radius, float jitter, int seed)
    {
        var points = new List<Vector3>();
        List<int> faces = CreateIcosahedron(points);
        for (int i = 0; i < subdivisions; i++) faces = Subdivide(points, faces);

        var random = new System.Random(seed);
        for (int i = 0; i < points.Count; i++)
        {
            float offset = (float)(random.NextDouble() * 2.0 - 1.0) * jitter;
            points[i] = points[i].normalized * radius * (1f + offset);
        }

        var builder = new FlatMeshBuilder();
        for (int i = 0; i < faces.Count; i += 3)
        {
            Vector3 a = points[faces[i]], b = points[faces[i + 1]], c = points[faces[i + 2]];
            builder.AddTriangle(a, b, c, a + b + c);
        }
        return builder.Build(name);
    }

    // A square height-field grid centered at the origin, facing up.
    public static Mesh Terrain(string name, float size, int cells, Func<float, float, float> height)
    {
        var builder = new FlatMeshBuilder();
        float cellSize = size / cells;
        float half = size / 2f;

        Vector3 Point(int x, int z)
        {
            float worldX = x * cellSize - half;
            float worldZ = z * cellSize - half;
            return new Vector3(worldX, height(worldX, worldZ), worldZ);
        }

        for (int x = 0; x < cells; x++)
        {
            for (int z = 0; z < cells; z++)
            {
                Vector3 p00 = Point(x, z), p10 = Point(x + 1, z), p01 = Point(x, z + 1), p11 = Point(x + 1, z + 1);

                // Alternating diagonals avoid the stripy look of a uniform grid.
                if ((x + z) % 2 == 0)
                {
                    builder.AddTriangle(p00, p01, p11, Vector3.up);
                    builder.AddTriangle(p00, p11, p10, Vector3.up);
                }
                else
                {
                    builder.AddTriangle(p00, p01, p10, Vector3.up);
                    builder.AddTriangle(p10, p01, p11, Vector3.up);
                }
            }
        }
        return builder.Build(name);
    }

    static Vector3 RingPoint(int index, int segments, float radius, float y)
    {
        float angle = index * Mathf.PI * 2f / segments;
        return new Vector3(Mathf.Cos(angle) * radius, y, Mathf.Sin(angle) * radius);
    }

    static Vector3 Horizontal(Vector3 vector)
    {
        return new Vector3(vector.x, 0f, vector.z);
    }

    static List<int> CreateIcosahedron(List<Vector3> points)
    {
        float t = (1f + Mathf.Sqrt(5f)) / 2f;
        points.AddRange(new[]
        {
            new Vector3(-1, t, 0), new Vector3(1, t, 0), new Vector3(-1, -t, 0), new Vector3(1, -t, 0),
            new Vector3(0, -1, t), new Vector3(0, 1, t), new Vector3(0, -1, -t), new Vector3(0, 1, -t),
            new Vector3(t, 0, -1), new Vector3(t, 0, 1), new Vector3(-t, 0, -1), new Vector3(-t, 0, 1),
        });
        return new List<int>
        {
            0, 11, 5, 0, 5, 1, 0, 1, 7, 0, 7, 10, 0, 10, 11,
            1, 5, 9, 5, 11, 4, 11, 10, 2, 10, 7, 6, 7, 1, 8,
            3, 9, 4, 3, 4, 2, 3, 2, 6, 3, 6, 8, 3, 8, 9,
            4, 9, 5, 2, 4, 11, 6, 2, 10, 8, 6, 7, 9, 8, 1,
        };
    }

    // Splits every triangle into four; shared edges reuse one midpoint so the surface stays closed.
    static List<int> Subdivide(List<Vector3> points, List<int> faces)
    {
        var midpoints = new Dictionary<(int, int), int>();
        int Midpoint(int a, int b)
        {
            var key = a < b ? (a, b) : (b, a);
            if (midpoints.TryGetValue(key, out int index)) return index;

            points.Add((points[a] + points[b]) / 2f);
            midpoints[key] = points.Count - 1;
            return points.Count - 1;
        }

        var result = new List<int>();
        for (int i = 0; i < faces.Count; i += 3)
        {
            int a = faces[i], b = faces[i + 1], c = faces[i + 2];
            int ab = Midpoint(a, b), bc = Midpoint(b, c), ca = Midpoint(c, a);
            result.AddRange(new[] { a, ab, ca, b, bc, ab, c, ca, bc, ab, bc, ca });
        }
        return result;
    }
}
