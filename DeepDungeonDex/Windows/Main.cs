using System;
using System.Numerics;
using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using DeepDungeonDex.Models;
using DeepDungeonDex.Storage;
using ImGuiNET;
using ImGuiScene;

namespace DeepDungeonDex.Windows
{
    public class Main : Window, IDisposable
    {
        private static Main _instance;
        private static Mob _currentMob;
        private readonly Condition _condition;
        private readonly TargetManager _target;
        private readonly StorageHandler _storage;
        private readonly Framework _framework;
        private readonly ClientState _state;
        private readonly DataManager _gameData;
        private uint _targetId;
        private bool _debug;
        private bool _disable;
        private TextureWrap? _heavy;
        private TextureWrap? _bind;
        private TextureWrap? _stun;
        private TextureWrap? _slow;
        private TextureWrap? _sleep;
        private TextureWrap? _undead;

        public Main(StorageHandler storage, CommandHandler command, TargetManager target, Framework framework, Condition condition, ClientState state, DataManager gameData) : base("DeepDungeonDex MobView", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)
        {
            _condition = condition;
            _target = target;
            _storage = storage;
            _framework = framework;
            _state = state;
            _gameData = gameData;
            _instance = this;
            var _config = _storage.GetInstance<Configuration>()!;
            SizeConstraints = new WindowSizeConstraints
            {
                MaximumSize = new Vector2(400 * _config.WindowSizeScaled, 600),
                MinimumSize = new Vector2(250 * _config.WindowSizeScaled, 100)
            };
            LoadIcons();
            framework.Update += GetData;
            command.AddCommand("debug", (args) =>
            {
                if (!uint.TryParse(args.Split(' ')[0], out var id) || _disable)
                {
                    _debug = false;
                    IsOpen = false;
                    return;
                }

                _debug = true;
                _instance.SetTarget(id);
                _instance.IsOpen = true;
            }, show: false);
            _config.OnChange += ConfigChanged;
        }

        public void ConfigChanged(Configuration config)
        {
            SizeConstraints = new WindowSizeConstraints
            {
                MaximumSize = new Vector2(400 * config.WindowSizeScaled, 600),
                MinimumSize = new Vector2(250 * config.WindowSizeScaled, 100)
            };
            BgAlpha = config.Opacity;
            if (config.Clickthrough && !Flags.HasFlag(ImGuiWindowFlags.NoInputs))
                Flags |= ImGuiWindowFlags.NoInputs;
            else if (!config.Clickthrough && Flags.HasFlag(ImGuiWindowFlags.NoInputs))
                Flags &= ~ImGuiWindowFlags.NoInputs;
        }

        public void Dispose()
        {
            _framework.Update -= GetData;
            var _config = _storage.GetInstance<Configuration>()!;
            _config.OnChange -= ConfigChanged;
            _heavy.Dispose();
            _bind.Dispose();
            _stun.Dispose();
            _slow.Dispose();
            _sleep.Dispose();
            _undead.Dispose();
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
                IsOpen = false;
                return;
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
            if (_currentMob.Name != npc.Name.ToString())
                _currentMob.Name = npc.Name.ToString();
            IsOpen = true;
        }

        public override void Draw()
        {
            var legacy = _storage.GetInstance<Configuration>()?.LegacyWindow ?? false;
            if (legacy)
                DrawLegacy();
            else
                DrawCompact();
        }

        public void LoadIcons()
        {
            _heavy = _gameData.GetImGuiTextureHqIcon(15002);
            _bind = _gameData.GetImGuiTextureHqIcon(15003);
            _stun = _gameData.GetImGuiTextureHqIcon(15004);
            _slow = _gameData.GetImGuiTextureHqIcon(15009);
            _sleep = _gameData.GetImGuiTextureHqIcon(15013);
            _undead = _gameData.GetImGuiTextureHqIcon(15461);
            if (_heavy == null || _bind == null || _stun == null || _slow == null || _sleep == null || _undead == null)
            {
                _disable = true;
                PluginLog.Error("Could not load icons, disabling Main window.");
            }
        }

        public void DrawWeakness(Weakness weakness)
        {
            var _config = _storage.GetInstance<Configuration>()!;
            var size = new Vector2(24 * _config.FontSize / 16f, 32 * _config.FontSize / 16f);
            var uv0 = new Vector2(0, 0);
            var uv1 = new Vector2(1, 1);
            ImGui.Image(_stun.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Stun) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            ImGui.SameLine();
            ImGui.Image(_heavy.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Heavy) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            ImGui.SameLine();
            ImGui.Image(_slow.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Slow) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            ImGui.SameLine();
            ImGui.Image(_sleep.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Sleep) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            ImGui.SameLine();
            ImGui.Image(_bind.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Bind) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            
            if (_currentMob.Id is not (>= 7262 and <= 7610))
            {
                ImGui.SameLine();
                ImGui.Image(_undead.ImGuiHandle, size, uv0, uv1, weakness.HasFlag(Weakness.Undead) ? new Vector4(1, 1, 1, 1) : new Vector4(0.5f, 0.5f, 0.5f, 0.5f));
            }
        }

