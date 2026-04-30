using System.IO;
using System.Numerics;
using Dalamud.Interface;

namespace DeepDungeonDex.Models;

public class Configuration
{
    private const byte Version = 5;
    public bool ClickThrough { get; set; }
    public bool HideRed { get; set; }
    public bool HideJob { get; set; }
    public bool HideFloor { get; set; }
    public bool HideSpawns { get; set; }
    public bool Debug { get; set; }
    public bool LoadAll { get; set; }
    public bool ShowCorrectionButton { get; set; }
    public int Locale { get; set; } = 0;
    public int FontSize { get; set; } = 16;
    public float Opacity { get; set; } = 1f;
    public ContentType EnabledContentTypes { get; set; } = ContentType.DeepDungeon;
    public Vector4 VulnerableColor { get; set; } = VulnerableColorDefault;
    public Vector4 UnknownColor { get; set; } = UnknownColorDefault;
    public Vector4 ResistantColor { get; set; } = ResistantColorDefault;

#region Not Saved Variables
    public int PrevLocale;
    public float RemoveScaling => 1 / ImGui.GetIO().FontGlobalScale;
    public float WindowSizeScaled => Math.Max(FontSizeScaled, 1f) * RemoveScaling;
    public float FontSizeScaled => FontSize / 16f;
    public Action<Configuration>? OnChange { get; set; }

    public static Vector4 VulnerableColorDefault = new(1, 1, 1, 1);
    public static Vector4 UnknownColorDefault = new(0.75f, 0.75f, 0.75f, 0.75f);
    public static Vector4 ResistantColorDefault = new(0.5f, 0.5f, 0.5f, 0.5f);
#endregion

    public void Save(string path)
    {
        if (PrevLocale != Locale && !LoadAll)
        {
            PrevLocale = Locale;
        }
        OnChange?.Invoke(this);
        var origPath = path;
        if (!path.EndsWith(".tmp"))
            path += ".tmp";
        Stream stream = File.Open(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite);
        BinaryWriter writer = new(stream);
        writer.Write(Version);
        byte flags = 0;
        flags |= (byte)(ClickThrough ? 1 << 1 : 0);
        flags |= (byte)(HideFloor ? 1 << 2 : 0);
        flags |= (byte)(HideSpawns ? 1 << 3 : 0);
        flags |= (byte)(Debug ? 1 << 4 : 0);
        flags |= (byte)(LoadAll ? 1 << 5 : 0);
        flags |= (byte)(ShowCorrectionButton ? 1 << 6 : 0);
        writer.Write(flags);
        writer.Write((uint)EnabledContentTypes);
        writer.Write(Locale);
        writer.Write(FontSize);
        writer.Write(Opacity);
        writer.Write(ColorHelpers.RgbaVector4ToUint(VulnerableColor));
        writer.Write(ColorHelpers.RgbaVector4ToUint(UnknownColor));
        writer.Write(ColorHelpers.RgbaVector4ToUint(ResistantColor));
        writer.Close();
        stream.Close();
        if (File.Exists(origPath))
            File.Delete(origPath);
        File.Move(path, origPath);
    }
}