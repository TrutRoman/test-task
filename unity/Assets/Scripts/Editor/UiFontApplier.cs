using System;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Puts the font from UiStyle into every text of the built scenes, without rebuilding them,
// so hand-made layout changes survive a font change.
public static class UiFontApplier
{
    [MenuItem("Tools/Apply UI Font")]
    public static void Apply()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        UiStyle style = UiStyleAsset.LoadOrCreate();
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(buildScene.path, OpenSceneMode.Single);
            int count = ApplyFont(scene, style.font);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException($"Failed to save scene {buildScene.path}.");
            Debug.Log($"Apply UI Font: {count} texts in {buildScene.path} now use {style.font.name}.");
        }
    }

    static int ApplyFont(Scene scene, TMP_FontAsset font)
    {
        int count = 0;
        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (TMP_Text text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                text.font = font;
                EditorUtility.SetDirty(text);
                count++;
            }
        }
        EditorSceneManager.MarkSceneDirty(scene);
        return count;
    }
}
