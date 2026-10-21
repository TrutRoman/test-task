using UnityEngine;
using UnityEngine.InputSystem;

// Test scenes have no pause menu, so this locks the cursor for mouse look; Esc toggles it.
public class CursorLock : MonoBehaviour
{
    InputAction pauseAction;

    void Awake()
    {
        pauseAction = InputSystem.actions.FindAction("Player/Pause", throwIfNotFound: true);
    }

    void Start()
    {
        SetLocked(true);
    }

    void Update()
    {
        if (pauseAction.WasPressedThisFrame()) SetLocked(Cursor.lockState != CursorLockMode.Locked);
    }

    static void SetLocked(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }
}
