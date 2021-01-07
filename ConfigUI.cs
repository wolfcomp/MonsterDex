using Dalamud.Plugin;
using ImGuiNET;
using System.Numerics;

namespace DeepDungeonDex
{
    public class ConfigUI
    {

        public bool IsVisible { get; set; }
        private Configuration config;
        private DalamudPluginInterface pluginInterface;

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.config.Initialize(this.pluginInterface);
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
            if (ImGui.Checkbox("Enable clickthrough", ref isclickthrough))
            {
                config.IsClickthrough = isclickthrough;
            }
            if (ImGui.Button("Close"))
            {
                IsVisible = false;
                config.Save();
            }
        }
    }
}
