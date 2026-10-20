using UnityEngine;

public static class GameSettings
{
    public const float DefaultVolume = 1f;
    public const float DefaultSensitivity = 0.15f;

    const string VolumeKey = "volume";
    const string SensitivityKey = "sensitivity";

    public static float Volume
    {
        get => PlayerPrefs.GetFloat(VolumeKey, DefaultVolume);
        set
        {
            PlayerPrefs.SetFloat(VolumeKey, value);
            AudioListener.volume = value;
        }
    }

    public static float Sensitivity
    {
        get => PlayerPrefs.GetFloat(SensitivityKey, DefaultSensitivity);
        set => PlayerPrefs.SetFloat(SensitivityKey, value);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedVolume()
    {
        AudioListener.volume = Volume;
    }
}
