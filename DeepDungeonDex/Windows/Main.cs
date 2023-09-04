using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using ImGuiScene;

namespace DeepDungeonDex.Windows;

public class Main : Window, IDisposable
{
#pragma warning disable CS8618
    private static Mob _currentMob;
#pragma warning restore CS8618
    private readonly Condition _condition;
    private readonly TargetManager _target;
    private readonly StorageHandler _storage;
    private readonly Framework _framework;
    private readonly ITextureProvider _textureProvider;
    private readonly DalamudPluginInterface _pluginInterface;
    private readonly Configuration _config;
    private readonly IClientState _clientState;
    private Locale[] _locale;
    private uint _targetId;
    private bool _debug;
    private bool _disable;
    private TextureWrap? _unknown;

    public Main(StorageHandler storage, CommandHandler command, TargetManager target, Framework framework, IClientState state, Condition condition, ITextureProvider textureProvider, DalamudPluginInterface pluginInterface) : base("DeepDungeonDex MobView", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)
    {
        _condition = condition;
        _target = target;
        _storage = storage;
        storage.StorageChanged += Storage_StorageChanged;
        _framework = framework;
        _textureProvider = textureProvider;
        _pluginInterface = pluginInterface;
        _clientState = state;
        var instance = this;
        _config = _storage.GetInstance<Configuration>()!;
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(400 * _config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(250 * _config.WindowSizeScaled, 100)
        };
        BgAlpha = _config.Opacity;
        LoadIcons();
        framework.Update += GetData;
        command.AddCommand("debug_mob", (args) =>
        {
            if (!uint.TryParse(args.Split(' ')[0], out var id) || _disable)
            {
                _debug = false;
                IsOpen = false;
                return;
            }

            _debug = true;
            instance.SetTarget(id);
            instance.IsOpen = true;
        }, show: false);
        _config.OnChange += ConfigChanged;
    }

    private void Storage_StorageChanged(object? sender, StorageEventArgs e)
    {
        if (e.StorageType == typeof(Locale))
        {
            _locale = _storage.GetInstances<Locale>();
        }
    }

