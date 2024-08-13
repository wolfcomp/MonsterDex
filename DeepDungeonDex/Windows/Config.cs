using System.Numerics;

namespace DeepDungeonDex.Windows;

public class Config : Window, IDisposable
{
    private StorageHandler _handler;
    private Requests _requests;
    private IDalamudPluginInterface _pluginInterface;
    private IServiceProvider _provider;
    private Font.Font _font;
    private float _opacity;
    private bool _clickthrough;
    private bool _hideRed;
    private bool _hideJob;
    private bool _debug;
    private int _loc;
    private bool _loadAll;
    private bool _hideFloor;
    private ContentType _contentTypes = ContentType.DeepDungeon | ContentType.Eureka | ContentType.IslandSanctuary | ContentType.Diadem | ContentType.Bozja | ContentType.None;
    private ContentType[] _allContentTypes = Array.Empty<ContentType>();
    private ContentType[] AllContentTypes => _allContentTypes.Any() ? _allContentTypes : _allContentTypes = Enum.GetValues(typeof(ContentType)).Cast<ContentType>().ToArray();


    public Config(IDalamudPluginInterface pluginInterface, StorageHandler handler, CommandHandler command, Requests requests, IServiceProvider provider, Font.Font font) : base("MonsterDex Config", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
    {
        _handler = handler;
        _requests = requests;
        _provider = provider;
        _pluginInterface = pluginInterface;
        _font = font;
        var _config = _handler.GetInstance<Configuration>()!;
        _config.OnChange += OnChange;
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(400 * _config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(250 * _config.WindowSizeScaled, 100)
        };
        BgAlpha = _opacity = _config.Opacity;
        _clickthrough = _config.ClickThrough;
        _hideRed = _config.HideRed;
        _hideJob = _config.HideJob;
        _debug = _config.Debug;
        _loc = _config.Locale;
        _loadAll = _config.LoadAll;
        _hideFloor = _config.HideFloor;
        _pluginInterface.UiBuilder.OpenConfigUi += Open;
        command.AddCommand(new[] { "config", "cfg" }, Open, "Opens the config window.");
    }

    public void Open()
    {
        IsOpen = true;
    }

    public void Dispose()
    {
        _pluginInterface.UiBuilder.OpenConfigUi -= Open;
        _handler.GetInstance<Configuration>()!.OnChange -= OnChange;
        _handler = null!;
        _requests = null!;
        _pluginInterface = null!;
        _provider = null!;
    }

    private void OnChange(Configuration config)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(400 * config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(250 * config.WindowSizeScaled, 100)
        };
        BgAlpha = config.Opacity;
    }

    public override void Draw()
    {
        var _config = _handler.GetInstance<Configuration>()!;
        var _localeKeys = _handler.GetInstance<LocaleKeys>()!;
        var lang = _localeKeys.LocaleDictionary.Keys.ToArray()[_config.PrevLocale];
        var _locale = (Locale)_handler.GetInstance($"{lang}/main.yml")!;
        using var _ = Font.Font.RegularFont.Push();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("Opacity").X);
        if (ImGui.SliderFloat(_locale.GetLocale("Opacity"), ref _opacity, 0.0f, 1.0f))
        {
            _config.Opacity = _opacity;
        }
        ImGui.Columns(4, null, false);
        ImGui.Text(_locale.GetLocale("FontSize"));
        foreach (var f in new int[] { 12, 14, 16, 18, 24, 32 })
        {
            ImGui.NextColumn();
            if (ImGui.RadioButton($"{f}px", _config.FontSize == f))
            {
                _config.FontSize = f;
            }
        }
        ImGui.Columns(1);
        if (ImGui.Checkbox(_locale.GetLocale("IsClickthrough"), ref _clickthrough))
        {
            _config.ClickThrough = _clickthrough;
        }
        if (ImGui.Checkbox(_locale.GetLocale("HideFloorGuide"), ref _hideFloor))
        {
            _config.HideFloor = _hideFloor;
        }
        if (ImGui.Checkbox(_locale.GetLocale("ShowId"), ref _debug))
        {
            _config.Debug = _debug;
        }

        var locales = _localeKeys.LocaleDictionary.Values.ToArray();
        ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize("Locale").X);
        if (ImGui.Combo("Locale", ref _loc, locales, locales.Length))
        {
            _config.Locale = _loc;
            _requests.ChangeLanguage();
        }
        ImGui.Columns(2, null, false);
        ImGui.TextUnformatted(_locale.GetLocale("ContentTypes"));
        foreach (var contentType in AllContentTypes)
        {
            if (!_contentTypes.HasFlag(contentType))
                continue;

            var enabled = _config.EnabledContentTypes.HasFlag(contentType);
            ImGui.NextColumn();
            if (!ImGui.Checkbox(_locale.GetLocale($"ContentType{contentType:G}"), ref enabled))
                continue;

            if (enabled)
                _config.EnabledContentTypes |= contentType;
            else
                _config.EnabledContentTypes &= ~contentType;
        }
        ImGui.Columns(1);
        ImGui.NewLine();
        if (ImGui.Button(_locale.GetLocale("Save")))
        {
            IsOpen = false;
            _config.Save(_handler.GetFilePath(_config.GetType()));
            _font.RegisterNewBuild(_config.FontSize);
        }
        ImGui.SameLine();
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            ImGui.PushTextWrapPos(400f);
            ImGui.TextWrapped(_locale.GetLocale("Thanks"));
            ImGui.PopTextWrapPos();
            ImGui.EndTooltip();
        };
        ImGui.SameLine();
        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF5E5BFF);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xFF5E5BAA);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xFF5E5BDD);
        ImGui.PopStyleColor(3);
    }
}