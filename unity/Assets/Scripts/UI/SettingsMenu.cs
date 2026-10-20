using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] ThirdPersonCamera playerCamera;

    void Awake()
    {
        if (volumeSlider == null) throw new MissingReferenceException("SettingsMenu: volumeSlider is not assigned.");
        if (sensitivitySlider == null) throw new MissingReferenceException("SettingsMenu: sensitivitySlider is not assigned.");
        if (playerCamera == null) throw new MissingReferenceException("SettingsMenu: playerCamera is not assigned.");
    }

    void OnEnable()
    {
        volumeSlider.SetValueWithoutNotify(GameSettings.Volume);
        sensitivitySlider.SetValueWithoutNotify(GameSettings.Sensitivity);
    }

    void OnDisable()
    {
        PlayerPrefs.Save();
    }

    public void SetVolume(float value)
    {
        GameSettings.Volume = value;
    }

    public void SetSensitivity(float value)
    {
        GameSettings.Sensitivity = value;
        playerCamera.SetSensitivity(value);
    }
}
