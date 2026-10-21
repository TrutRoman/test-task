using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Tools → Setup Chair Test: imports model/chair.fbx, builds the Chair prefab with one BoxCollider per part
// and a separate CollisionTest scene. MainMenu and Game are never saved; Game is only opened to copy its player.
public static class ChairTestBuilder
{
    const string SourceFbx = "../model/chair.fbx";
    const string ModelPath = "Assets/Models/chair.fbx";
    const string MaterialsFolder = "Assets/Models/Materials";
    const string PrefabPath = "Assets/Models/Chair.prefab";
    const string TestMaterialsFolder = "Assets/CollisionTest/Materials";
    const string ScenePath = "Assets/Scenes/CollisionTest.unity";
    const string GameScenePath = "Assets/Scenes/Game.unity";
    const string SmoothnessProperty = "_Smoothness";
    const float ChairHeight = 0.9f;
    const float Tolerance = 0.01f;

    static readonly string[] PartNames = { "Seat", "Back", "Leg_FrontLeft", "Leg_FrontRight", "Leg_BackLeft", "Leg_BackRight" };

    // Positions above the seat (in the chair's space, +Z is the front edge) and sizes of the cubes that fall onto it.
    // Each cube drops into its own quarter of the seat, clear of the back, so they land instead of knocking each other off.
    static readonly (Vector3 offset, float size, Vector3 euler)[] FallingCubes =
    {
        (new Vector3(0.1f, 1.2f, 0.105f), 0.17f, new Vector3(0f, 15f, 0f)),
        (new Vector3(-0.1f, 1.5f, 0.105f), 0.16f, new Vector3(4f, 40f, 0f)),
        // Rear cubes turn only a little: a turned corner would catch the top of the back and bounce off.
        (new Vector3(0.1f, 1.8f, -0.065f), 0.15f, new Vector3(0f, 10f, 3f)),
        (new Vector3(-0.1f, 2.1f, -0.065f), 0.15f, new Vector3(3f, 5f, 0f)),
    };

