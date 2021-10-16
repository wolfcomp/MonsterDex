
using ImGuiNET;
using System.Numerics;

namespace DeepDungeonDex
{
    public class PluginUI
    {
        public bool IsVisible { get; set; }
        private Configuration config;

        public PluginUI(Configuration config)
        {
            this.config = config;
        }

        private void PrintSingleVuln(bool? isVulnerable, string message)
        {
            ImGui.Text(message);
            switch (isVulnerable)
            {
                case true:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FF00);
                    ImGui.Text("Yes");
                    ImGui.PopStyleColor();
                    break;
                case false:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                    ImGui.Text("No");
                    ImGui.PopStyleColor();
                    break;
                default:
                    ImGui.Text("Untested");
                    break;
            }
        }
        public void Draw()
        {
            if (!IsVisible)
                return;
            var mobData = DataHandler.Mobs(TargetData.NameID);
            if (mobData == null) return;
            var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoTitleBar;
            if (config.IsClickthrough)
            {
                flags |= ImGuiWindowFlags.NoInputs;
            }
            ImGui.SetNextWindowSizeConstraints(new Vector2(250, 0), new Vector2(9001, 9001));
            ImGui.SetNextWindowBgAlpha(config.Opacity);
            ImGui.Begin("cool strati window", flags);
            ImGui.Text("Name:\n"+TargetData.Name);
            ImGui.NewLine();
            ImGui.Columns(3, null, false);
            ImGui.Text("Aggro Type:\n");
            ImGui.Text(mobData.Aggro.ToString());
            ImGui.NextColumn();
            ImGui.Text("Threat:\n");
            switch (mobData.Threat)
            {
                case DataHandler.MobData.ThreatLevel.Easy:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FF00);
                    ImGui.Text("Easy");
                    ImGui.PopStyleColor();
                    break;
                case DataHandler.MobData.ThreatLevel.Caution:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FFFF);
                    ImGui.Text("Caution");
                    ImGui.PopStyleColor();
                    break;
                case DataHandler.MobData.ThreatLevel.Dangerous:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                    ImGui.Text("Dangerous");
                    ImGui.PopStyleColor();
                    break;
                case DataHandler.MobData.ThreatLevel.Vicious:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFF00FF);
                    ImGui.Text("Vicious");
                    ImGui.PopStyleColor();
                    break;
                default:
                    ImGui.Text("Undefined");
                    break;
            }
            ImGui.NextColumn();
            //PrintSingleVuln(mobData.Vuln.CanStun, "Can Stun");
            ImGui.Text("Can stun:\n");
            switch (mobData.Vuln.CanStun)
            {
                case true:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FF00);
                    ImGui.Text("Yes");
                    ImGui.PopStyleColor();
                    break;
                case false:
                    ImGui.PushStyleColor(ImGuiCol.Text, 0xFF0000FF);
                    ImGui.Text("No");
                    ImGui.PopStyleColor();
                    break;
                default:
                    ImGui.Text("Untested");
                    break;
            }
            ImGui.NextColumn();
            ImGui.Columns(1);
            ImGui.NewLine();
            ImGui.TextWrapped(mobData.MobNotes);
            ImGui.End();
        }
    }
}
