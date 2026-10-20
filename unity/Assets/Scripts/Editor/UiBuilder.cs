using TMPro;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

// Builds every UI element in one style: font and sizes from UiStyle, colors from Palette,
// rounded corners from the generated 9-sliced sprite.
public sealed class UiBuilder
{
    public const string GameTitle = "Тестік гейм";

    static readonly Vector2 ReferenceResolution = new(1920f, 1080f);

    static readonly Color TextColor = Palette.Ink;
    static readonly Color MutedTextColor = Palette.Lighten(Palette.Ink, 0.25f);
    static readonly Color PanelColor = WithAlpha(Palette.Lighten(Palette.Peach, 0.78f), 0.97f);
    static readonly Color DimColor = WithAlpha(Palette.Ink, 0.55f);
    static readonly Color PrimaryColor = Palette.Rose;
    static readonly Color SecondaryColor = Palette.Lighten(Palette.Lavender, 0.35f);
    static readonly Color DisabledColor = WithAlpha(Palette.Lighten(Palette.Ink, 0.7f), 0.7f);
    static readonly Color TrackColor = Palette.Lighten(Palette.Ink, 0.72f);
    static readonly Color KnobColor = Palette.Darken(Palette.Rose, 0.25f);
    static readonly Color HudColor = WithAlpha(Palette.Ink, 0.55f);
    static readonly Color HudTextColor = Palette.Lighten(Palette.Peach, 0.85f);
    static readonly Color KeyFaceColor = Palette.Lighten(Palette.Peach, 0.85f);
    static readonly Color KeyEdgeColor = Palette.Darken(Palette.Peach, 0.35f);
    static readonly Color ToastColor = Palette.Mint;

    const float MenuPanelLeftMargin = 160f;
    const float HudBottomMargin = 24f;
    const float HudGroupSpacing = 28f;
    const float ToastTopMargin = 32f;
    const float KeyHeight = 44f;
    const float KeyDepth = 4f;
    const float KeyCornerRadius = 9f;
    const float KeyTextSize = 22f;
    const float KeyWidthPerLetter = 18f;
    const float TrackHeight = 12f;
    const float KnobSize = 34f;

    enum ButtonKind { Primary, Secondary }

    readonly UiStyle style;
    readonly UiSprites sprites;

    public UiBuilder(UiStyle style, UiSprites sprites)
    {
        this.style = style;
        this.sprites = sprites;
    }

    // ---------- Screens ----------

    public Transform CreateCanvas()
    {
        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;
        return canvasObject.transform;
    }

    // The panel sits on the left, so the orbiting level stays visible on the right.
    public void CreateMainMenu(Transform canvas, SoundManager sounds)
    {
        var menu = canvas.gameObject.AddComponent<MainMenu>();
        RectTransform panel = CreateFrame(canvas, "Menu Panel");
        Place(panel, new Vector2(0f, 0.5f), new Vector2(MenuPanelLeftMargin, 0f));
        panel.gameObject.AddComponent<UiFade>();

        CreateText(panel, "Title", GameTitle, style.titleSize, FontStyles.Bold, TextColor);
        Button play = CreateButton(panel, "Play Button", "Грати", ButtonKind.Primary, sounds);
        Button quit = CreateButton(panel, "Quit Button", "Вийти", ButtonKind.Secondary, sounds);
        UnityEventTools.AddPersistentListener(play.onClick, menu.Play);
        UnityEventTools.AddPersistentListener(quit.onClick, menu.Quit);
        Rebuild(panel);
    }

    // Keycap hints in a compact bar at the bottom of the screen.
    public void CreateControlsHud(Transform canvas)
    {
        RectTransform hud = CreateRow(canvas, "Controls Hint", HudColor, new RectOffset(20, 20, 10, 10), HudGroupSpacing);
        Place(hud, new Vector2(0.5f, 0f), new Vector2(0f, HudBottomMargin));

        CreateHint(hud, "рух", "W", "A", "S", "D");
        CreateHint(hud, "біг", "Shift");
        CreateHint(hud, "присід", "Ctrl");
        CreateHint(hud, "стрибок", "Space");
        CreateHint(hud, "зберегти", "F5");
        CreateHint(hud, "пауза", "Esc");
        Rebuild(hud);
    }

