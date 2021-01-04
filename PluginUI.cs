using ImGuiNET;

namespace DeepDungeonDex
{
    public class PluginUI
    {
        public bool IsVisible { get; set; }

        public void Draw()
        {
            if (!IsVisible)
                return;
            ImGui.Begin("cool strati window");
            ImGui.Text("Name: "+ TargetData.name);
            ImGui.NewLine();
            ImGui.Text("NameID: "+ TargetData.nameID);
            ImGui.NewLine();
            ImGui.TextWrapped("");
            if (ImGui.Button("Close")) { this.IsVisible = false; }
            ImGui.End();
        }
    }
}
