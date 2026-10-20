using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Hover and press sounds for a button. The press sound plays on pointer down, before the click:
// buttons that load another scene on click would otherwise cut their own sound off.
[RequireComponent(typeof(Button))]
public class ButtonSounds : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler
{
    [SerializeField] SoundManager sounds;

    Button button;

    void Awake()
    {
        if (sounds == null) throw new MissingReferenceException("ButtonSounds: sounds is not assigned.");

        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (button.IsInteractable()) sounds.PlayHover();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && button.IsInteractable()) sounds.PlayClick();
    }
}
