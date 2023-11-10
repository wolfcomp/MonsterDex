using System.Buffers.Binary;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using Configuration = DeepDungeonDex.Models.Configuration;

namespace DeepDungeonDex.Font;

internal class Font : IDisposable
{
    private readonly StorageHandler _handler;
    private readonly IPluginLog _log;
    private ImFontConfigPtr _fontCfg;
    private ImFontConfigPtr _fontCfgMerge;
    private (GCHandle, int) _gameSymFont;
    private GCHandle _ranges;
    private GCHandle _jpRanges;
    private GCHandle _krRanges;
    private GCHandle _tcRanges;
    private GCHandle _scRanges;
    private (GCHandle, int) _regularFont;
    private (GCHandle, int) _jpFont;
    private (GCHandle, int) _krFont;
    private (GCHandle, int) _tcFont;
    private (GCHandle, int) _scFont;
    private GCHandle _symRange = GCHandle.Alloc(
        new ushort[] {
             0xE020,
             0xE0DB,
             0,
        },
        GCHandleType.Pinned
    );
    internal static ImFontPtr RegularFont;

    public unsafe Font(StorageHandler handler, IPluginLog log)
    {
        _handler = handler;
        _log = log;
        SetUpRanges();
        SetUpFonts();
        _fontCfg = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig()) { FontDataOwnedByAtlas = false };
        _fontCfgMerge = new ImFontConfigPtr(ImGuiNative.ImFontConfig_ImFontConfig()) { FontDataOwnedByAtlas = false, MergeMode = true };
        var gameSym = new HttpClient().GetAsync("https://img.finalfantasyxiv.com/lds/pc/global/fonts/FFXIV_Lodestone_SSF.ttf")
            .Result
            .Content
            .ReadAsByteArrayAsync()
            .Result;
        _gameSymFont = (
            GCHandle.Alloc(gameSym, GCHandleType.Pinned),
            gameSym.Length
        );
    }

    public byte[] GetResource(string path)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)!;
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    public ushort[] GetRange(string path)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)!;
        using var reader = new BinaryReader(stream);
        var tableDirectory = TableDirectory.Read(reader);
        var tables = new List<TableRecord>();
        for (var i = 0; i < tableDirectory.NumTables; i++)
        {
            tables.Add(TableRecord.Read(reader));
        }
        var cmap = tables.First(x => x.Tag == "cmap");
        var cmapTable = CmapTable.Read(reader, cmap.Offset);
        var cmapEncodingRecords = cmapTable.Subtables.First(x => x.PlatformID == 3 && x.EncodingID == 1);
        var cmapEncodingOffset = cmapEncodingRecords.Offset + cmap.Offset;
        var cmapFormat = (CmapFormat4)CmapFormat.Read(reader, cmapEncodingOffset);
        var charCodes = new List<ushort>();
        for (var i = 0; i < cmapFormat.SegCountX2 / 2; i++)
        {
            var first = cmapFormat.StartCodes[i];
            var last = cmapFormat.EndCodes[i];
            first = Math.Clamp(first, (ushort)1, (ushort)0xFFFF);
            if (first > last)
                continue;
            last = Math.Clamp(last, (ushort)1, (ushort)0xFFFF);
            if (first > last)
                continue;
            charCodes.Add(first);
            charCodes.Add(last);
        }
        charCodes.Add(0);
        return charCodes.ToArray();
    }

    public void SetUpRanges()
    {
        _ranges = GCHandle.Alloc(GetRange("DeepDungeonDex.Font.NotoSans-Regular.ttf"), GCHandleType.Pinned);
        _jpRanges = GCHandle.Alloc(GetRange("DeepDungeonDex.Font.NotoSansJP-Regular.otf"), GCHandleType.Pinned);
        _krRanges = GCHandle.Alloc(GetRange("DeepDungeonDex.Font.NotoSansKR-Regular.otf"), GCHandleType.Pinned);
        _tcRanges = GCHandle.Alloc(GetRange("DeepDungeonDex.Font.NotoSansTC-Regular.otf"), GCHandleType.Pinned);
        _scRanges = GCHandle.Alloc(GetRange("DeepDungeonDex.Font.NotoSansSC-Regular.otf"), GCHandleType.Pinned);
    }

    public void LoadFontFile(string path, ref (GCHandle, int) font)
    {
        if (font.Item1.IsAllocated)
            font.Item1.Free();
        var file = GetResource(path);
        font = (GCHandle.Alloc(file, GCHandleType.Pinned), file.Length);
    }

    public void SetUpFonts()
    {
        var config = _handler.GetInstance<Configuration>()!;
        LoadFontFile("DeepDungeonDex.Font.NotoSans-Regular.ttf", ref _regularFont);
        SetUpSpecificFonts(config);
    }

    public void SetUpSpecificFonts(Configuration config)
    {
        FreeFonts(_jpFont, _krFont, _scFont, _tcFont);
        LoadFontFile("DeepDungeonDex.Font.NotoSansJP-Regular.otf", ref _jpFont);
        LoadFontFile("DeepDungeonDex.Font.NotoSansSC-Regular.otf", ref _scFont);
        LoadFontFile("DeepDungeonDex.Font.NotoSansTC-Regular.otf", ref _tcFont);
        LoadFontFile("DeepDungeonDex.Font.NotoSansKR-Regular.otf", ref _krFont);
    }

    public void FreeFonts(params (GCHandle, int)[] fonts)
    {
        foreach (var font in fonts)
        {
            if (font.Item1.IsAllocated)
                font.Item1.Free();
        }
    }

    public void FreeGcHandles(params GCHandle[] handles)
    {
        foreach (var gcHandle in handles)
        {
            if (gcHandle.IsAllocated)
                gcHandle.Free();
        }
    }

    public ImFontPtr AddFont((GCHandle, int) font, float scale, ImFontConfigPtr cfg, GCHandle ranges)
    {
        return ImGui.GetIO().Fonts.AddFontFromMemoryTTF(font.Item1.AddrOfPinnedObject(), font.Item2, scale, cfg, ranges.AddrOfPinnedObject());
    }

    public void BuildFonts(float scale)
    {
        var config = _handler.GetInstance<Configuration>()!;
        RegularFont = AddFont(_regularFont, scale, _fontCfg, _ranges);
        if (config.Locale == 1 || config.LoadAll)
            AddFont(_jpFont, scale, _fontCfgMerge, _jpRanges);
        if (config.Locale == 4 || config.LoadAll)
            AddFont(_scFont, scale, _fontCfgMerge, _scRanges);
        if (config.Locale == 5 || config.LoadAll)
            AddFont(_tcFont, scale, _fontCfgMerge, _tcRanges);
        if (config.Locale == 6 || config.LoadAll)
            AddFont(_krFont, scale, _fontCfgMerge, _krRanges);
        AddFont(_gameSymFont, scale, _fontCfgMerge, _symRange);
    }

    public unsafe void Dispose()
    {
        FreeFonts(_regularFont, _jpFont, _krFont, _scFont, _tcFont, _gameSymFont);
        FreeGcHandles(_symRange, _ranges, _jpRanges, _krRanges, _tcRanges, _scRanges);

        if (_fontCfg.NativePtr != null)
            _fontCfg.Destroy();
        if (_fontCfgMerge.NativePtr != null)
            _fontCfgMerge.Destroy();
    }
}