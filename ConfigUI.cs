using Dalamud.Plugin;
using ImGuiNET;
using System.Numerics;

namespace DeepDungeonDex
{
    public class ConfigUI
    {

        public bool IsVisible { get; set; }
        private float opacity;
        private bool isClickthrough;
        private Plugin plugin = new Plugin();

        public ConfigUI(float opacity, bool isClickthrough)
        {
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
                plugin.Opacity = opacity;
            }
            if (ImGui.Checkbox("Enable clickthrough", ref isClickthrough))
            {
                plugin.IsClickthrough = isClickthrough;
            }
            if (ImGui.Button("Close"))
            {
                IsVisible = false;
            }
        }
    }
}
