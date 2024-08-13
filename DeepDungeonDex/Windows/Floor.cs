using DeepDungeonDex.Hooks;
using System.Numerics;

namespace DeepDungeonDex.Windows;

public class Floor : Window, IDisposable
{
    private IClientState _clientState;
    private ICondition _condition;
    private IFramework _framework;
    private IPluginLog _log;
    private StorageHandler _storage;
    private AddonAgent _addon;
    private Locale? _locale;
    private byte _debug;
    private string _dataPath = "";

    public Floor(StorageHandler storage, CommandHandler command, IFramework framework, ICondition condition, IClientState state, IPluginLog log, AddonAgent addon) : base("MonsterDex FloorGuide", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)
    {
        _storage = storage;
        _framework = framework;
        _condition = condition;
        _clientState = state;
        _addon = addon;
        _log = log;
        command.AddCommand("debug_floor", args =>
        {
            var argArr = args.Split(' ');
            if (!byte.TryParse(argArr[0], out var id) || !ushort.TryParse(argArr[1], out var ter))
            {
                if (argArr[0] == "print")
                {
                    var floor = _debug == 0 ? _addon.Floor : _debug;
                    _log.Debug($"Currently displaying: {_dataPath}{floor}");
                    return;
                }
                _debug = 0;
                _dataPath = "";
                IsOpen = false;
                return;
            }

            _debug = id;
            TerritoryChanged(ter);
            _locale = _storage.GetInstance<Locale>(_dataPath + "/Floors.yml");
            IsOpen = true;

        }, show: false);
        command.AddCommand(new[] { "enable_floor", "e_floor", "enable_f", "ef" }, _addon.Restart, "Resets the floor getter function to try again");
        var config = _storage.GetInstance<Configuration>()!;
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(800 * config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(450 * config.WindowSizeScaled, 100)
        };
        BgAlpha = config.Opacity;
        framework.Update += GetData;
        config.OnChange += ConfigChanged;
        state.TerritoryChanged += TerritoryChanged;
        storage.StorageChanged += StorageChanged;
    }

    private void StorageChanged(StorageEventArgs obj)
    {
        if (obj.StorageType == typeof(Territories) && obj.StorageType == typeof(Locale))
            TerritoryChanged(_clientState.TerritoryType);
    }

    private void TerritoryChanged(ushort e)
    {
        var territories = _storage.GetInstance<Territories>();
        if (territories == null)
            return;
        _dataPath = territories.GetTerritoryName(e, _log);
        _locale = _storage.GetInstance<Locale>(_dataPath + "/Floors.yml");
    }

    private void GetData(IFramework framework)
    {
        if (_debug != 0)
            return;

        if (_dataPath != "")
        {
            var config = _storage.GetInstance<Configuration>()!;
            if (!_addon.DirectorDisabled && !config.HideFloor && _addon.Floor % 10 != 0 && _storage.GetInstance(_dataPath + "/Floors.yml") != null)
                IsOpen = true;
            else
                IsOpen = false;
        }
        else
            IsOpen = false;
    }

    private void ConfigChanged(Configuration config)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(800 * config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(450 * config.WindowSizeScaled, 100)
        };
        BgAlpha = config.Opacity;
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (config.ClickThrough && !Flags.HasFlag(ImGuiWindowFlags.NoInputs))
            Flags |= ImGuiWindowFlags.NoInputs;
        else if (!config.ClickThrough && Flags.HasFlag(ImGuiWindowFlags.NoInputs))
            Flags &= ~ImGuiWindowFlags.NoInputs;
    }

    public override void Draw()
    {
        var remap = (FloorData)_storage.GetInstance(_dataPath + "/Floors.yml")!;
        var floor = _debug == 0 ? _addon.Floor : _debug;
        floor = remap.FloorDictionary.TryGetValue(floor, out var f) ? f : floor;
        using var _ = Font.Font.RegularFont.Push();
        try
        {
            ImGui.Text("Floor Help");
            ImGui.TextUnformatted(_locale?.GetLocale($"{_dataPath[(_dataPath.LastIndexOf("/", StringComparison.Ordinal) + 1)..]}{floor}"));
        }
        catch (Exception e)
        {
            _log.Error(e, "Error trying to draw floor guide.");
            // ignored
        }
    }

    public void Dispose()
    {
        _framework.Update -= GetData;
        _clientState.TerritoryChanged -= TerritoryChanged;
        var _config = _storage.GetInstance<Configuration>()!;
        _config.OnChange -= ConfigChanged;
        _clientState = null!;
        _condition = null!;
        _framework = null!;
        _storage = null!;
        _addon = null!;
        _locale = null!;
        _log = null!;
    }
}