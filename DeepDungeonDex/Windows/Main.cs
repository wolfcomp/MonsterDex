using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using DeepDungeonDex.Hooks;
using DeepDungeonDex.Weather;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Numerics;
using BattleChara = FFXIVClientStructs.FFXIV.Client.Game.Character.BattleChara;
using STask = System.Threading.Tasks.Task;

namespace DeepDungeonDex.Windows;

public partial class Main : Window, IDisposable
{
#pragma warning disable CS8618
    private static Mob _currentMob;
    private static unsafe BattleChara* _currentNpc;
#pragma warning restore CS8618
    private ICondition _condition;
    private ITargetManager _target;
    private IFramework _framework;
    private ITextureProvider _textureProvider;
    private IClientState _clientState;
    private IPluginLog _log;
    private StorageHandler _storage;
    private Configuration _config;
    private WeatherManager _weatherManager;
    private Locale[] _locale = Array.Empty<Locale>();
    private uint _targetId;
    private bool _debug;
    private IDalamudTextureWrap? _unknown;
    private AddonAgent _addon;
    private const string _githubIssuePath = "https://github.com/wolfcomp/MonsterDex/issues/new?template=fix_node.yaml";
    private unsafe string IssueUrl => $"{_githubIssuePath}&mob_id={_currentNpc->NameId}%20-%20{_currentNpc->NameString}&content_type={_addon.ContentType:G}&version={Assembly.GetExecutingAssembly().GetName().Version?.ToString(3)}";

    public Main(StorageHandler storage, CommandHandler command, ITargetManager target, IFramework framework, IClientState state, ICondition condition, ITextureProvider textureProvider, IPluginLog log, AddonAgent addon, WeatherManager weatehrManager) : base("MonsterDex MobView", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)
    {
        _condition = condition;
        _target = target;
        _storage = storage;
        storage.StorageChanged += Storage_StorageChanged;
        _framework = framework;
        _textureProvider = textureProvider;
        _clientState = state;
        _log = log;
        _addon = addon;
        _weatherManager = weatehrManager;
        var instance = this;
        _config = _storage.GetInstance<Configuration>()!;
        SizeConstraints = new WindowSizeConstraints
        {
            MaximumSize = new Vector2(400 * _config.WindowSizeScaled, 600),
            MinimumSize = new Vector2(250 * _config.WindowSizeScaled, 100)
        };
        BgAlpha = _config.Opacity;
        _framework.RunOnTick(LoadIcons);
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
        _clientState = null!;
        _condition = null!;
        _target = null!;
        _storage = null!;
        _framework = null!;
    }