    [MenuItem("Tools/Setup Chair Test")]
    public static void Setup()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        // Prefab building needs an open scene; an empty one keeps every existing scene untouched.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        ImportModel();
        GameObject chairPrefab = CreateChairPrefab();
        BuildCollisionTestScene(chairPrefab);
        AddSceneToBuildSettings();
        AssetDatabase.SaveAssets();
        Debug.Log($"ChairTest: built {PrefabPath} and {ScenePath}; build scenes: {string.Join(", ", EditorBuildSettings.scenes.Select(scene => scene.path))}");
    }

    // ---------- Model ----------

    static void ImportModel()
    {
        string source = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), SourceFbx));
        if (!File.Exists(source)) throw new FileNotFoundException("Export the chair first: blender -b --python-exit-code 1 -P model/make_chair.py", source);

        CreateFolder("Assets", "Models");
        CreateFolder("Assets/Models", "Materials");
        File.Copy(source, ModelPath, true);
        AssetDatabase.ImportAsset(ModelPath, ImportAssetOptions.ForceUpdate);

        var importer = (ModelImporter)AssetImporter.GetAtPath(ModelPath);
        // The FBX is written in metres with no axis rotation left (see model/make_chair.py), so Unity keeps it 1:1.
        importer.globalScale = 1f;
        importer.useFileScale = true;
        importer.importCameras = false;
        importer.importLights = false;
        importer.importBlendShapes = false;
        importer.importAnimation = false;
        importer.animationType = ModelImporterAnimationType.None;
        importer.addCollider = false;
        importer.importNormals = ModelImporterNormals.Import;
        importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Wood"),
            CreateLitMaterial($"{MaterialsFolder}/Chair Wood.mat", Palette.Darken(Palette.Peach, 0.4f), 0.1f));
        importer.AddRemap(new AssetImporter.SourceAssetIdentifier(typeof(Material), "Fabric"),
            CreateLitMaterial($"{MaterialsFolder}/Chair Fabric.mat", Palette.Rose, 0.2f));
        importer.SaveAndReimport();
    }

    static GameObject CreateChairPrefab()
    {
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
        var chair = (GameObject)PrefabUtility.InstantiatePrefab(model);
        chair.name = "Chair";
        VerifyModel(chair);
        foreach (string partName in PartNames) AddBoxCollider(FindPart(chair, partName));

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(chair, PrefabPath, out bool saved);
        if (!saved) throw new InvalidOperationException($"Failed to save {PrefabPath}.");
        Object.DestroyImmediate(chair);
        return prefab;
    }

    // The FBX must arrive in metres, unrotated, standing on y = 0, facing +Z (Unity's forward),
    // with its materials remapped to the palette materials.
    static void VerifyModel(GameObject chair)
    {
        Bounds total = FindPart(chair, PartNames[0]).GetComponent<MeshRenderer>().bounds;
        foreach (string partName in PartNames)
        {
            Transform part = FindPart(chair, partName);
            var renderer = part.GetComponent<MeshRenderer>();
            Bounds bounds = renderer.bounds;
            total.Encapsulate(bounds);
            string materialPath = AssetDatabase.GetAssetPath(renderer.sharedMaterial);
            Debug.Log($"ChairTest: {partName,-15} rotation {part.localEulerAngles} scale {part.lossyScale} size {bounds.size:F3} center {bounds.center:F3} material {materialPath}");

            if (part.localRotation != Quaternion.identity) throw new InvalidOperationException($"{partName} is rotated by {part.localEulerAngles}.");
            if (part.lossyScale != Vector3.one) throw new InvalidOperationException($"{partName} has scale {part.lossyScale}.");
            if (!materialPath.StartsWith(MaterialsFolder)) throw new InvalidOperationException($"{partName} uses {materialPath} instead of a palette material.");
        }

        Debug.Log($"ChairTest: whole chair size {total.size:F3}, bottom {total.min.y:F3}");
        if (Mathf.Abs(total.size.y - ChairHeight) > Tolerance || Mathf.Abs(total.min.y) > Tolerance)
            throw new InvalidOperationException($"Chair should be {ChairHeight} m tall and stand on y = 0, got size {total.size} and bottom {total.min.y}.");
        if (FindPart(chair, "Back").GetComponent<MeshRenderer>().bounds.center.z >= 0f)
            throw new InvalidOperationException("The back should be behind the seat (-Z), so the chair faces +Z.");
    }

    // A box that exactly encloses the part's mesh, in the part's own space.
    static void AddBoxCollider(Transform part)
    {
        Bounds bounds = part.GetComponent<MeshFilter>().sharedMesh.bounds;
        var box = part.gameObject.AddComponent<BoxCollider>();
        box.center = bounds.center;
        box.size = bounds.size;
    }

    static Transform FindPart(GameObject chair, string partName)
    {
        Transform part = chair.transform.Find(partName);
        if (part == null) throw new InvalidOperationException($"The chair model has no part '{partName}'.");
        return part;
    }

    // ---------- Scene ----------

    static void BuildCollisionTestScene(GameObject chairPrefab)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("The default scene setup has no Main Camera.");

        CreateFolder("Assets", "CollisionTest");
        CreateFolder("Assets/CollisionTest", "Materials");
        CreateFloor(CreateLitMaterial($"{TestMaterialsFolder}/Floor.mat", Palette.Mint, 0.05f));

        // Turned around so the chair's front faces the player, who starts behind it on -Z and looks along +Z.
        var chair = (GameObject)PrefabUtility.InstantiatePrefab(chairPrefab);
        chair.transform.SetPositionAndRotation(new Vector3(0f, 0f, 2f), Quaternion.Euler(0f, 180f, 0f));
        CreateFallingCubes(chair.transform, CreateLitMaterial($"{TestMaterialsFolder}/Falling Cube.mat", Palette.Lavender, 0.3f));

        GameObject player = CopyPlayerFromGame();
        player.transform.SetPositionAndRotation(new Vector3(0f, 0f, -1f), Quaternion.identity);
        SetUpPlayerCamera(camera, player);

        if (!EditorSceneManager.SaveScene(scene, ScenePath)) throw new InvalidOperationException($"Failed to save {ScenePath}.");
    }

    static void CreateFloor(Material material)
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.position = new Vector3(0f, -0.5f, 0f);
        floor.transform.localScale = new Vector3(20f, 1f, 20f);
        floor.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    // Continuous dynamic detection: a cube falling 3 m moves ~0.15 m per physics step, more than the
    // 6 cm seat is thick, so plain discrete detection could let it pass through.
    static void CreateFallingCubes(Transform chair, Material material)
    {
        for (int i = 0; i < FallingCubes.Length; i++)
        {
            (Vector3 offset, float size, Vector3 euler) = FallingCubes[i];
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = $"Falling Cube {i + 1}";
            cube.transform.SetPositionAndRotation(chair.TransformPoint(offset), Quaternion.Euler(euler));
            cube.transform.localScale = Vector3.one * size;
            cube.GetComponent<MeshRenderer>().sharedMaterial = material;

            var body = cube.AddComponent<Rigidbody>();
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }
    }

    // The player is not a prefab, so the Game scene is opened only to copy it with all its components
    // (movement, animation, footsteps) and is closed again without saving.
    static GameObject CopyPlayerFromGame()
    {
        Scene game = EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Additive);
        GameObject source = game.GetRootGameObjects().SingleOrDefault(root => root.name == "Player");
        if (source == null) throw new InvalidOperationException($"{GameScenePath} has no root object 'Player'.");

        // Instantiate puts the copy into the active scene, which is still CollisionTest.
        GameObject player = Object.Instantiate(source);
        player.name = "Player";
        if (!EditorSceneManager.CloseScene(game, true)) throw new InvalidOperationException($"Failed to close {GameScenePath}.");

        // QuickSave needs the HUD and the SoundManager of the Game scene, which this test scene does not have.
        var quickSave = player.GetComponent<QuickSave>();
        if (quickSave == null) throw new InvalidOperationException("The Game player has no QuickSave component.");
        Object.DestroyImmediate(quickSave);
        return player;
    }

    static void SetUpPlayerCamera(Camera camera, GameObject player)
    {
        camera.transform.SetPositionAndRotation(new Vector3(0f, 3f, -6f), Quaternion.Euler(15f, 0f, 0f));
        var playerCamera = camera.gameObject.AddComponent<ThirdPersonCamera>();
        EditorWiring.SetReference(playerCamera, "target", player.transform);
        EditorWiring.SetReference(player.GetComponent<PlayerController>(), "cameraTransform", camera.transform);
        camera.gameObject.AddComponent<CursorLock>();
    }

    static void AddSceneToBuildSettings()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        if (scenes.Any(scene => scene.path == ScenePath)) return;
        EditorBuildSettings.scenes = scenes.Append(new EditorBuildSettingsScene(ScenePath, true)).ToArray();
    }

    // ---------- Helpers ----------

    static Material CreateLitMaterial(string path, Color color, float smoothness)
    {
        Shader lit = Shader.Find("Universal Render Pipeline/Lit");
        if (lit == null) throw new InvalidOperationException("Shader 'Universal Render Pipeline/Lit' not found.");

        var material = new Material(lit) { color = color };
        if (!material.HasProperty(SmoothnessProperty)) throw new InvalidOperationException($"URP Lit has no {SmoothnessProperty}.");
        material.SetFloat(SmoothnessProperty, smoothness);
        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{name}")) AssetDatabase.CreateFolder(parent, name);
    }
}
