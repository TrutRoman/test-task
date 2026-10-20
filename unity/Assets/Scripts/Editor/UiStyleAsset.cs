using System;
using TMPro;
using UnityEditor;
using UnityEngine;

// Assets/Art/UiStyle.asset is created once with the default TMP font and then edited by hand.
// The builders only read it and never overwrite it.
public static class UiStyleAsset
{
    const string Path = "Assets/Art/UiStyle.asset";

    public static UiStyle LoadOrCreate()
    {
        var style = AssetDatabase.LoadAssetAtPath<UiStyle>(Path);
        if (style == null)
        {
            style = ScriptableObject.CreateInstance<UiStyle>();
            style.font = TMP_Settings.defaultFontAsset;
            AssetDatabase.CreateAsset(style, Path);
        }

        if (style.font == null) throw new InvalidOperationException($"{Path} has no font: assign a TMP font asset.");
        return style;
    }
}
