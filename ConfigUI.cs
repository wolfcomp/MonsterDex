using ImGuiNET;
using System.Diagnostics;
using System.Numerics;

namespace DeepDungeonDex
{
    public class ConfigUI
    {

        public bool IsVisible { get; set; }
        private float opacity;
        private bool isClickthrough;
        private Configuration config;

        public ConfigUI(float opacity, bool isClickthrough, Configuration config)
        {
            this.config = config;
            this.opacity = opacity;
            this.isClickthrough = isClickthrough;
        }

        public void Draw()
        {
            if (!IsVisible)
                return;
            var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;
            ImGui.SetNextWindowSizeConstraints(new Vector2(250, 100), new Vector2(1000, 1000));
            ImGui.Begin("config", flags);
            if (ImGui.SliderFloat("Opacity", ref opacity, 0.0f, 1.0f))
            {
                config.Opacity = opacity;
            }
            if (ImGui.Checkbox("Enable clickthrough", ref isClickthrough))
            {
                config.IsClickthrough = isClickthrough;
            }
            if (ImGui.Button("Save"))
            {
                IsVisible = false;
                config.Save();
            }
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF5E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xFF5E5BAA);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xFF5E5BDD);
            var c = ImGui.GetCursorPos();
            ImGui.SetCursorPosX(ImGui.GetWindowContentRegionWidth() - ImGui.CalcTextSize("Support on Ko-fi").X);
            if (ImGui.SmallButton("Support on Ko-fi"))
            {
                Process.Start("https://ko-fi.com/strati");
            }
            ImGui.SetCursorPos(c);
            ImGui.PopStyleColor(3);
        }
    }
}
