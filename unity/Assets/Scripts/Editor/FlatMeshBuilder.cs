using System;
using System.Collections.Generic;
using UnityEngine;

// Collects triangles into a flat-shaded mesh: every triangle gets its own three vertices,
// so RecalculateNormals produces one normal per face (the low-poly look).
public sealed class FlatMeshBuilder
{
    const int MaxVerticesFor16BitIndices = 65535;

    readonly List<Vector3> vertices = new();

    // Adds a triangle and orders its vertices so the front face points along outward.
    public void AddTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 outward)
    {
        if (Vector3.Dot(Vector3.Cross(b - a, c - a), outward) < 0f) (b, c) = (c, b);

        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);
    }

    public Mesh Build(string name)
    {
        if (vertices.Count == 0) throw new InvalidOperationException($"Mesh {name} has no triangles.");
        if (vertices.Count > MaxVerticesFor16BitIndices) throw new InvalidOperationException($"Mesh {name} has {vertices.Count} vertices, more than 16-bit indices allow.");

        var indices = new int[vertices.Count];
        for (int i = 0; i < indices.Length; i++) indices[i] = i;

        var mesh = new Mesh { name = name };
        mesh.SetVertices(vertices);
        mesh.SetTriangles(indices, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
