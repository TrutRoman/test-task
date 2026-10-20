using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

// Builds the level, the decoration around it and the scene lighting in the active scene.
// Obstacle positions and sizes are gameplay: change them only on purpose.
public static class EnvironmentBuilder
{
    public const float ArenaHalfSize = 20f;
    public const float TerrainSize = 130f;
    public const int TerrainCells = 80;

    const float FenceInset = 0.4f;
    const float FencePostSpacing = 2.5f;
    const float FenceWallHeight = 4f;
    static readonly float[] FenceRailHeights = { 0.45f, 0.9f };

    static readonly Vector3[] TreePositions =
    {
        new(-14f, 0f, -13f), new(-16f, 0f, -3f), new(-15f, 0f, 10f), new(-8f, 0f, 15f), new(12f, 0f, 15f),
        new(16f, 0f, 8f), new(15f, 0f, -7f), new(9f, 0f, -15f), new(-3f, 0f, -16f), new(17f, 0f, -16f),
    };

    static readonly Vector3[] BushPositions =
    {
        new(-12.5f, 0f, -11.5f), new(14f, 0f, 10f), new(10.5f, 0f, -13.5f), new(-9.5f, 0f, 13.5f), new(13.5f, 0f, 13.5f), new(-14f, 0f, 1f),
    };

    public static void Build(ArtMaterials materials, EnvironmentMeshes meshes, Light sun)
    {
        Transform level = new GameObject("Level").transform;
        CreateGround(level, materials, meshes);
        CreateObstacles(level, materials);

        Transform decor = new GameObject("Decor").transform;
        CreateFence(decor, materials, meshes);
        CreateTrees(decor, materials, meshes);
        CreateBushes(decor, materials, meshes);
        CreateRocks(decor, materials, meshes);
        CreateColumns(decor, materials, meshes);

        ApplyLighting(materials, sun);
    }

