namespace DeepDungeonDex.Models;

public class Configuration : ISaveable
{
    public int Version { get; set; }
    public bool Clickthrough { get; set; }
    public bool HideRed { get; set; }
    public bool HideJob { get; set; }
    public bool HideFloor { get; set; }
    public bool Debug { get; set; }
    public int Locale { get; set; } = 0;
    public int FontSize { get; set; } = 16;
    public float Opacity { get; set; } = 1f;
    public bool LoadAll { get; set; }

    [JsonIgnore] public int PrevFontSize;
    [JsonIgnore] public int PrevLocale;
    [JsonIgnore] public float FontSizeScaled => FontSize * 1 / ImGui.GetIO().FontGlobalScale;
    [JsonIgnore] public float WindowSizeScaled => Math.Max(FontSizeScaled / 16f, 1f);
    [JsonIgnore] public Action<Configuration>? OnChange { get; set; }
    [JsonIgnore] public Action? OnSizeChange { get; set; }

    public NamedType? Save(string path)
    {
        if (FontSize != PrevFontSize)
        {
            PrevFontSize = FontSize;
            OnSizeChange?.Invoke();
        }
        if (PrevLocale != Locale)
        {
            PrevLocale = Locale;
            OnSizeChange?.Invoke();
        }
        OnChange?.Invoke(this);
        StorageHandler.SerializeJsonFile(path, this);
        return null;
    }

    public void Dispose()
    {
    }
}