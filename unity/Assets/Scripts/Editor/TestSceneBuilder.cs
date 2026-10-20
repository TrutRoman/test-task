using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Builds the MainMenu and Game scenes from scratch and registers them in Build Settings.
// Running it again overwrites both scenes and everything in Assets/Art/Generated.
// Assets/Art/UiStyle.asset is only read (see UiStyleAsset).
public static class TestSceneBuilder
{
    const string ScenesFolder = "Assets/Scenes";

    [MenuItem("Tools/Build Test Scene")]
    public static void Build()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        RequireTmpEssentials();

        // An empty scene first, so no open scene references the generated assets that are about to be replaced.
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GeneratedAssets.Reset();
        ArtMaterials materials = ArtMaterials.Create();
        EnvironmentMeshes meshes = EnvironmentMeshes.Create();
        VolumeProfile postProcessing = PostProcessBuilder.CreateProfile();
        UiStyle style = UiStyleAsset.LoadOrCreate();
        var ui = new UiBuilder(style, UiSprites.Create(style.cornerRadius));

        string gameScenePath = BuildGameScene(materials, meshes, postProcessing, ui);
        string mainMenuScenePath = BuildMainMenuScene(materials, meshes, postProcessing, ui);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(mainMenuScenePath, true),
            new EditorBuildSettingsScene(gameScenePath, true),
        };
        AssetDatabase.SaveAssets();
        Debug.Log($"TestSceneBuilder: built {mainMenuScenePath} and {gameScenePath} and added them to Build Settings.");
    }

    static void RequireTmpEssentials()
    {
        if (Resources.Load<TMP_Settings>("TMP Settings") == null)
            throw new InvalidOperationException("TMP Essential Resources are missing: Window > TextMeshPro > Import TMP Essential Resources.");
        if (TMP_Settings.defaultFontAsset == null)
            throw new InvalidOperationException("TMP Settings has no default font asset.");
    }

    // ---------- Scenes ----------

    static string BuildGameScene(ArtMaterials materials, EnvironmentMeshes meshes, VolumeProfile postProcessing, UiBuilder ui)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera camera = FindMainCamera();
        EnvironmentBuilder.Build(materials, meshes, FindSun());
        PostProcessBuilder.AddToScene(postProcessing, camera);
        SoundManager sounds = SoundBuilder.CreateSoundManager();
        PlayerController player = CreatePlayer(materials, camera.transform);
        ThirdPersonCamera playerCamera = AddThirdPersonCamera(camera, player.transform);

        Transform canvas = ui.CreateCanvas();
        ui.CreateControlsHud(canvas);
        GameObject savedMessage = ui.CreateToast(canvas, "Збережено");
        QuickSave quickSave = AddQuickSave(player, savedMessage, sounds);
        ui.CreatePauseMenu(canvas, player, playerCamera, quickSave, sounds);
        UiBuilder.CreateEventSystem();

        return SaveScene(scene, "Game");
    }

    // The menu shows the same level behind it, seen by a slowly orbiting camera.
    static string BuildMainMenuScene(ArtMaterials materials, EnvironmentMeshes meshes, VolumeProfile postProcessing, UiBuilder ui)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        Camera camera = FindMainCamera();
        EnvironmentBuilder.Build(materials, meshes, FindSun());
        PostProcessBuilder.AddToScene(postProcessing, camera);
        camera.gameObject.AddComponent<MenuCameraOrbit>().MoveToStart();
        SoundManager sounds = SoundBuilder.CreateSoundManager();

        ui.CreateMainMenu(ui.CreateCanvas(), sounds);
        UiBuilder.CreateEventSystem();

        return SaveScene(scene, "MainMenu");
    }

    static Camera FindMainCamera()
    {
        Camera camera = Camera.main;
        if (camera == null) throw new InvalidOperationException("The default scene setup has no Main Camera.");
        return camera;
    }

    static Light FindSun()
    {
        Light sun = Object.FindAnyObjectByType<Light>();
        if (sun == null) throw new InvalidOperationException("The default scene setup has no Directional Light.");
        return sun;
    }

    static string SaveScene(Scene scene, string name)
    {
        string path = $"{ScenesFolder}/{name}.unity";
        if (!EditorSceneManager.SaveScene(scene, path)) throw new InvalidOperationException($"Failed to save scene {path}.");
        return path;
    }

    // ---------- Player and camera ----------

    static PlayerController CreatePlayer(ArtMaterials materials, Transform cameraTransform)
    {
        var player = new GameObject("Player") { tag = "Player" };
        player.transform.position = new Vector3(0f, 0f, -6f);

        var controller = player.AddComponent<CharacterController>();
        controller.height = 2f;
        controller.radius = 0.4f;
        controller.center = new Vector3(0f, 1f, 0f);

        var movement = player.AddComponent<PlayerController>();
        EditorWiring.SetReference(movement, "cameraTransform", cameraTransform);
        CharacterBuilder.Build(player, materials);
        SoundBuilder.AddPlayerAudio(player);
        return movement;
    }

    static ThirdPersonCamera AddThirdPersonCamera(Camera camera, Transform target)
    {
        camera.transform.SetPositionAndRotation(new Vector3(0f, 3f, -11f), Quaternion.Euler(15f, 0f, 0f));
        var playerCamera = camera.gameObject.AddComponent<ThirdPersonCamera>();
        EditorWiring.SetReference(playerCamera, "target", target);
        return playerCamera;
    }

    static QuickSave AddQuickSave(PlayerController player, GameObject savedMessage, SoundManager sounds)
    {
        var quickSave = player.gameObject.AddComponent<QuickSave>();
        EditorWiring.SetReference(quickSave, "savedMessage", savedMessage);
        EditorWiring.SetReference(quickSave, "sounds", sounds);
        return quickSave;
    }
}
