using UnityEngine;

// Fades the panel in every time it is shown. Uses unscaled time, so it also works while the game is paused.
[RequireComponent(typeof(CanvasGroup))]
public class UiFade : MonoBehaviour
{
    [SerializeField] float duration = 0.25f;

    CanvasGroup group;
    float elapsed;

    void Awake()
    {
        group = GetComponent<CanvasGroup>();
    }

    void OnEnable()
    {
        elapsed = 0f;
        group.alpha = 0f;
    }

    void Update()
    {
        if (elapsed >= duration) return;

        elapsed += Time.unscaledDeltaTime;
        group.alpha = Mathf.Clamp01(elapsed / duration);
    }
}
