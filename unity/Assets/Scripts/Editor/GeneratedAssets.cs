using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

// Owns Assets/Art/Generated. Everything inside is recreated from code on every build,
// so hand edits there are lost; tune the generators instead.
public static class GeneratedAssets
{
    const string ArtFolder = "Assets/Art";
    const string Root = ArtFolder + "/Generated";
    static readonly string[] Subfolders = { "Materials", "Meshes", "Textures", "Sprites", "Rendering" };

    public static void Reset()
    {
        if (AssetDatabase.IsValidFolder(Root) && !AssetDatabase.DeleteAsset(Root))
            throw new InvalidOperationException($"Failed to delete {Root}.");

        if (!AssetDatabase.IsValidFolder(ArtFolder)) AssetDatabase.CreateFolder("Assets", "Art");
        AssetDatabase.CreateFolder(ArtFolder, "Generated");
        foreach (string subfolder in Subfolders) AssetDatabase.CreateFolder(Root, subfolder);
    }

    // relativePath is inside Generated, for example "Materials/Grass.mat".
    public static T Save<T>(T asset, string relativePath) where T : Object
    {
        AssetDatabase.CreateAsset(asset, PathOf(relativePath));
        return asset;
    }

    public static string PathOf(string relativePath)
    {
        return $"{Root}/{relativePath}";
    }

    public static Texture2D SavePng(Texture2D texture, string relativePath, Action<TextureImporter> configureImport)
    {
        string path = $"{Root}/{relativePath}";
        File.WriteAllBytes(path, texture.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        configureImport(importer);
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }
}