    // Shallow dips inside the arena (never above the flat collider at y = 0) and hills outside it.
    public static float TerrainHeight(float x, float z)
    {
        float distanceFromCenter = Mathf.Max(Mathf.Abs(x), Mathf.Abs(z));
        float dips = -0.12f * Mathf.PerlinNoise(x * 0.35f + 31.7f, z * 0.35f + 12.3f);
        float hillMask = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(ArenaHalfSize + 1f, ArenaHalfSize + 16f, distanceFromCenter));
        float hillHeight = 2.5f + 5f * Mathf.PerlinNoise(x * 0.07f + 5.1f, z * 0.07f + 9.4f);
        return dips + hillMask * hillHeight;
    }

    // ---------- Level ----------

    static void CreateGround(Transform parent, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        // The walkable surface stays a flat box; the visible relief is decoration only.
        var ground = CreateChild(parent, "Ground Collider").gameObject.AddComponent<BoxCollider>();
        ground.center = new Vector3(0f, -0.5f, 0f);
        ground.size = new Vector3(ArenaHalfSize * 2f, 1f, ArenaHalfSize * 2f);

        CreateMeshObject(parent, "Terrain", meshes.Terrain, materials.Grass, Vector3.zero, Vector3.one, false);
    }

    static void CreateObstacles(Transform parent, ArtMaterials materials)
    {
        CreateBox(parent, "Crate", new Vector3(2.5f, 0.5f, -3f), Vector3.one, materials.Obstacle, true);

        // Stairs: 0.25 m steps are below the controller's default 0.3 m step offset.
        for (int i = 0; i < 5; i++)
        {
            float height = (i + 1) * 0.25f;
            CreateBox(parent, $"Step {i + 1}", new Vector3(6f, height / 2f, i * 0.6f), new Vector3(3f, height, 0.6f), materials.Stairs, true);
        }
        CreateBox(parent, "Stairs Top", new Vector3(6f, 0.625f, 4.2f), new Vector3(3f, 1.25f, 3f), materials.Stairs, true);

        // Jump boxes: 0.8 m is reachable from the ground, 1.6 m only from the lower box.
        CreateBox(parent, "Jump Box Low", new Vector3(-6f, 0.4f, 0f), new Vector3(2f, 0.8f, 2f), materials.Obstacle, true);
        CreateBox(parent, "Jump Box High", new Vector3(-6f, 0.8f, 2f), new Vector3(2f, 1.6f, 2f), materials.Obstacle, true);

        // Crouch tunnel: 1.4 m clearance is lower than the standing height and higher than the crouch height.
        CreateBox(parent, "Tunnel Wall Left", new Vector3(-1.75f, 0.7f, 8f), new Vector3(0.5f, 1.4f, 4f), materials.Accent, true);
        CreateBox(parent, "Tunnel Wall Right", new Vector3(1.75f, 0.7f, 8f), new Vector3(0.5f, 1.4f, 4f), materials.Accent, true);
        CreateBox(parent, "Tunnel Roof", new Vector3(0f, 1.65f, 8f), new Vector3(4f, 0.5f, 4f), materials.Accent, true);
    }

    // ---------- Decoration ----------

    static void CreateFence(Transform parent, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        Transform fence = CreateChild(parent, "Fence");
        float edge = ArenaHalfSize - FenceInset;
        string[] names = { "North", "East", "South", "West" };
        for (int side = 0; side < 4; side++)
        {
            Quaternion rotation = Quaternion.Euler(0f, side * 90f, 0f);
            CreateFenceSide(fence, names[side], rotation * new Vector3(0f, 0f, edge), rotation, edge * 2f, materials, meshes);
        }
    }

    // Posts and rails for the look, plus one tall invisible box so nobody falls off the arena.
    // Each side skips its last post: the next side starts with a post in that corner.
    static void CreateFenceSide(Transform parent, string name, Vector3 center, Quaternion rotation, float length, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        Transform side = CreateChild(parent, $"Fence {name}");
        side.SetLocalPositionAndRotation(center, rotation);

        var wall = side.gameObject.AddComponent<BoxCollider>();
        wall.center = new Vector3(0f, FenceWallHeight / 2f, 0f);
        wall.size = new Vector3(length, FenceWallHeight, 0.4f);

        int posts = Mathf.RoundToInt(length / FencePostSpacing);
        for (int i = 0; i < posts; i++)
        {
            float x = -length / 2f + i * length / posts;
            CreateMeshObject(side, $"Post {i + 1}", meshes.FencePost, materials.Wood, new Vector3(x, 0f, 0f), Vector3.one, false);
        }
        for (int i = 0; i < FenceRailHeights.Length; i++)
            CreateBox(side, $"Rail {i + 1}", new Vector3(0f, FenceRailHeights[i], 0f), new Vector3(length, 0.09f, 0.06f), materials.Wood, false);
    }

    static void CreateTrees(Transform parent, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        var random = new System.Random(11);
        for (int i = 0; i < TreePositions.Length; i++)
        {
            Transform tree = CreateChild(parent, $"Tree {i + 1}");
            tree.SetLocalPositionAndRotation(TreePositions[i], Quaternion.Euler(0f, Range(random, 0f, 360f), 0f));
            tree.localScale = Vector3.one * Range(random, 0.85f, 1.2f);

            Material foliage = i % 3 == 2 ? materials.FoliageRose : materials.FoliageMint;
            CreateMeshObject(tree, "Trunk", meshes.Trunk, materials.Wood, Vector3.zero, Vector3.one, true);
            // Three stacked cones, each smaller and higher than the one below.
            CreateMeshObject(tree, "Foliage 1", meshes.FoliageCone, foliage, new Vector3(0f, 1f, 0f), new Vector3(1.4f, 1.7f, 1.4f), true);
            CreateMeshObject(tree, "Foliage 2", meshes.FoliageCone, foliage, new Vector3(0f, 1.8f, 0f), new Vector3(1.05f, 1.5f, 1.05f), true);
            CreateMeshObject(tree, "Foliage 3", meshes.FoliageCone, foliage, new Vector3(0f, 2.55f, 0f), new Vector3(0.7f, 1.2f, 0.7f), true);
        }
    }

    static void CreateBushes(Transform parent, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        var random = new System.Random(23);
        for (int i = 0; i < BushPositions.Length; i++)
        {
            float size = Range(random, 0.7f, 1.05f);
            Material foliage = i % 2 == 1 ? materials.FoliageRose : materials.FoliageMint;
            GameObject bush = CreateMeshObject(parent, $"Bush {i + 1}", meshes.Bush, foliage, BushPositions[i] + Vector3.up * 0.25f * size, new Vector3(1.1f, 0.75f, 1.1f) * size, true);
            bush.transform.localRotation = Quaternion.Euler(0f, Range(random, 0f, 360f), 0f);
        }
    }

    static void CreateRocks(Transform parent, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        CreateRock(parent, 1, meshes.Rocks[0], materials, new Vector3(-10f, 0f, -8f), new Vector3(1.3f, 0.8f, 1.1f), 20f);
        CreateRock(parent, 2, meshes.Rocks[1], materials, new Vector3(9f, 0f, -9f), new Vector3(1f, 0.7f, 1.4f), 70f);
        CreateRock(parent, 3, meshes.Rocks[2], materials, new Vector3(-17f, 0f, 16.5f), new Vector3(1.8f, 1.2f, 1.6f), 10f);
        CreateRock(parent, 4, meshes.Rocks[0], materials, new Vector3(17f, 0f, -12f), new Vector3(1.5f, 1f, 1.3f), 140f);
        CreateRock(parent, 5, meshes.Rocks[1], materials, new Vector3(3f, 0f, 14f), new Vector3(0.7f, 0.5f, 0.8f), 200f);
        CreateRock(parent, 6, meshes.Rocks[2], materials, new Vector3(-6f, 0f, 13f), new Vector3(0.9f, 0.6f, 1f), 300f);
        CreateRock(parent, 7, meshes.Rocks[0], materials, new Vector3(12f, 0f, 3f), new Vector3(0.8f, 0.6f, 0.7f), 45f);
    }

    // Rocks sit partly below the ground so they look embedded.
    static void CreateRock(Transform parent, int number, Mesh mesh, ArtMaterials materials, Vector3 position, Vector3 scale, float yaw)
    {
        GameObject rock = CreateMeshObject(parent, $"Rock {number}", mesh, materials.Rock, position + Vector3.up * scale.y * 0.3f, scale, true);
        rock.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);
    }

    // A small ruin: two whole columns and a broken one.
    static void CreateColumns(Transform parent, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        CreateColumn(parent, "Column 1", new Vector3(-13f, 0f, -0.5f), 2.6f, true, materials, meshes);
        CreateColumn(parent, "Column 2", new Vector3(-13f, 0f, 3.5f), 2.6f, true, materials, meshes);
        CreateColumn(parent, "Column 3", new Vector3(-13f, 0f, 7.5f), 1.3f, false, materials, meshes);
    }

    static void CreateColumn(Transform parent, string name, Vector3 position, float shaftHeight, bool hasCapital, ArtMaterials materials, EnvironmentMeshes meshes)
    {
        Transform column = CreateChild(parent, name);
        column.localPosition = position;

        CreateBox(column, "Base", new Vector3(0f, 0.15f, 0f), new Vector3(0.9f, 0.3f, 0.9f), materials.Stone, true);
        CreateMeshObject(column, "Shaft", meshes.ColumnShaft, materials.Stone, new Vector3(0f, 0.3f, 0f), new Vector3(1f, shaftHeight, 1f), true);
        if (hasCapital)
            CreateBox(column, "Capital", new Vector3(0f, 0.3f + shaftHeight + 0.125f, 0f), new Vector3(0.85f, 0.25f, 0.85f), materials.Stone, true);
    }

    // ---------- Lighting ----------

    static void ApplyLighting(ArtMaterials materials, Light sun)
    {
        sun.color = Palette.Lighten(Palette.Peach, 0.6f);
        sun.intensity = 1.3f;
        sun.shadows = LightShadows.Soft;
        sun.shadowStrength = 0.85f;
        sun.transform.rotation = Quaternion.Euler(45f, -35f, 0f);

        // Ambient is kept darker than the sun so faces and shadows keep their shape.
        RenderSettings.sun = sun;
        RenderSettings.skybox = materials.Sky;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Palette.Darken(Palette.Lavender, 0.2f);
        RenderSettings.ambientEquatorColor = Palette.Darken(Palette.Peach, 0.25f);
        RenderSettings.ambientGroundColor = Palette.Darken(Palette.Mint, 0.55f);

        // Fog takes the horizon color, so distant hills melt into the sky.
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = ArtMaterials.SkyHorizon;
        RenderSettings.fogStartDistance = 35f;
        RenderSettings.fogEndDistance = 110f;
    }

    // ---------- Helpers ----------

    static Transform CreateChild(Transform parent, string name)
    {
        Transform child = new GameObject(name).transform;
        child.SetParent(parent, false);
        return child;
    }

    static GameObject CreateBox(Transform parent, string name, Vector3 position, Vector3 size, Material material, bool collide)
    {
        GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.SetParent(parent, false);
        box.transform.localPosition = position;
        box.transform.localScale = size;
        box.GetComponent<MeshRenderer>().sharedMaterial = material;
        if (!collide) Object.DestroyImmediate(box.GetComponent<BoxCollider>());
        return box;
    }

    static GameObject CreateMeshObject(Transform parent, string name, Mesh mesh, Material material, Vector3 position, Vector3 scale, bool collide)
    {
        var item = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        item.transform.SetParent(parent, false);
        item.transform.localPosition = position;
        item.transform.localScale = scale;
        item.GetComponent<MeshFilter>().sharedMesh = mesh;
        item.GetComponent<MeshRenderer>().sharedMaterial = material;

        if (collide)
        {
            var collider = item.AddComponent<MeshCollider>();
            collider.sharedMesh = mesh;
            collider.convex = true;
        }
        return item;
    }

    static float Range(System.Random random, float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }
}
