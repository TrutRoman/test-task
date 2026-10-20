using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// Tools → Generate Audio: synthesizes every game sound into Assets/Audio as 16-bit WAV.
// The scene builder only loads these files; run this first after changing a recipe.
public static class AudioGenerator
{
    public const string Hover = "UI Hover";
    public const string Click = "UI Click";
    public const string WalkStep = "Step Walk";
    public const string RunStep = "Step Run";
    public const string Jump = "Jump";
    public const string Land = "Land";
    public const string Save = "Save";
    public const string Load = "Load";
    public const string Pause = "Pause";
    public const string Resume = "Resume";
    public const string Music = "Music Loop";

    const string Folder = "Assets/Audio";

    [MenuItem("Tools/Generate Audio")]
    public static void Generate()
    {
        if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "Audio");

        Write(Hover, SoundEffects.Hover());
        Write(Click, SoundEffects.Click());
        Write(WalkStep, SoundEffects.WalkStep());
        Write(RunStep, SoundEffects.RunStep());
        Write(Jump, SoundEffects.Jump());
        Write(Land, SoundEffects.Land());
        Write(Save, SoundEffects.Save());
        Write(Load, SoundEffects.Load());
        Write(Pause, SoundEffects.Pause());
        Write(Resume, SoundEffects.Resume());
        Write(Music, MusicLoop.Create());
        Debug.Log($"AudioGenerator: wrote 11 clips to {Folder}.");
    }

    public static AudioClip LoadClip(string name)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(PathOf(name));
        if (clip == null) throw new InvalidOperationException($"{PathOf(name)} is missing: run Tools → Generate Audio first.");
        return clip;
    }

    // PCM keeps every clip sample-exact: the music loops without a gap and short effects start instantly.
    static void Write(string name, float[] samples)
    {
        string path = PathOf(name);
        File.WriteAllBytes(path, AudioSynth.EncodeWav(samples));
        AssetDatabase.ImportAsset(path);

        var importer = (AudioImporter)AssetImporter.GetAtPath(path);
        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        settings.loadType = AudioClipLoadType.DecompressOnLoad;
        settings.compressionFormat = AudioCompressionFormat.PCM;
        importer.defaultSampleSettings = settings;
        importer.SaveAndReimport();
    }

    static string PathOf(string name)
    {
        return $"{Folder}/{name}.wav";
    }
}
