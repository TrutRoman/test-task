using TMPro;
using UnityEngine;

// The one place for UI typography and sizes (colors come from Palette).
// The scene builder reads it; Tools → Apply UI Font pushes a changed font into the built scenes.
[CreateAssetMenu(menuName = "Test Task/UI Style", fileName = "UiStyle")]
public class UiStyle : ScriptableObject
{
    [Header("Typography")]
    public TMP_FontAsset font;
    public float titleSize = 88f;
    public float headingSize = 60f;
    public float buttonTextSize = 34f;
    public float labelSize = 30f;
    public float hintSize = 24f;

    [Header("Layout (canvas units at 1920×1080)")]
    public Vector2 buttonSize = new(540f, 76f);
    public float sliderWidth = 540f;
    public float spacing = 18f;
    public float panelPadding = 48f;
    public float cornerRadius = 22f;
}
