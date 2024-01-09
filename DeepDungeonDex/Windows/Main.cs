using System.IO;
using System.Numerics;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace DeepDungeonDex.Windows;

public partial class Main : Window, IDisposable
{
#pragma warning disable CS8618
    private static Mob _currentMob;
#pragma warning restore CS8618
    private ICondition _condition;
    private ITargetManager _target;
    private IFramework _framework;
    private ITextureProvider _textureProvider;
    private IClientState _clientState;
    private IPluginLog _log;
    private StorageHandler _storage;
    private DalamudPluginInterface _pluginInterface;
    private Configuration _config;
    private Locale[] _locale = Array.Empty<Locale>();
    private uint _targetId;
    private bool _debug;
    private IDalamudTextureWrap? _unknown;

    public Main(StorageHandler storage, CommandHandler command, ITargetManager target, IFramework framework, IClientState state, ICondition condition, ITextureProvider textureProvider, IPluginLog log, DalamudPluginInterface pluginInterface) : base("DeepDungeonDex MobView", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)
    {
        _condition = condition;
        _target = target;
        _storage = storage;
        storage.StorageChanged += Storage_StorageChanged;
        _framework = framework;
        _textureProvider = textureProvider;
        _pluginInterface = pluginInterface;
        _clientState = state;
        _log = log;
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
            if (!uint.TryParse(args.Split(' ')[0], out var id))
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

    private void Storage_StorageChanged(StorageEventArgs e)
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
        if (config.ClickThrough && !Flags.HasFlag(ImGuiWindowFlags.NoInputs))
            Flags |= ImGuiWindowFlags.NoInputs;
        else if (!config.ClickThrough && Flags.HasFlag(ImGuiWindowFlags.NoInputs))
            Flags &= ~ImGuiWindowFlags.NoInputs;
    }

    public void Dispose()
    {
        _config.OnChange -= ConfigChanged;
        _framework.Update -= GetData;
        _storage.GetInstance<Configuration>()!.OnChange -= ConfigChanged;
        _unknown?.Dispose();
        _locale = null!;
        _currentMob = null!;
        _textureProvider = null!;
        _pluginInterface = null!;
        _clientState = null!;
        _condition = null!;
        _target = null!;
        _storage = null!;
        _framework = null!;
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
            _log.Information($"No data for {_targetId} setting unknowns.");
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

    private void GetData(IFramework framework)
    {
        if (_debug)
        {
            return;
        }

        if (_condition[ConditionFlag.PvPDisplayActive])
        {
            IsOpen = false;
            return;
        }

        if (_target.Target is not BattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy } npc)
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
        using var _ = ImRaii.PushFont(Font.Font.RegularFont);
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_currentMob.InstanceContentType)
        {
            case InstanceContentType.DeepDungeon:
                DrawDeepDungeonData();
                break;
            case InstanceContentType.Dungeon:
            case InstanceContentType.GuildOrder:
            case InstanceContentType.QuestBattle:
            case InstanceContentType.BeginnerTraining:
            case InstanceContentType.TreasureHuntDungeon:
            case InstanceContentType.SeasonalDungeon:
            case InstanceContentType.MaskedCarnivale:
            case InstanceContentType.VariantDungeon:
            case InstanceContentType.CriterionDungeon:
            default:
                DrawUnknownContent();
                break;
        }
    }

    public void DrawUnknownContent()
    {
        ImGui.TextUnformatted(string.Format(_locale.GetLocale("UnknownContent"), _currentMob.Name, _currentMob.Id));
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
        DrawIcon(15004, size, weakness, Weakness.Stun);
        ImGui.SameLine();
        DrawIcon(15002, size, weakness, Weakness.Heavy);
        ImGui.SameLine();
        DrawIcon(15009, size, weakness, Weakness.Slow);
        ImGui.SameLine();
        DrawIcon(15013, size, weakness, Weakness.Sleep);
        ImGui.SameLine();
        DrawIcon(15003, size, weakness, Weakness.Bind);

        // ReSharper disable once InvertIf
        if (_currentMob.Id is not (>= 7262 and <= 7610) && _clientState.TerritoryType is >= 561 and <= 565 or >= 593 and <= 607 || weakness.HasFlag(Weakness.Undead))
        {
            ImGui.SameLine();
            DrawIcon(15461, size, weakness, Weakness.Undead);
        }
    }

    private static Vector2 _uv0 = new(0, 0);
    private static Vector2 _uv1 = new(1, 1);
    private static Vector4 _color = new(1, 1, 1, 1);
    private static Vector4 _unknownColor = new(0.5f, 0.5f, 0.5f, 0.5f);

    public void DrawIcon(uint iconId, Vector2 size, Weakness weakness, Weakness check)
    {
        var cursor = ImGui.GetCursorPos();
        var color = GetColor(weakness, check);
        ImGui.Image(_textureProvider.GetIcon(iconId)!.ImGuiHandle, size, _uv0, _uv1, color);
        var unknownBit = (Weakness)((int)check << 6);
        if (weakness.HasFlag(unknownBit))
        {
            ImGui.SetCursorPos(cursor);
            ImGui.Image(_unknown!.ImGuiHandle, size);
        }
    }

    public Vector4 GetColor(Weakness weakness, Weakness check)
    {
        return weakness.HasFlag(check) ? _color : _unknownColor;
    }

    private static void PrintTextWithColor(string? text, uint color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }
}