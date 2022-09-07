using System;
using Dalamud.Configuration;
using Dalamud.Plugin;
using ImGuiNET;
using Newtonsoft.Json;

namespace DeepDungeonDex
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public bool IsClickthrough { get; set; } = false;
        public float Opacity { get; set; } = 1.0f;
        public bool HideRedVulns { get; set; } = false;
        public bool HideBasedOnJob { get; set; } = false;
        public int Locale { get; set; } = 0;
        public float FontSize { get; set; } = 16f;

        // Add any other properties or methods here.
        [JsonIgnore] private DalamudPluginInterface _pluginInterface;

        [JsonIgnore]
        public string LocaleString => Localization.Locale.GetLocales()[Locale];

        [JsonIgnore]
        public float FontSizeScaled => FontSize * 1 / ImGui.GetIO().FontGlobalScale;
        [JsonIgnore]
        public float WindowSizeScaled { get; set; }


        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
            WindowSizeScaled = Math.Max(FontSize / 16, 1f);
        }

        public void Save()
        {
            WindowSizeScaled = Math.Max(FontSize / 16, 1f);
            _pluginInterface.SavePluginConfig(this);
            _pluginInterface.UiBuilder.RebuildFonts();
        }
    }
}
