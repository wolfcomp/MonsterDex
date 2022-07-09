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
            ImGui.NextColumn();
        }

        private void DrawVulns(MobData mobData)
        {
            var classJobId = _clientState.LocalPlayer?.ClassJob.GameData?.RowId ?? 0;
            foreach (var vulnerabilities in new []{ Vulnerabilities.Stun, Vulnerabilities.Sleep, Vulnerabilities.Bind, Vulnerabilities.Heavy, Vulnerabilities.Slow })
            {
                if (!_config.HideBasedOnJob || DataHandler.ShouldRender(classJobId, vulnerabilities))
                {
                    PrintSingleVuln(mobData.Vuln.HasFlag(vulnerabilities), vulnerabilities.ToString());
                }
            }
            if (!(TargetData.NameID >= 7262 && TargetData.NameID <= 7610))
            {
                PrintSingleVuln(mobData.Vuln.HasFlag(Vulnerabilities.Undead), "Undead");
            }
        }

        public void Draw()
        {
            if (!IsVisible)
                return;
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
            ImGui.Columns(2, null, false);
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
            ImGui.NewLine();
            ImGui.Text("Vulns:\n");
            ImGui.Columns(4, null, false);
            DrawVulns(mobData);
            ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.NewLine();
            ImGui.Text("Notes:\n");
            ImGui.TextWrapped(mobData.MobNotes);
            ImGui.End();
        }
    }
}
