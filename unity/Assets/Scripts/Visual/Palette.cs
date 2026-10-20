using UnityEngine;

// The only source of colors for materials, lighting, sky and UI.
// Shades are derived from the five base colors instead of adding new ones.
public static class Palette
{
    public static readonly Color Mint = FromHex(0x9ED9C1);
    public static readonly Color Peach = FromHex(0xF7C8A8);
    public static readonly Color Rose = FromHex(0xF2A7B8);
    public static readonly Color Lavender = FromHex(0xB9A9E0);
    public static readonly Color Ink = FromHex(0x4B3F5E);

    public static Color Lighten(Color color, float amount)
    {
        return Color.Lerp(color, Color.white, amount);
    }

    // Darkens towards Ink rather than black, so shadows stay in the palette.
    public static Color Darken(Color color, float amount)
    {
        return Color.Lerp(color, Ink, amount);
    }

    static Color FromHex(int rgb)
    {
        return new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
    }
}
