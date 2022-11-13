using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Configuration;
using Dalamud.Plugin;
using DeepDungeonDex.Storage;
using ImGuiNET;
using Newtonsoft.Json;

namespace DeepDungeonDex
{
    public class Configuration : ISave
    {
        public int Version { get; set; }
        [JsonProperty("IsClickthrough")] public bool Clickthrough { get; set; }
        [JsonProperty("HideRedVulns")] public bool HideRed { get; set; }
        [JsonProperty("HideBasedOnJob")] public bool HideJob { get; set; }
        [JsonProperty("ShowId")] public bool Debug { get; set; }
        public int Locale { get; set; } = 0;
        public int FontSize { get; set; } = 16;
        public float Opacity { get; set; } = 1f;

        [JsonIgnore] public float FontSizeScaled => FontSize * 1 / ImGui.GetIO().FontGlobalScale;

        [JsonIgnore] public float WindowSizeScaled => Math.Max(FontSize / 16f, 1f);
        
        public void Save(string path)
        {
            StorageHandler.SerializeFile(path, this);
        }
    }
}
