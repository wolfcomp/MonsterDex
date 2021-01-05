using ImGuiNET;
using System.Numerics;

namespace DeepDungeonDex
{
    public class ConfigUI
    {
        public bool IsVisible { get; set; }
        public bool IsClickthrough = false;
        public float Opacity = 1.0f;

        public void Draw()
        {
            if (!IsVisible)
                return;
            
        }
    }
}
