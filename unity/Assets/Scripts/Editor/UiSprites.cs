using UnityEditor;
using UnityEngine;

// Generated UI sprites: a 9-sliced rounded rectangle and a circle, both white so Image.color tints them.
public sealed class UiSprites
{
    const int CircleSize = 64;
    // Pixels of plain middle between the corners, stretched by 9-slicing.
    const int RoundedMiddle = 4;

    public Sprite Rounded;
    public Sprite Circle;

    // cornerRadius is in canvas units: at 100 pixels per unit one texture pixel is one canvas unit.
    public static UiSprites Create(float cornerRadius)
    {
        int radius = Mathf.RoundToInt(cornerRadius);
        int roundedSize = radius * 2 + RoundedMiddle;
        return new UiSprites
        {
            Rounded = SaveSprite(Rasterize(roundedSize, roundedSize / 2f - radius, radius), "Rounded", radius),
            Circle = SaveSprite(Rasterize(CircleSize, 0f, CircleSize / 2f - 1f), "Circle", 0),
        };
    }

    // Draws a white rounded square with anti-aliased edges using its signed distance.
    // innerHalf is the half size of the straight part, radius the corner radius.
    static Texture2D Rasterize(int size, float innerHalf, float radius)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                var q = new Vector2(Mathf.Abs(x + 0.5f - center) - innerHalf, Mathf.Abs(y + 0.5f - center) - innerHalf);
                float distance = Vector2.Max(q, Vector2.zero).magnitude + Mathf.Min(Mathf.Max(q.x, q.y), 0f) - radius;
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, Mathf.Clamp01(0.5f - distance)));
            }
        }
        texture.Apply();
        return texture;
    }

    static Sprite SaveSprite(Texture2D texture, string name, int border)
    {
        Texture2D imported = GeneratedAssets.SavePng(texture, $"Sprites/{name}.png", importer =>
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spriteBorder = new Vector4(border, border, border, border);
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;

            // 9-slicing needs the full-rect sprite mesh.
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        });
        return AssetDatabase.LoadAssetAtPath<Sprite>(AssetDatabase.GetAssetPath(imported));
    }
}
