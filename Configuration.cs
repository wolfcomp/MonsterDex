using Dalamud.Configuration;
using Dalamud.Plugin;
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


        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            _pluginInterface = pluginInterface;
        }

        public void Save()
        {
            _pluginInterface.SavePluginConfig(this);
            _pluginInterface.UiBuilder.RebuildFonts();
        }
    }
}
