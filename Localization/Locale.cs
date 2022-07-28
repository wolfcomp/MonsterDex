using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using Dalamud.Logging;
using Newtonsoft.Json;

namespace DeepDungeonDex.Localization
{
    public class Locale
    {
        private ResxReader _manager;
        private string _locale = "en";
        private static Dictionary<string, string> _resourcesPaths = new();
        private static string[] _langs;
        private static string[] _langNames;
        private readonly Dictionary<string, ResxReader> _resources = new();
        private readonly List<string> _errorList = new();
        private readonly ResxReader _fallback;

        public static void LoadResources()
        {
            var httpClient = new HttpClient();
            var dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(httpClient.GetStringAsync("https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/dev/Localization/locales.json").GetAwaiter().GetResult());
            _langs = dictionary != null ? dictionary.Keys.ToArray() : new[] { "en" };
            _langNames = dictionary != null ? dictionary.Values.ToArray() : new[] { "English" };
            foreach (var t1 in _langs)
            {
                var path = $"https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/dev/Localization/{t1}.resx";
                var res = httpClient.GetAsync(path).GetAwaiter().GetResult();
                if(res.IsSuccessStatusCode)
                    _resourcesPaths.Add(t1, path);
            }
        }

        private ResxReader LoadResourceSet(string locale)
        {
            if (_resources.TryGetValue(locale, out var reader)) return reader;
            if (!_resourcesPaths.TryGetValue(locale, out var path)) return null;
            var resource = new ResxReader(path);
            _resources.Add(locale, resource);
            return resource;

        }

        public Locale(string locale)
        {
            _manager = LoadResourceSet(locale);
            _fallback = LoadResourceSet("en");
        }

        public static string[] GetLocales() => _langs;
        
        public static string[] GetLocaleNames() => _langNames;

        public void Refresh() 
        {
            _resources.Clear();
            _resourcesPaths.Clear();
            LoadResources();
        }

        public void ChangeLocale(int locale)
        {
            _locale = _langs[locale];
            try
            {
                _manager = LoadResourceSet(_locale);
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "Failed to load locale falling back.");
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
    }
}