    private void SetTarget(uint id)
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
                Weakness = Weakness.BindUnknown | Weakness.HeavyUnknown | Weakness.SleepUnknown | Weakness.SlowUnknown | Weakness.StunUnknown,
                InstanceContentType = _addon.ContentType,
                IsGenerated = true
            };
            if (_clientState.TerritoryType is >= 561 and <= 565 or >= 593 and <= 607)
                data.Weakness |= Weakness.UndeadUnknown;
        }

        _log.Verbose("Set target to:\n" + data);

        _currentMob = data;
    }

    private unsafe void GetData(IFramework framework)
    {
        if (_debug)
        {
            return;
        }

        if (_condition[ConditionFlag.PvPDisplayActive] || !EnabledContents())
        {
            IsOpen = false;
            return;
        }

        if (_target.Target is not IBattleNpc { BattleNpcKind: BattleNpcSubKind.Enemy } npc)
        {
            if (!_debug)
            {
                IsOpen = false;
                _currentMob = null!;
            }
            return;
        }

        SetTarget(npc.NameId);

        _currentNpc = (BattleChara*)npc.Address;
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

    private bool EnabledContents() => _addon.ContentType.HasAnyFlag(_config.EnabledContentTypes);

    public override void Draw()
    {
        using var _ = Font.Font.RegularFont.Push();
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (_currentMob.InstanceContentType)
        {
            case ContentType.DeepDungeon:
                DrawDeepDungeonData();
                break;
            case ContentType.Eureka:
#if DEBUG
                DrawEurekaData();
                break;
#endif
            default:
                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (_addon.ContentType)
                {
                    case ContentType.OceanFishing:
                    case ContentType.BondingCeremony:
                    case ContentType.CrystallineConflict:
                    case ContentType.Trial:
                    case ContentType.Raid:
                    case ContentType.UnrealTrial:
                    case ContentType.Mahjong:
                    case ContentType.GoldSaucer:
                    case ContentType.RivalWing:
                    case ContentType.TripleTriad:
                    case ContentType.PublicTripleTriad:
                    case ContentType.LeapOfFaith:
                    case ContentType.Frontlines:
                        break;
                    case ContentType.Eureka:
                    case ContentType.IslandSanctuary:
                    case ContentType.Diadem:
                    case ContentType.Bozja:
                    case ContentType.None:
                        DrawNoDisplay();
                        break;
                    default:
                        DrawUnknownContent();
                        break;
                }
                break;
        }
    }

    private void DrawNoDisplay() => ImGui.TextUnformatted(string.Format(_locale.GetLocale("NoDisplay"), _addon.ContentType.ToString("G")));

    private unsafe void DrawUnknownContent()
    {
        ImGui.TextUnformatted(string.Format(_locale.GetLocale("UnknownContent"), _currentNpc->NameString, _currentNpc->NameId));
        // ReSharper disable once InvertIf
        if (ImGui.Button(_locale.GetLocale("CreateDataIssue")))
        {
            Util.OpenLink(IssueUrl);
        }

        if (_config.Debug && _currentNpc != null)
        {
            ImGui.TextUnformatted("Debug information");
            var forayInfo = _currentNpc->GetForayInfo();
            if (forayInfo != null)
                ImGui.TextUnformatted($"NamePlateIconId: {(uint)forayInfo->Element + 60650}");
            ImGui.TextUnformatted($"SubKind: {_currentNpc->Character.GameObject.SubKind}");
            ImGui.TextUnformatted($"Current Time: {((Time)Framework.GetServerTime()).GetEorzeanTime()}");
            ImGui.TextUnformatted($"Weather: {_addon.Weather}");
            ImGui.TextUnformatted("WeatherIds:");
            foreach (var se in _addon.WeatherIds.Where(t => t != 0).Select(t => $"{t}: {_weatherManager.GetWeatherName(t)}").ToList())
            {
                ImGui.Text(se);
            }
        }
    }

    private async STask LoadIcons()
    {
        await using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("DeepDungeonDex.UnknownDebuf.png");
        if (stream is null)
        {
            _log.Error("Failed to load unknown debuf icon.");
            return;
        }
        _unknown = await _textureProvider.CreateFromImageAsync(stream, debugName: "DeepDungeonDex.UnknownDebuf");
    }

    private static Vector2 _uv0 = new(0, 0);
    private static Vector2 _uv1 = new(1, 1);
    private static Vector4 _color = new(1, 1, 1, 1);
    private static Vector4 _unknownColor = new(0.75f, 0.75f, 0.75f, 0.75f);
    private static Vector4 _notActiveColor = new(0.5f, 0.5f, 0.5f, 0.5f);

    private void DrawWeaknessIcon(uint iconId, Vector2 size, Weakness weakness, Weakness check)
    {
        var cursor = ImGui.GetCursorPos();
        var color = GetColor(weakness, check);
        ImGui.Image(_textureProvider.GetFromGameIcon(iconId + 200000).GetWrapOrEmpty().Handle, size, _uv0, _uv1, color);
        if (weakness.HasUnknownFlag(check))
        {
            ImGui.SetCursorPos(cursor);
            ImGui.Image(_unknown!.Handle, size, _uv0, _uv1, color);
        }
    }

    private void DrawIcon(uint iconId, Vector2 size, Vector4 color)
    {
        ImGui.Image(_textureProvider.GetFromGameIcon(iconId).GetWrapOrEmpty().Handle, size, _uv0, _uv1, color);
    }

    private Vector4 GetColor(Weakness weakness, Weakness check)
    {
        return weakness.HasUnknownFlag(check) ? _unknownColor : weakness.HasFlag(check) ? _color : _notActiveColor;
    }

    private static void PrintTextWithColor(string? text, uint color)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, color);
        ImGui.TextUnformatted(text);
        ImGui.PopStyleColor();
    }
}