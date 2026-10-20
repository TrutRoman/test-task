using UnityEngine;

// Puts the generated clips (see AudioGenerator) into the scenes.
public static class SoundBuilder
{
    public static SoundManager CreateSoundManager()
    {
        var root = new GameObject("Sound Manager");
        var manager = root.AddComponent<SoundManager>();
        EditorWiring.SetReference(manager, "uiSource", CreateSource(root.transform, "UI Source"));
        EditorWiring.SetReference(manager, "musicSource", CreateSource(root.transform, "Music Source"));
        EditorWiring.SetReference(manager, "hover", AudioGenerator.LoadClip(AudioGenerator.Hover));
        EditorWiring.SetReference(manager, "click", AudioGenerator.LoadClip(AudioGenerator.Click));
        EditorWiring.SetReference(manager, "save", AudioGenerator.LoadClip(AudioGenerator.Save));
        EditorWiring.SetReference(manager, "load", AudioGenerator.LoadClip(AudioGenerator.Load));
        EditorWiring.SetReference(manager, "pause", AudioGenerator.LoadClip(AudioGenerator.Pause));
        EditorWiring.SetReference(manager, "resume", AudioGenerator.LoadClip(AudioGenerator.Resume));
        EditorWiring.SetReference(manager, "music", AudioGenerator.LoadClip(AudioGenerator.Music));
        return manager;
    }

    public static void AddPlayerAudio(GameObject player)
    {
        Configure(player.AddComponent<AudioSource>());
        var playerAudio = player.AddComponent<PlayerAudio>();
        EditorWiring.SetReference(playerAudio, "walkStep", AudioGenerator.LoadClip(AudioGenerator.WalkStep));
        EditorWiring.SetReference(playerAudio, "runStep", AudioGenerator.LoadClip(AudioGenerator.RunStep));
        EditorWiring.SetReference(playerAudio, "jump", AudioGenerator.LoadClip(AudioGenerator.Jump));
        EditorWiring.SetReference(playerAudio, "land", AudioGenerator.LoadClip(AudioGenerator.Land));
    }

    static AudioSource CreateSource(Transform parent, string name)
    {
        var sourceObject = new GameObject(name);
        sourceObject.transform.SetParent(parent, false);
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        Configure(source);
        return source;
    }

    // 2D sound: the camera always stays near the player, so distance attenuation would add nothing.
    static void Configure(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = 0f;
    }
}
