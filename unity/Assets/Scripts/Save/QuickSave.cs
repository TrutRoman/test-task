using UnityEngine;
using UnityEngine.InputSystem;

public class QuickSave : MonoBehaviour
{
    [SerializeField] GameObject savedMessage;
    [SerializeField] SoundManager sounds;
    [SerializeField] float messageSeconds = 1.5f;

    InputAction saveAction;
    float hideMessageTime;

    void Awake()
    {
        if (savedMessage == null) throw new MissingReferenceException("QuickSave: savedMessage is not assigned.");
        if (sounds == null) throw new MissingReferenceException("QuickSave: sounds is not assigned.");

        saveAction = InputSystem.actions.FindAction("Player/QuickSave", throwIfNotFound: true);
    }

    void Update()
    {
        if (saveAction.WasPressedThisFrame()) Save();
        if (savedMessage.activeSelf && Time.time >= hideMessageTime) savedMessage.SetActive(false);
    }

    void Save()
    {
        SaveSystem.Save(new SaveData { position = transform.position });
        sounds.PlaySave();
        savedMessage.SetActive(true);
        hideMessageTime = Time.time + messageSeconds;
    }
}
