using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public const string MainMenuScene = "MainMenu";

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsPanel;
    [SerializeField] Button loadButton;
    [SerializeField] PlayerController player;
    [SerializeField] ThirdPersonCamera playerCamera;
    [SerializeField] QuickSave quickSave;
    [SerializeField] SoundManager sounds;

    InputAction pauseAction;
    bool isPaused;

    void Awake()
    {
        if (pausePanel == null) throw new MissingReferenceException("PauseMenu: pausePanel is not assigned.");
        if (settingsPanel == null) throw new MissingReferenceException("PauseMenu: settingsPanel is not assigned.");
        if (loadButton == null) throw new MissingReferenceException("PauseMenu: loadButton is not assigned.");
        if (player == null) throw new MissingReferenceException("PauseMenu: player is not assigned.");
        if (playerCamera == null) throw new MissingReferenceException("PauseMenu: playerCamera is not assigned.");
        if (quickSave == null) throw new MissingReferenceException("PauseMenu: quickSave is not assigned.");
        if (sounds == null) throw new MissingReferenceException("PauseMenu: sounds is not assigned.");

        pauseAction = InputSystem.actions.FindAction("Player/Pause", throwIfNotFound: true);
    }

    void Start()
    {
        Resume();
    }

    void Update()
    {
        if (!pauseAction.WasPressedThisFrame()) return;

        if (isPaused) Resume();
        else Pause();
    }

    public void Pause()
    {
        sounds.PlayPause();
        SetPaused(true);
        loadButton.interactable = SaveSystem.HasSave();
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void Resume()
    {
        // Start also calls Resume; the sound is only for leaving an actual pause.
        if (isPaused) sounds.PlayResume();
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        SetPaused(false);
    }

    public void LoadSave()
    {
        player.Teleport(SaveSystem.Load().position);
        sounds.PlayLoad();
        Resume();
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(MainMenuScene);
    }

    void SetPaused(bool paused)
    {
        isPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        // Gameplay scripts read input in Update, which still runs while timeScale is 0.
        player.enabled = !paused;
        playerCamera.enabled = !paused;
        quickSave.enabled = !paused;
    }
}
