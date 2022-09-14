using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace DeepDungeonDex.Localization
{
    public class Locale
    {
        private Dictionary<string, string> _manager;
        private string _locale = "en";
        private static Dictionary<string, string> _resourcesPaths = new();
        private static string[] _langs;
        private static string[] _langNames;
        private readonly Dictionary<string, Dictionary<string, string>> _resources = new();
        private readonly List<string> _errorList = new();
        private readonly Dictionary<string, string> _fallback;
        private static readonly HttpClient _httpClient = new();

        public static void LoadResources()
        {
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(_httpClient.GetStringAsync("https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/dev/Localization/locales.json").GetAwaiter().GetResult());
            _langs = dictionary != null ? dictionary.Keys.ToArray() : new[] { "en" };
            _langNames = dictionary != null ? dictionary.Values.ToArray() : new[] { "English" };
            foreach (var t1 in _langs)
            {
                var path = $"https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/dev/Localization/{t1}.yml";
                var res = _httpClient.GetAsync(path).GetAwaiter().GetResult();
                if(res.IsSuccessStatusCode)
                    _resourcesPaths.Add(t1, path);
            }
        }

        private async Task<Dictionary<string, string>> LoadResourceSet(string locale)
        {
            if (_resources.TryGetValue(locale, out var localization)) return localization;
            if (!_resourcesPaths.TryGetValue(locale, out var path)) return null;
            var requestString = await _httpClient.GetStringAsync(path);
            var resource = Plugin.Deserializer.Deserialize<Dictionary<string, string>>(requestString);
            _resources.Add(locale, resource);
            return resource;
        }

        public Locale(string locale)
        {
            _manager = LoadResourceSet(locale).GetAwaiter().GetResult();
            _fallback = LoadResourceSet("en").GetAwaiter().GetResult();
        }

        public static string[] GetLocales() => _langs;
        
        public static string[] GetLocaleNames() => _langNames;

        public void Refresh() 
        {
            _resources.Clear();
            _resourcesPaths.Clear();
            LoadResources();
            _manager = LoadResourceSet(_locale).GetAwaiter().GetResult();
        }

        public async Task ChangeLocale(int locale)
        {
            _locale = _langs[locale];
            try
            {
                _manager = await LoadResourceSet(_locale);
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "Failed to load locale falling back to English.");
                _manager = _fallback;
            }
        }

        public string GetString(string name)
        {
            if (_manager == null && _fallback == null) return name;
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
        public string FontSize => GetString("FontSize");
        public string ShowId => GetString("ShowId");
    }

    public static class DictionaryExtension
    {
        public static string GetString(this Dictionary<string, string> dictionary, string key)
        {
            if(dictionary.TryGetValue(key, out var value))
                return value;
            throw new KeyNotFoundException($"Key {key} could not be found in dictionary.");
        }
    }
}
