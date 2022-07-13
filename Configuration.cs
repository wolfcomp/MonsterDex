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

        // Add any other properties or methods here.
        [JsonIgnore] private DalamudPluginInterface _pluginInterface;

        [JsonIgnore]
        public string LocaleString => Locale switch
        {
            1 => "jp",
            2 => "fr",
            3 => "de",
            4 => "zh.simpl",
            5 => "zh.full",
            _ => "en"
        };

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this._pluginInterface = pluginInterface;
        }

        public void Save()
        {
            this._pluginInterface.SavePluginConfig(this);
        }
    }
}
