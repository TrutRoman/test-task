using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// One post-processing profile shared by both scenes, applied by a global volume.
// The overall effect is mild: softer highlights, a light vignette, a faint glow on sunlit edges.
public static class PostProcessBuilder
{
    public static VolumeProfile CreateProfile()
    {
        VolumeProfile profile = VolumeProfileFactory.CreateVolumeProfileAtPath(GeneratedAssets.PathOf("Rendering/Post Processing.asset"));

        // Only the brightest sunlit surfaces (HDR values above 0.9) glow a little.
        var bloom = VolumeProfileFactory.CreateVolumeComponent<Bloom>(profile, saveAsset: false);
        bloom.threshold.Override(0.9f);
        bloom.intensity.Override(0.35f);
        bloom.scatter.Override(0.6f);

        // Darkens the corners with the palette's Ink instead of black.
        var vignette = VolumeProfileFactory.CreateVolumeComponent<Vignette>(profile, saveAsset: false);
        vignette.color.Override(Palette.Ink);
        vignette.intensity.Override(0.15f);
        vignette.smoothness.Override(0.45f);

        // Neutral tonemapping compresses bright pastels and makes them grey; these values bring the palette
        // back to its designed brightness and saturation (picked by comparing renders with and without post-processing).
        var colorAdjustments = VolumeProfileFactory.CreateVolumeComponent<ColorAdjustments>(profile, saveAsset: false);
        colorAdjustments.postExposure.Override(0.5f);
        colorAdjustments.contrast.Override(12f);
        colorAdjustments.saturation.Override(30f);

        // Neutral changes hue and saturation the least, which keeps the pastel colors as designed.
        var tonemapping = VolumeProfileFactory.CreateVolumeComponent<Tonemapping>(profile, saveAsset: false);
        tonemapping.mode.Override(TonemappingMode.Neutral);

        // The overrides were changed after being added as sub-assets, so they must be saved explicitly.
        foreach (VolumeComponent component in profile.components) EditorUtility.SetDirty(component);
        EditorUtility.SetDirty(profile);
        AssetDatabase.SaveAssets();
        return profile;
    }

    public static void AddToScene(VolumeProfile profile, Camera camera)
    {
        var volume = new GameObject("Global Volume").AddComponent<Volume>();
        volume.isGlobal = true;
        volume.sharedProfile = profile;

        // URP cameras skip post-processing unless this flag is on.
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
    }
}