        public void DrawCompact()
        {
            var _config = _storage.GetInstance<Configuration>();
            var _locale = _storage.GetInstances<Locale>();
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
            if (!string.IsNullOrWhiteSpace(_currentMob.Description))
            {
                ImGui.NewLine();
                ImGui.TextUnformatted(_locale.GetLocale("Notes") + ":\n");
                ImGui.TextWrapped(_currentMob.Description.Replace("\\n", "\n"));
            }
            ImGui.PopFont();
        }
        
        private void PrintTextWithColor(string? text, uint color)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.TextUnformatted(text);
            ImGui.PopStyleColor();
        }

        #region Legacy Window Code

        public void DrawLegacy()
        {
            var _config = _storage.GetInstance<Configuration>()!;
            if (_config.Clickthrough && !Flags.HasFlag(ImGuiWindowFlags.NoInputs))
                Flags |= ImGuiWindowFlags.NoInputs;
            else if (!_config.Clickthrough && Flags.HasFlag(ImGuiWindowFlags.NoInputs))
                Flags &= ~ImGuiWindowFlags.NoInputs;
            ImGui.PushFont(Font.RegularFont);
            ImGui.SetNextWindowSizeConstraints(new Vector2(250 * _config.WindowSizeScaled, 0), new Vector2(9001, 9001));
            ImGui.SetNextWindowBgAlpha(_config.Opacity);
            var _locale = _storage.GetInstances<Locale>();


            void printSingleVuln(bool? flag, string? message)
            {
                if (message is null)
                    return;
                switch (flag)
                {
                    case true:
                        PrintTextWithColor(message, 0xFF00FF00);
                        break;
                    case false:
                        if (!_config.HideRed)
                        {
                            PrintTextWithColor(message, 0xFF0000FF);
                        }
                        break;
                    default:
                        PrintTextWithColor(message, 0x50FFFFFF);
                        break;
                }
                ImGui.NextColumn();
            }

            void drawVulns(Mob mob)
            {
                var classJobId = _state.LocalPlayer?.ClassJob.GameData?.RowId ?? 0;
                var _classJob = _storage.GetInstance<JobData>()!.JobDictionary[classJobId];
                foreach (var _weakness in new[] { Weakness.Stun, Weakness.Sleep, Weakness.Bind, Weakness.Heavy, Weakness.Slow })
                {
                    if (!_config.HideJob || _classJob.HasFlag(_weakness))
                    {
                        printSingleVuln(mob.Weakness.HasFlag(_weakness), _locale.GetLocale(_weakness.ToString()));
                    }
                }
                if (mob.Id is not (>= 7262 and <= 7610))
                {
                    printSingleVuln(mob.Weakness.HasFlag(Weakness.Undead), _locale.GetLocale("Undead"));
                }
            }

            if (_config.Debug)
            {
                ImGui.Columns(2, null, false);
                ImGui.TextUnformatted(_locale.GetLocale("Name") + ":\n" + _currentMob.Name);
                ImGui.NextColumn();
                ImGui.TextUnformatted("ID:\n" + _currentMob.Id);
                ImGui.NewLine();
                ImGui.NextColumn();
            }
            else
            {
                ImGui.TextUnformatted(_locale.GetLocale("Name") + ":\n" + _currentMob.Name);
                ImGui.NewLine();
                ImGui.Columns(2, null, false);
            }
            ImGui.TextUnformatted(_locale.GetLocale("AggroType") + ":\n");
            ImGui.TextUnformatted(_locale.GetLocale(_currentMob.Aggro.ToString()));
            ImGui.NextColumn();
            ImGui.TextUnformatted(_locale.GetLocale("Threat") + ":\n");
            switch (_currentMob.Threat)
            {
                case Threat.Easy:
                    PrintTextWithColor(_locale.GetLocale("Easy"), 0xFF00FF00);
                    break;
                case Threat.Caution:
                    PrintTextWithColor(_locale.GetLocale("Caution"), 0xFF00FFFF);
                    break;
                case Threat.Dangerous:
                    PrintTextWithColor(_locale.GetLocale("Dangerous"), 0xFF0000FF);
                    break;
                case Threat.Vicious:
                    PrintTextWithColor(_locale.GetLocale("Vicious"), 0xFFFF00FF);
                    break;
                default:
                    ImGui.TextUnformatted(_locale.GetLocale("Undefined"));
                    break;
            }
            ImGui.NextColumn();
            ImGui.NewLine();
            ImGui.TextUnformatted(_locale.GetLocale("Vulns") + ":\n");
            ImGui.Columns(4, null, false);
            drawVulns(_currentMob);
            ImGui.NextColumn();
            ImGui.Columns(1);
            if (!string.IsNullOrWhiteSpace(_currentMob.Description))
            {
                ImGui.NewLine();
                ImGui.TextUnformatted(_locale.GetLocale("Notes") + ":\n");
                ImGui.TextWrapped(_currentMob.Description.Replace("\\n", "\n"));
            }
            ImGui.PopFont();
        }
        #endregion
    }
}
