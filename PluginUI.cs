using ImGuiNET;
using System.Numerics;
using Dalamud.Game.ClientState;
using Dalamud.Logging;

namespace DeepDungeonDex
{
    public class PluginUI
    {
        public bool IsVisible { get; set; }
        private readonly Configuration _config;
        private readonly ClientState _clientState;

        public PluginUI(Configuration config, ClientState clientState)
        {
            this._config = config;
            this._clientState = clientState;
        }

        private readonly bool[] _classJobStun =
        {
            true, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, false, true, false, false, false, false, true, true, false, true, false, true, false, true, true, false, true, false
        };

        private readonly bool[] _classJobSleep =
        {
            true, false, false, false, false, false, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, true, false, false, false, false, true, false, true, true, false, false, false, true
        };

        private readonly bool[] _classJobBind =
        {
            true, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, true, true, false, false, false, false, true, false, true, false, false
        };

        private readonly bool[] _classJobHeavy =
        {
            true, false, false, false, false, true, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, false, true, false, false, false, false, false, false, true, true, false, false, false, false, true, false, true, false, false
        };

        private readonly bool[] _classJobSlow =
        {
            true, true, true, true, true, true, false, false, false, false, false, false, false, false, false, false, false, false, false, true, true, true, true, true, false, false, false, false, false, true, true, true, true, false, true, false, true, true, true, true, false
        };

        private static void PrintTextWithColor(string text, uint color)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, color);
            ImGui.Text(text);
            ImGui.PopStyleColor();
        }
        private void PrintSingleVuln(bool? flag, string message)
        {
            switch (flag)
            {
                case true:
                    PrintTextWithColor(message, 0xFF00FF00);
                    break;
                case false:
                    if (!_config.HideRedVulns)
                    {
                        PrintTextWithColor(message, 0xFF0000FF);
                    }
                    break;
                default:
                    PrintTextWithColor(message, 0x50FFFFFF);
                    break;
            }
        }
        public void Draw()
        {
            if (!IsVisible)
                return;
            var classJobId = _clientState.LocalPlayer?.ClassJob.GameData?.RowId ?? 0;
            var data = DataHandler.Mobs(TargetData.NameID);
            if (!data.HasValue)
            {
                PluginLog.Log("No data found for " + TargetData.Name + " (" + TargetData.NameID + ")");
                return;
            }
            var mobData = data.Value;
            var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar;
            if (_config.IsClickThrough)
            {
                flags |= ImGuiWindowFlags.NoInputs;
            }
            ImGui.SetNextWindowSizeConstraints(new Vector2(250, 0), new Vector2(9001, 9001));
            ImGui.SetNextWindowBgAlpha(_config.Opacity);
            ImGui.Begin("cool strati window", flags);
            ImGui.Text("Name:\n" + TargetData.Name);
            ImGui.NewLine();
            ImGui.Columns(3, null, false);
            ImGui.Text("Aggro Type:\n");
            ImGui.Text(mobData.Aggro.ToString());
            ImGui.NextColumn();
            ImGui.Text("Threat:\n");
            switch (mobData.Threat)
            {
                case ThreatLevel.Easy:
                    PrintTextWithColor("Easy", 0xFF00FF00);
                    break;
                case ThreatLevel.Caution:
                    PrintTextWithColor("Caution", 0xFF00FFFF);
                    break;
                case ThreatLevel.Dangerous:
                    PrintTextWithColor("Dangerous", 0xFF0000FF);
                    break;
                case ThreatLevel.Vicious:
                    PrintTextWithColor("Vicious", 0xFFFF00FF);
                    break;
                default:
                    ImGui.Text("Undefined");
                    break;
            }
            ImGui.NextColumn();
            if (!_config.HideBasedOnJob || _classJobStun[classJobId])
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Stun), "Stun");
            }
            if (!_config.HideBasedOnJob || _classJobSleep[classJobId])
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Sleep), "Sleep");
            }
            if (!_config.HideBasedOnJob || _classJobBind[classJobId])
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Bind), "Bind");
            }
            if (!_config.HideBasedOnJob || _classJobHeavy[classJobId])
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Heavy), "Heavy");
            }
            if (!_config.HideBasedOnJob || _classJobSlow[classJobId])
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Slow), "Slow");
            }
            if (!(TargetData.NameID >= 7262 && TargetData.NameID <= 7610))
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Undead), "Undead");
            }
            ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.NewLine();
            ImGui.TextWrapped(mobData.MobNotes);
            ImGui.End();
        }
    }
}