    public void ConfigChanged(Configuration config)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(400 * config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(250 * config.WindowSizeScaled, 100)
        };
        BgAlpha = config.Opacity;
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (config.Clickthrough && !Flags.HasFlag(ImGuiWindowFlags.NoInputs))
            Flags |= ImGuiWindowFlags.NoInputs;
        else if (!config.Clickthrough && Flags.HasFlag(ImGuiWindowFlags.NoInputs))
            Flags &= ~ImGuiWindowFlags.NoInputs;
    }

    public void Dispose()
    {
        _framework.Update -= GetData;
        _storage.GetInstance<Configuration>()!.OnChange -= ConfigChanged;
        _unknown?.Dispose();
    }

    public void SetTarget(uint id)
    {
        _targetId = id;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_currentMob is not null && _currentMob.Id == id)
        {
            return;
        }

        var data = _storage.GetInstances<MobData>().GetData(_targetId);
        if (data == null)
        {
            PluginLog.Information($"No data for {_targetId} setting unknowns.");
            data = new Mob
            {
                Aggro = Aggro.Undefined,
                Id = id,
                Threat = Threat.Undefined,
                Weakness = Weakness.BindUnknown | Weakness.HeavyUnknown | Weakness.SleepUnknown | Weakness.SlowUnknown | Weakness.StunUnknown | Weakness.UndeadUnknown
            };
        }

        _currentMob = data;
    }

    private void GetData(Framework framework)
    {
        if (_debug)
        {
            return;
        }
            
        if (!_condition[ConditionFlag.InDeepDungeon] || _disable)
        {
            IsOpen = false;
            return;
        }

        if (_target.Target is not BattleNpc npc)
        {
            if (!_debug)
                IsOpen = false;
            return;
        }

        SetTarget(npc.NameId);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_currentMob is null)
        {
            IsOpen = false;
            return;
        }
        if (_currentMob.Name != npc.Name.ToString())
            _currentMob.Name = npc.Name.ToString();
        IsOpen = true;
    }

    public override void Draw()
    {
        ImGui.PushFont(Font.RegularFont);
        var line = $"{_currentMob.Name}{(_config.Debug ? $" ({_currentMob.Id})" : "")}";
        ImGui.TextUnformatted(line);
        ImGui.TextUnformatted($"{_locale.GetLocale(_currentMob.Aggro.ToString())}\t");
        ImGui.SameLine();
        PrintTextWithColor(_locale.GetLocale(_currentMob.Threat.ToString()), _currentMob.Threat.GetColor());
        ImGui.NewLine();
        ImGui.TextUnformatted(_locale.GetLocale("Vulns"));
        ImGui.SameLine();
        DrawWeakness(_currentMob.Weakness);
        if (!string.IsNullOrWhiteSpace(_currentMob.JoinedProcessedDescription))
        {
            ImGui.NewLine();
            ImGui.TextUnformatted(_locale.GetLocale("Notes") + ":\n");
            var size = ImGui.GetWindowSize();
            var desc = _currentMob.ProcessedDescription;
            if(desc.Length == 0)
                _currentMob.ProcessDescription(size.X);
            foreach (var s in desc)
            {
                ImGui.TextUnformatted(s);
            }
        }
        ImGui.PopFont();
    }

    public void LoadIcons()
    {
        _unknown = _pluginInterface.UiBuilder.LoadImage(GetResource("DeepDungeonDex.UnknownDebuf.png"));
    }

    public byte[] GetResource(string path)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path)!;
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    public void DrawWeakness(Weakness weakness)
    {
        var size = new Vector2(24 * _config.FontSize / 16f, 32 * _config.FontSize / 16f);
        var uv0 = new Vector2(0, 0);
        var uv1 = new Vector2(1, 1);
        var cursor = ImGui.GetCursorPos();
        ImGui.Image(_textureProvider.GetIcon(15004)!.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Stun) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        if(weakness.HasFlag(Weakness.StunUnknown))
            DrawUnknown(cursor, size);
        ImGui.SameLine();
        cursor = ImGui.GetCursorPos();
        ImGui.Image(_textureProvider.GetIcon(15002)!.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Heavy) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        if(weakness.HasFlag(Weakness.HeavyUnknown))
            DrawUnknown(cursor, size);
        ImGui.SameLine();
        cursor = ImGui.GetCursorPos();
        ImGui.Image(_textureProvider.GetIcon(15009)!.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Slow) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        if(weakness.HasFlag(Weakness.SlowUnknown))
            DrawUnknown(cursor, size);
        ImGui.SameLine();
        cursor = ImGui.GetCursorPos();
        ImGui.Image(_textureProvider.GetIcon(15013)!.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Sleep) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        if(weakness.HasFlag(Weakness.SleepUnknown))
            DrawUnknown(cursor, size);
        ImGui.SameLine();
        cursor = ImGui.GetCursorPos();
        ImGui.Image(_textureProvider.GetIcon(15003)!.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Bind) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
        if(weakness.HasFlag(Weakness.BindUnknown))
            DrawUnknown(cursor, size);
            
        // ReSharper disable once InvertIf
        if (_currentMob.Id is not (>= 7262 and <= 7610) && _clientState.TerritoryType is >= 561 and <= 565 or >= 593 and <= 607 || weakness.HasFlag(Weakness.Undead))
        {
            ImGui.SameLine();
            cursor = ImGui.GetCursorPos();
            ImGui.Image(_textureProvider.GetIcon(15461)!.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Undead) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            if(weakness.HasFlag(Weakness.UndeadUnknown))
                DrawUnknown(cursor, size);
        }
    }

    public void DrawUnknown(Vector2 pos, Vector2 size)
    {
        ImGui.SetCursorPos(pos);
        ImGui.Image(_unknown!.ImGuiHandle, size);
    }
        
    private static void PrintTextWithColor(string? text, uint color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }
}