using System.IO;
using Dalamud.Interface.ManagedFontAtlas;

namespace DeepDungeonDex.Font;

public class Font : IDisposable
{
    private readonly IPluginLog _log;
    private readonly IFontAtlas _atlas;
    internal static IFontHandle RegularFont = null!;
    internal float FontSize = 16;

    public Font(IPluginLog log, IFontAtlas atlas)
    {
        _log = log;
        _atlas = atlas;
        RegularFont = _atlas.NewDelegateFontHandle(step =>
        {
            step.OnPreBuild(act =>
            {
                var conf = new SafeFontConfig
                {
                    SizePx = FontSize
                };
                conf.MergeFont = act.AddFontFromStream(GetResourceStream("DeepDungeonDex.Font.NotoSans-Regular.ttf"), conf, false, "DeepDungeonDex-NotoSans-Regular");
                conf.MergeFont = act.AddFontFromStream(GetResourceStream("DeepDungeonDex.Font.NotoSansJP-Regular.otf"), conf, false, "DeepDungeonDex-NotoSansJP-Regular");
                conf.MergeFont = act.AddFontFromStream(GetResourceStream("DeepDungeonDex.Font.NotoSansKR-Regular.otf"), conf, false, "DeepDungeonDex-NotoSansKR-Regular");
                conf.MergeFont = act.AddFontFromStream(GetResourceStream("DeepDungeonDex.Font.NotoSansTC-Regular.otf"), conf, false, "DeepDungeonDex-NotoSansTC-Regular");
                conf.MergeFont = act.AddFontFromStream(GetResourceStream("DeepDungeonDex.Font.NotoSansSC-Regular.otf"), conf, false, "DeepDungeonDex-NotoSansSC-Regular");
                conf.MergeFont = act.AddGameSymbol(conf);
                conf.MergeFont = act.SetFontScaleMode(conf.MergeFont, FontScaleMode.UndoGlobalScale);
            });
        });
    }

    public void RegisterNewBuild(float size)
    {
        FontSize = size;
        _atlas.BuildFontsAsync();
    }

    public Stream GetResourceStream(string path)
    {
        return Assembly.GetExecutingAssembly().GetManifestResourceStream(path)!;
    }

    public void Dispose()
    {
        RegularFont.Dispose();
    }
}