    // A pill at the top of the screen; QuickSave shows and hides it.
    public GameObject CreateToast(Transform canvas, string text)
    {
        RectTransform toast = CreateRow(canvas, "Saved Message", ToastColor, new RectOffset(32, 32, 12, 12), 0f);
        Place(toast, new Vector2(0.5f, 1f), new Vector2(0f, -ToastTopMargin));
        CreateText(toast, "Label", text, style.labelSize, FontStyles.Bold, TextColor);
        Rebuild(toast);

        SetCornerRadius(toast.GetComponent<Image>(), toast.rect.height / 2f);
        toast.gameObject.SetActive(false);
        return toast.gameObject;
    }

    public void CreatePauseMenu(Transform canvas, PlayerController player, ThirdPersonCamera playerCamera, QuickSave quickSave, SoundManager sounds)
    {
        GameObject pausePanel = CreateOverlay(canvas, "Pause Panel");
        RectTransform pauseFrame = CreateFrame(pausePanel.transform, "Frame");
        CreateText(pauseFrame, "Title", "Пауза", style.headingSize, FontStyles.Bold, TextColor);
        Button continueButton = CreateButton(pauseFrame, "Continue Button", "Продовжити", ButtonKind.Primary, sounds);
        Button loadButton = CreateButton(pauseFrame, "Load Button", "Завантажити збереження", ButtonKind.Secondary, sounds);
        Button mainMenuButton = CreateButton(pauseFrame, "Main Menu Button", "Головне меню", ButtonKind.Secondary, sounds);
        Button settingsButton = CreateButton(pauseFrame, "Settings Button", "Налаштування", ButtonKind.Secondary, sounds);

        GameObject settingsPanel = CreateOverlay(canvas, "Settings Panel");
        RectTransform settingsFrame = CreateFrame(settingsPanel.transform, "Frame");
        CreateText(settingsFrame, "Title", "Налаштування", style.headingSize, FontStyles.Bold, TextColor);
        CreateFieldLabel(settingsFrame, "Volume Label", "Гучність");
        Slider volumeSlider = CreateSlider(settingsFrame, "Volume Slider", 0f, 1f, GameSettings.DefaultVolume);
        CreateFieldLabel(settingsFrame, "Sensitivity Label", "Чутливість камери");
        Slider sensitivitySlider = CreateSlider(settingsFrame, "Sensitivity Slider", 0.02f, 0.5f, GameSettings.DefaultSensitivity);
        Button backButton = CreateButton(settingsFrame, "Back Button", "Назад", ButtonKind.Secondary, sounds);

        var settingsMenu = settingsPanel.AddComponent<SettingsMenu>();
        EditorWiring.SetReference(settingsMenu, "volumeSlider", volumeSlider);
        EditorWiring.SetReference(settingsMenu, "sensitivitySlider", sensitivitySlider);
        EditorWiring.SetReference(settingsMenu, "playerCamera", playerCamera);
        UnityEventTools.AddPersistentListener<float>(volumeSlider.onValueChanged, settingsMenu.SetVolume);
        UnityEventTools.AddPersistentListener<float>(sensitivitySlider.onValueChanged, settingsMenu.SetSensitivity);

        var pauseMenu = canvas.gameObject.AddComponent<PauseMenu>();
        EditorWiring.SetReference(pauseMenu, "pausePanel", pausePanel);
        EditorWiring.SetReference(pauseMenu, "settingsPanel", settingsPanel);
        EditorWiring.SetReference(pauseMenu, "loadButton", loadButton);
        EditorWiring.SetReference(pauseMenu, "player", player);
        EditorWiring.SetReference(pauseMenu, "playerCamera", playerCamera);
        EditorWiring.SetReference(pauseMenu, "quickSave", quickSave);
        EditorWiring.SetReference(pauseMenu, "sounds", sounds);
        UnityEventTools.AddPersistentListener(continueButton.onClick, pauseMenu.Resume);
        UnityEventTools.AddPersistentListener(loadButton.onClick, pauseMenu.LoadSave);
        UnityEventTools.AddPersistentListener(mainMenuButton.onClick, pauseMenu.GoToMainMenu);
        UnityEventTools.AddPersistentListener(settingsButton.onClick, pauseMenu.OpenSettings);
        UnityEventTools.AddPersistentListener(backButton.onClick, pauseMenu.CloseSettings);

        Rebuild(pauseFrame);
        Rebuild(settingsFrame);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    public static void CreateEventSystem()
    {
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    // ---------- Building blocks ----------

    // A full-screen dimmed layer that fades in each time it is shown.
    GameObject CreateOverlay(Transform parent, string name)
    {
        RectTransform overlay = CreateRect(parent, name);
        Stretch(overlay, Vector2.zero, Vector2.zero);
        overlay.gameObject.AddComponent<Image>().color = DimColor;
        overlay.gameObject.AddComponent<UiFade>();
        return overlay.gameObject;
    }

    // A rounded panel that stacks its children vertically and sizes itself to them.
    RectTransform CreateFrame(Transform parent, string name)
    {
        Image frame = CreateImage(parent, name, sprites.Rounded, PanelColor);
        int padding = Mathf.RoundToInt(style.panelPadding);
        var layout = frame.gameObject.AddComponent<VerticalLayoutGroup>();
        ConfigureLayout(layout, new RectOffset(padding, padding, padding, padding), style.spacing, TextAnchor.UpperCenter);
        FitToContent(frame.gameObject);
        return frame.rectTransform;
    }

    // A rounded bar that lines up its children horizontally and sizes itself to them.
    RectTransform CreateRow(Transform parent, string name, Color color, RectOffset padding, float spacing)
    {
        Image row = CreateImage(parent, name, sprites.Rounded, color);
        var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
        ConfigureLayout(layout, padding, spacing, TextAnchor.MiddleCenter);
        FitToContent(row.gameObject);
        return row.rectTransform;
    }

    void CreateHint(Transform parent, string action, params string[] keys)
    {
        RectTransform group = CreateRect(parent, $"Hint {action}");
        ConfigureLayout(group.gameObject.AddComponent<HorizontalLayoutGroup>(), new RectOffset(), 6f, TextAnchor.MiddleLeft);
        foreach (string key in keys) CreateKeycap(group, key);
        CreateText(group, "Label", action, style.hintSize, FontStyles.Normal, HudTextColor);
    }

    // A darker base with a lighter face lifted above it, like a keyboard key seen from the front.
    void CreateKeycap(Transform parent, string key)
    {
        Image edge = CreateImage(parent, $"Key {key}", sprites.Rounded, KeyEdgeColor);
        SetCornerRadius(edge, KeyCornerRadius);
        var size = edge.gameObject.AddComponent<LayoutElement>();
        size.preferredHeight = KeyHeight;
        size.preferredWidth = Mathf.Max(KeyHeight, KeyWidthPerLetter * key.Length + 20f);

        Image face = CreateImage(edge.transform, "Face", sprites.Rounded, KeyFaceColor);
        SetCornerRadius(face, KeyCornerRadius);
        Stretch(face.rectTransform, new Vector2(0f, KeyDepth), Vector2.zero);

        TextMeshProUGUI label = CreateText(face.transform, "Label", key, KeyTextSize, FontStyles.Bold, TextColor);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
    }

    Button CreateButton(Transform parent, string name, string text, ButtonKind kind, SoundManager sounds)
    {
        // The image stays white: the button's color tint gives it the normal, hover and pressed colors.
        Image background = CreateImage(parent, name, sprites.Rounded, Color.white);
        var size = background.gameObject.AddComponent<LayoutElement>();
        size.preferredWidth = style.buttonSize.x;
        size.preferredHeight = style.buttonSize.y;

        var button = background.gameObject.AddComponent<Button>();
        button.targetGraphic = background;
        button.colors = TintColors(kind == ButtonKind.Primary ? PrimaryColor : SecondaryColor);
        EditorWiring.SetReference(background.gameObject.AddComponent<ButtonSounds>(), "sounds", sounds);

        TextMeshProUGUI label = CreateText(background.transform, "Label", text, style.buttonTextSize, FontStyles.Bold, TextColor);
        Stretch(label.rectTransform, Vector2.zero, Vector2.zero);
        return button;
    }

    void CreateFieldLabel(Transform parent, string name, string text)
    {
        TextMeshProUGUI label = CreateText(parent, name, text, style.labelSize, FontStyles.Normal, MutedTextColor);
        label.alignment = TextAlignmentOptions.Left;
        label.gameObject.AddComponent<LayoutElement>().preferredWidth = style.sliderWidth;
    }

    // A thin rounded track, a fill in the accent color and a round knob.
    Slider CreateSlider(Transform parent, string name, float min, float max, float value)
    {
        RectTransform root = CreateRect(parent, name);
        var size = root.gameObject.AddComponent<LayoutElement>();
        size.preferredWidth = style.sliderWidth;
        size.preferredHeight = KnobSize;

        Image track = CreateImage(root, "Track", sprites.Rounded, TrackColor);
        SetCornerRadius(track, TrackHeight / 2f);
        HorizontalBand(track.rectTransform, TrackHeight, 0f);

        // Fill and knob areas are inset by the knob radius, so the fill ends under the knob center.
        // The fill reaches back over the inset to start at the track's left edge; Slider only moves its anchors.
        RectTransform fillArea = CreateRect(root, "Fill Area");
        HorizontalBand(fillArea, TrackHeight, KnobSize / 2f);
        Image fill = CreateImage(fillArea, "Fill", sprites.Rounded, PrimaryColor);
        SetCornerRadius(fill, TrackHeight / 2f);
        Stretch(fill.rectTransform, new Vector2(-KnobSize / 2f, 0f), Vector2.zero);

        RectTransform knobArea = CreateRect(root, "Knob Area");
        Stretch(knobArea, new Vector2(KnobSize / 2f, 0f), new Vector2(-KnobSize / 2f, 0f));
        Image knob = CreateImage(knobArea, "Knob", sprites.Circle, Color.white);
        knob.type = Image.Type.Simple;
        knob.rectTransform.anchorMin = Vector2.zero;
        knob.rectTransform.anchorMax = Vector2.up;
        knob.rectTransform.sizeDelta = new Vector2(KnobSize, 0f);

        var slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill.rectTransform;
        slider.handleRect = knob.rectTransform;
        slider.targetGraphic = knob;
        slider.colors = TintColors(KnobColor);
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;
        return slider;
    }

    TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, FontStyles fontStyle, Color color)
    {
        RectTransform rect = CreateRect(parent, name);
        var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.font = style.font;
        label.text = text;
        label.fontSize = size;
        label.fontStyle = fontStyle;
        label.color = color;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        return label;
    }

    Image CreateImage(Transform parent, string name, Sprite sprite, Color color)
    {
        var image = CreateRect(parent, name).gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.type = Image.Type.Sliced;
        image.color = color;
        return image;
    }

    // ---------- Helpers ----------

    // The rounded sprite has corners of style.cornerRadius; the multiplier scales them to the wanted radius.
    void SetCornerRadius(Image image, float radius)
    {
        image.pixelsPerUnitMultiplier = style.cornerRadius / radius;
    }

    static ColorBlock TintColors(Color baseColor)
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = baseColor;
        colors.highlightedColor = Palette.Lighten(baseColor, 0.35f);
        colors.pressedColor = Palette.Darken(baseColor, 0.2f);
        // A clicked button stays selected; it should not keep looking hovered.
        colors.selectedColor = baseColor;
        colors.disabledColor = DisabledColor;
        colors.fadeDuration = 0.08f;
        return colors;
    }

    static void ConfigureLayout(HorizontalOrVerticalLayoutGroup layout, RectOffset padding, float spacing, TextAnchor alignment)
    {
        layout.padding = padding;
        layout.spacing = spacing;
        layout.childAlignment = alignment;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
    }

    static void FitToContent(GameObject target)
    {
        var fitter = target.AddComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    // Layout normally runs on the next canvas update; the builder needs final sizes right away.
    static void Rebuild(RectTransform root)
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(root);
    }

    static RectTransform CreateRect(Transform parent, string name)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    static void Place(RectTransform rect, Vector2 anchor, Vector2 position)
    {
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
    }

    static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = offsetMin;
        rect.offsetMax = offsetMax;
    }

    // A band of fixed height across the middle of the parent, inset from both sides.
    static void HorizontalBand(RectTransform rect, float height, float inset)
    {
        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(1f, 0.5f);
        rect.sizeDelta = new Vector2(-inset * 2f, height);
        rect.anchoredPosition = Vector2.zero;
    }

    static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
