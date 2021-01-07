using Dalamud.Plugin;
using ImGuiNET;
using System.Numerics;

namespace DeepDungeonDex
{
    public class ConfigUI
    {

        public bool IsVisible { get; set; }
        public float opacity;
        public bool isClickthrough;
        private DalamudPluginInterface pluginInterface;
        private Configuration config;

        public ConfigUI(float opacity, bool isClickthrough, DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;
            this.config = (Configuration)this.pluginInterface.GetPluginConfig() ?? new Configuration();
            this.opacity = opacity;
            this.isClickthrough = isClickthrough;
            config.Initialize(pluginInterface);
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
            if (ImGui.Button("Close"))
            {
                IsVisible = false;
                config.Save();
            }
        }
    }
}
