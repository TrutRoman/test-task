using UnityEngine;

// The low-poly meshes of the level decoration, generated once per build and shared by both scenes.
public sealed class EnvironmentMeshes
{
    public Mesh Terrain;
    public Mesh Trunk;
    public Mesh FoliageCone;
    public Mesh ColumnShaft;
    public Mesh FencePost;
    public Mesh Bush;
    public Mesh[] Rocks;

    public static EnvironmentMeshes Create()
    {
        return new EnvironmentMeshes
        {
            Terrain = Save(LowPolyMeshes.Terrain("Terrain", EnvironmentBuilder.TerrainSize, EnvironmentBuilder.TerrainCells, EnvironmentBuilder.TerrainHeight)),
            Trunk = Save(LowPolyMeshes.Frustum("Trunk", 6, 0.22f, 0.14f, 1.4f)),
            // Unit-sized shapes are scaled per instance.
            FoliageCone = Save(LowPolyMeshes.Frustum("Foliage Cone", 7, 1f, 0f, 1f)),
            ColumnShaft = Save(LowPolyMeshes.Frustum("Column Shaft", 8, 0.34f, 0.28f, 1f)),
            FencePost = Save(LowPolyMeshes.Frustum("Fence Post", 6, 0.08f, 0.06f, 1.1f)),
            Bush = Save(LowPolyMeshes.Sphere("Bush", 1, 1f, 0.15f, 7)),
            Rocks = new[]
            {
                Save(LowPolyMeshes.Sphere("Rock A", 0, 1f, 0.25f, 1)),
                Save(LowPolyMeshes.Sphere("Rock B", 0, 1f, 0.3f, 2)),
                Save(LowPolyMeshes.Sphere("Rock C", 1, 1f, 0.2f, 3)),
            },
        };
    }

    static Mesh Save(Mesh mesh)
    {
        return GeneratedAssets.Save(mesh, $"Meshes/{mesh.name}.asset");
    }
}
