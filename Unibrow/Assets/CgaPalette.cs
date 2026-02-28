using UnityEngine;

public static class CgaPalette
{
    // CGA 16-color palette (approx sRGB)
    public static readonly Color32 Black       = new(0x00, 0x00, 0x00, 0xFF);
    public static readonly Color32 Blue        = new(0x00, 0x00, 0xAA, 0xFF);
    public static readonly Color32 Green       = new(0x00, 0xAA, 0x00, 0xFF);
    public static readonly Color32 Cyan        = new(0x00, 0xAA, 0xAA, 0xFF);
    public static readonly Color32 Red         = new(0xAA, 0x00, 0x00, 0xFF);
    public static readonly Color32 Magenta     = new(0xAA, 0x00, 0xAA, 0xFF);
    public static readonly Color32 Brown       = new(0xAA, 0x55, 0x00, 0xFF);
    public static readonly Color32 LightGray   = new(0xAA, 0xAA, 0xAA, 0xFF);

    public static readonly Color32 DarkGray    = new(0x55, 0x55, 0x55, 0xFF);
    public static readonly Color32 LightBlue   = new(0x55, 0x55, 0xFF, 0xFF);
    public static readonly Color32 LightGreen  = new(0x55, 0xFF, 0x55, 0xFF);
    public static readonly Color32 LightCyan   = new(0x55, 0xFF, 0xFF, 0xFF);
    public static readonly Color32 LightRed    = new(0xFF, 0x55, 0x55, 0xFF);
    public static readonly Color32 LightMagenta= new(0xFF, 0x55, 0xFF, 0xFF);
    public static readonly Color32 Yellow      = new(0xFF, 0xFF, 0x55, 0xFF);
    public static readonly Color32 White       = new(0xFF, 0xFF, 0xFF, 0xFF);

    public enum Pair
    {
        LightRed_Red,
        DarkGray_Black,
        LightBlue_Blue,
        LightGreen_Green,
        LightCyan_Cyan,
        LightMagenta_Magenta,
        Yellow_Brown,
        White_LightGray
    }

    public static (Color32 light, Color32 dark) GetPair(Pair p) => p switch
    {
        Pair.LightRed_Red         => (LightRed, Red),
        Pair.DarkGray_Black       => (DarkGray, Black),
        Pair.LightBlue_Blue       => (LightBlue, Blue),
        Pair.LightGreen_Green     => (LightGreen, Green),
        Pair.LightCyan_Cyan       => (LightCyan, Cyan),
        Pair.LightMagenta_Magenta => (LightMagenta, Magenta),
        Pair.Yellow_Brown         => (Yellow, Brown),
        Pair.White_LightGray      => (White, LightGray),
        _                         => (LightRed, Red)
    };
}