using Dalamud.Configuration;
using Dalamud.Plugin;
using Newtonsoft.Json;

namespace DeepDungeonDex
{
    public class Configuration : IPluginConfiguration
    {
        public int Version { get; set; }
        public bool IsClickThrough { get; set; } = false;
        public float Opacity { get; set; } = 1.0f;
        public bool HideRedVulns { get; set; } = false;
        public bool HideBasedOnJob { get; set; } = false;

        // Add any other properties or methods here.
        [JsonIgnore] private DalamudPluginInterface _pluginInterface;

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
