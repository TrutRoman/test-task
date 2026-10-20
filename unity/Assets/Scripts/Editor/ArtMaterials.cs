using System;
using UnityEditor;
using UnityEngine;

// Every material the scenes use, with colors taken from Palette.
public sealed class ArtMaterials
{
    public static readonly Color SkyTop = Palette.Lighten(Palette.Lavender, 0.1f);
    public static readonly Color SkyHorizon = Palette.Lighten(Palette.Peach, 0.55f);
    public static readonly Color SkyBelowHorizon = Palette.Lighten(Palette.Mint, 0.35f);

    const string SmoothnessProperty = "_Smoothness";
    const string SkyTextureProperty = "_MainTex";
    const string UnlitTextureProperty = "_BaseMap";
    const string SurfaceProperty = "_Surface";
    const string BlendProperty = "_Blend";
    const int SkyTextureHeight = 256;
    const int BlobTextureSize = 64;
    const float BlobOpacity = 0.4f;

    public Material Grass;
    public Material Stairs;
    public Material Obstacle;
    public Material Accent;
    public Material Rock;
    public Material Stone;
    public Material Wood;
    public Material FoliageMint;
    public Material FoliageRose;
    public Material CharacterJacket;
    public Material CharacterPants;
    public Material CharacterSkin;
    public Material CharacterScarf;
    public Material CharacterDark;
    public Material BlobShadow;
    public Material Sky;

    public static ArtMaterials Create()
    {
        Shader lit = FindShader("Universal Render Pipeline/Lit");
        return new ArtMaterials
        {
            Grass = CreateLit(lit, "Grass", Palette.Mint, 0.05f),
            Stairs = CreateLit(lit, "Stairs", Palette.Lavender, 0.3f),
            Obstacle = CreateLit(lit, "Obstacle", Palette.Peach, 0.2f),
            Accent = CreateLit(lit, "Accent", Palette.Rose, 0.25f),
            Rock = CreateLit(lit, "Rock", Palette.Darken(Palette.Lavender, 0.2f), 0.1f),
            Stone = CreateLit(lit, "Stone", Palette.Lighten(Palette.Peach, 0.5f), 0.3f),
            Wood = CreateLit(lit, "Wood", Palette.Darken(Palette.Peach, 0.4f), 0.1f),
            FoliageMint = CreateLit(lit, "Foliage Mint", Palette.Darken(Palette.Mint, 0.12f), 0.05f),
            FoliageRose = CreateLit(lit, "Foliage Rose", Palette.Lighten(Palette.Rose, 0.15f), 0.05f),
            CharacterJacket = CreateLit(lit, "Character Jacket", Palette.Lavender, 0.3f),
            CharacterPants = CreateLit(lit, "Character Pants", Palette.Lighten(Palette.Ink, 0.3f), 0.2f),
            CharacterSkin = CreateLit(lit, "Character Skin", Palette.Lighten(Palette.Peach, 0.35f), 0.35f),
            CharacterScarf = CreateLit(lit, "Character Scarf", Palette.Rose, 0.2f),
            CharacterDark = CreateLit(lit, "Character Dark", Palette.Ink, 0.5f),
            BlobShadow = CreateBlobShadow(),
            Sky = CreateSky(),
        };
    }

    static Material CreateLit(Shader shader, string name, Color color, float smoothness)
    {
        var material = new Material(shader) { name = name, color = color };
        RequireProperty(material, SmoothnessProperty);
        material.SetFloat(SmoothnessProperty, smoothness);
        return GeneratedAssets.Save(material, $"Materials/{name}.mat");
    }

    // Unlit, alpha-blended, tinted with Ink: a soft dark spot that never looks black.
    static Material CreateBlobShadow()
    {
        Texture2D texture = GeneratedAssets.SavePng(CreateBlobTexture(), "Textures/Blob Shadow.png", importer =>
        {
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        });

        var material = new Material(FindShader("Universal Render Pipeline/Unlit")) { name = "Blob Shadow" };
        RequireProperty(material, UnlitTextureProperty);
        RequireProperty(material, SurfaceProperty);
        RequireProperty(material, BlendProperty);
        material.SetTexture(UnlitTextureProperty, texture);
        material.SetFloat(SurfaceProperty, (float)BaseShaderGUI.SurfaceType.Transparent);
        material.SetFloat(BlendProperty, (float)BaseShaderGUI.BlendMode.Alpha);
        material.color = new Color(Palette.Ink.r, Palette.Ink.g, Palette.Ink.b, BlobOpacity);
        // Applies blend states, keywords and render queue exactly like the URP material inspector.
        BaseShaderGUI.SetMaterialKeywords(material);
        return GeneratedAssets.Save(material, "Materials/Blob Shadow.mat");
    }

    // White with alpha that fades from the center to the edge.
    static Texture2D CreateBlobTexture()
    {
        var texture = new Texture2D(BlobTextureSize, BlobTextureSize, TextureFormat.RGBA32, false);
        float center = (BlobTextureSize - 1) / 2f;
        for (int y = 0; y < BlobTextureSize; y++)
        {
            for (int x = 0; x < BlobTextureSize; x++)
            {
                float distance = new Vector2(x - center, y - center).magnitude / center;
                float alpha = 1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.25f, 1f, distance));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();
        return texture;
    }

    // The built-in Panoramic skybox wraps a vertical gradient texture around the scene.
    static Material CreateSky()
    {
        Texture2D gradient = GeneratedAssets.SavePng(CreateSkyGradient(), "Textures/Sky Gradient.png", importer =>
        {
            importer.mipmapEnabled = false;
            importer.wrapModeU = TextureWrapMode.Repeat;
            importer.wrapModeV = TextureWrapMode.Clamp;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
        });

        var material = new Material(FindShader("Skybox/Panoramic")) { name = "Sky" };
        RequireProperty(material, SkyTextureProperty);
        material.SetTexture(SkyTextureProperty, gradient);
        return GeneratedAssets.Save(material, "Materials/Sky.mat");
    }

    // Row 0 looks straight down, the middle row is the horizon, the last row looks straight up.
    static Texture2D CreateSkyGradient()
    {
        var texture = new Texture2D(4, SkyTextureHeight, TextureFormat.RGBA32, false);
        for (int y = 0; y < SkyTextureHeight; y++)
        {
            float v = y / (SkyTextureHeight - 1f);
            Color color = v < 0.5f
                ? Color.Lerp(SkyBelowHorizon, SkyHorizon, Mathf.Pow(v * 2f, 3f))
                : Color.Lerp(SkyHorizon, SkyTop, Mathf.Pow((v - 0.5f) * 2f, 0.6f));
            for (int x = 0; x < texture.width; x++) texture.SetPixel(x, y, color);
        }
        texture.Apply();
        return texture;
    }

    static Shader FindShader(string name)
    {
        Shader shader = Shader.Find(name);
        if (shader == null) throw new InvalidOperationException($"Shader '{name}' not found.");
        return shader;
    }

    static void RequireProperty(Material material, string property)
    {
        if (!material.HasProperty(property))
            throw new InvalidOperationException($"Shader '{material.shader.name}' has no property {property}.");
    }
}
