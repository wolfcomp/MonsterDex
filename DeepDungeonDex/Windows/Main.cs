using System;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using DeepDungeonDex.Models;
using DeepDungeonDex.Storage;
using ImGuiNET;

namespace DeepDungeonDex.Windows
{
    public class Main : Window, IDisposable
    {
        private readonly Condition _condition;
        private readonly TargetManager _target;
        private readonly StorageHandler _storage;
        private readonly Framework _framework;
        private readonly ClientState _state;
        private Mob _currentMob;
        private uint _targetId;
        private bool _debug;

        public Main(StorageHandler storage, CommandHandler command, TargetManager target, Framework framework, Condition condition, ClientState state) : base("DeepDungeonDex MobView", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar)
        {
            _condition = condition;
            _target = target;
            _storage = storage;
            _framework = framework;
            _state = state;
            framework.Update += GetData;
            command.AddCommand("debug", (args) =>
            {
                if (!uint.TryParse(args.Split(' ')[0], out var id))
                {
                    IsOpen = false;
                    return;
                }

                _targetId = id;

                
                if (_currentMob.Id != _targetId)
                {
                    var data = _storage.GetInstances<MobData>().GetData(_targetId);
                    if (data == null)
                    {
                        IsOpen = false;
                        return;
                    }

                    _currentMob = data;
                }
                IsOpen = true;
            }, show: false);
        }

        public void Dispose()
        {
            _framework.Update -= GetData;
        }

        private void GetData(Framework framework)
        {
            if (!_condition[ConditionFlag.InDeepDungeon] && !_debug)
            {
                IsOpen = false;
                return;
            }
            
            if (_target.Target is not BattleNpc npc)
            {
                if(!_debug)
                    IsOpen = false;
                return;
            }

            _targetId = npc.NameId;
            
            if (_currentMob.Id != _targetId)
            {
                var data = _storage.GetInstances<MobData>().GetData(_targetId);
                if (data == null)
                {
                    IsOpen = false;
                    return;
                }

                _currentMob = data;
            }
            IsOpen = true;
        }

        public override void Draw()
        {
            var legacy = _storage.GetInstance<Configuration>()?.LegacyWindow ?? false;
            if(legacy)
                DrawLegacy();
            DrawCompact();
        }

        public void DrawCompact()
        {

        }

        public void DrawLegacy()
        {
            var _config = _storage.GetInstance<Configuration>();
            if (_config!.Clickthrough && !Flags.HasFlag(ImGuiWindowFlags.NoInputs))
                Flags |= ImGuiWindowFlags.NoInputs;
            else if (!_config.Clickthrough && Flags.HasFlag(ImGuiWindowFlags.NoInputs))
                Flags &= ~ImGuiWindowFlags.NoInputs;
            ImGui.SetNextWindowSizeConstraints(new Vector2(250 * _config.WindowSizeScaled, 0), new Vector2(9001, 9001));
            ImGui.SetNextWindowBgAlpha(_config.Opacity);
            var _locale = _storage.GetInstances<Locale>();

            void printTextWithColor(string text, uint color)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, color);
                ImGui.Text(text);
                ImGui.PopStyleColor();
            }

            void printSingleVuln(bool? flag, string? message)
            {
                if (message is null)
                    return;
                switch (flag)
                {
                    case true:
                        printTextWithColor(message, 0xFF00FF00);
                        break;
                    case false:
                        if (!_config.HideRed)
                        {
                            printTextWithColor(message, 0xFF0000FF);
                        }
                        break;
                    default:
                        printTextWithColor(message, 0x50FFFFFF);
                        break;
                }
                ImGui.NextColumn();
            }
            
            void drawVulns(Mob mob)
            {
                var classJobId = _state.LocalPlayer?.ClassJob.GameData?.RowId ?? 0;
                var _classJob = _storage.GetInstance<JobData>()!.JobDictionary[classJobId];
                foreach (var _weakness in new []{ Weakness.Stun, Weakness.Sleep, Weakness.Bind, Weakness.Heavy, Weakness.Slow })
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
                ImGui.Text(_locale.GetLocale("Name") + ":\n" + _currentMob.Name);
                ImGui.NextColumn();
                ImGui.Text("ID:\n" + _currentMob.Id);
                ImGui.NewLine();
                ImGui.NextColumn();
            }
            else
            {
                ImGui.Text(_locale.GetLocale("Name") + ":\n" + _currentMob.Name);
                ImGui.NewLine();
                ImGui.Columns(2, null, false);
            }
            ImGui.Text(_locale.GetLocale("AggroType") + ":\n");
            ImGui.Text(_locale.GetLocale(_currentMob.Aggro.ToString()));
            ImGui.NextColumn();
            ImGui.Text(_locale.GetLocale("Threat")+":\n");
            switch (_currentMob.Threat)
            {
                case Threat.Easy:
                    printTextWithColor(_locale.GetLocale("Easy"), 0xFF00FF00);
                    break;
                case Threat.Caution:
                    printTextWithColor(_locale.GetLocale("Caution"), 0xFF00FFFF);
                    break;
                case Threat.Dangerous:
                    printTextWithColor(_locale.GetLocale("Dangerous"), 0xFF0000FF);
                    break;
                case Threat.Vicious:
                    printTextWithColor(_locale.GetLocale("Vicious"), 0xFFFF00FF);
                    break;
                default:
                    ImGui.Text(_locale.GetLocale("Undefined"));
                    break;
            }
            ImGui.NextColumn();
            ImGui.NewLine();
            ImGui.Text(_locale.GetLocale("Vulns") + ":\n");
            ImGui.Columns(4, null, false);
            drawVulns(_currentMob);
            ImGui.NextColumn();
            ImGui.Columns(1);
            var note = _currentMob.Description.Replace("\\n", "\n");
            if (!string.IsNullOrWhiteSpace(note))
            {
                ImGui.NewLine();
                ImGui.Text(_locale.GetLocale("Notes") + ":\n");
                ImGui.TextWrapped(note);
            }
            ImGui.PopFont();
        }
    }
}
