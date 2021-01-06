using ImGuiNET;
using System.Numerics;

namespace DeepDungeonDex
{
    public class ConfigUI
    {

        public bool IsVisible { get; set; }
        private Configuration config = new Configuration();

        public void Draw()
        {

            if (!IsVisible)
                return;
            var flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize;
            ImGui.SetNextWindowSizeConstraints(new Vector2(250, 100), new Vector2(1000, 1000));
            ImGui.Begin("config", flags);
            ImGui.SliderFloat("Opacity", ref config.Opacity, 0.0f, 1.0f);
            ImGui.Checkbox("Enable clickthrough", ref config.IsClickthrough);
            if (ImGui.Button("Close"))
            {
                IsVisible = false;
                config.Save();
            }
        }
    }
}
