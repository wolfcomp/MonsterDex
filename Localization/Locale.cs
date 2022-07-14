using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Logging;

namespace DeepDungeonDex.Localization
{
    public class Locale
    {
        private ResourceManager _manager;
        private ResourceManager _fallback;
        private string _locale = "en";
        private List<string> _errorList = new List<string>();

        public Locale()
        {
            _manager = new ResourceManager("DeepDungeonDex.Localization.en", typeof(Locale).Assembly);
            _fallback = new ResourceManager("DeepDungeonDex.Localization.en", typeof(Locale).Assembly);
        }

        public void ChangeLocale(string locale)
        {
            _manager.ReleaseAllResources();
            _locale = locale;
            try
            {
                _manager = new ResourceManager("DeepDungeonDex.Localization." + locale, typeof(Locale).Assembly);
            }
            catch(Exception e)
            {
                PluginLog.Error(e, "Failed to load locale falling back.");
                _manager = new ResourceManager("DeepDungeonDex.Localization.en", typeof(Locale).Assembly);
            }
            _fallback = new ResourceManager("DeepDungeonDex.Localization.en", typeof(Locale).Assembly);
        }

        public string GetString(string name)
        {
            string ret = null;
            try
            {
                ret = _manager.GetString(name) ?? _fallback.GetString(name);
            }
            catch (Exception e)
            {
                if (!_errorList.Contains($"{_locale}.{name}"))
                    PluginLog.Error(e, $"Failed to get translated string of {name} falling back");
                try
                {
                    _errorList.Add($"{_locale}.{name}");
                    ret = _fallback.GetString(name);
                }
                catch (Exception ex)
                {
                    if (!_errorList.Contains($"en.{name}"))
                        PluginLog.Error(ex, $"Failed to get fallback string of {name} report error to developer");
                    _errorList.Add($"en.{name}");
                }
            }

            return ret ?? name;
        }

        public string NoDataFound => GetString("NoDataFound");
        public string HideBasedOnJob => GetString("HideBasedOnJob");
        public string HideRedVulns => GetString("HideRedVulns");
        public string IsClickthrough => GetString("IsClickthrough");
        public string Opacity => GetString("Opacity");
        public string Save => GetString("Save");
        public string Thanks => GetString("Thanks");
        public string Undead => GetString("Undead");
        public string Name => GetString("Name");
        public string AggroType => GetString("AggroType");
        public string Threat => GetString("Threat");
        public string Easy => GetString("Easy");
        public string Caution => GetString("Caution");
        public string Dangerous => GetString("Dangerous");
        public string Vicious => GetString("Vicious");
        public string Undefined => GetString("Undefined");
        public string Vulns => GetString("Vulns");
        public string Notes => GetString("Notes");
    }
}
