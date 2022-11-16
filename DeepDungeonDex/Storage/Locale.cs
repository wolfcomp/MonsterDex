using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepDungeonDex.Models;
using Newtonsoft.Json;

namespace DeepDungeonDex.Storage
{
    public class Locale : ILoadable<Locale>
    {
        public Dictionary<string, string> LocaleDictionary { get; set; } = new();

        public void Save(string path)
        {
            StorageHandler.SerializeJsonFile(path, this);
        }

        [JsonIgnore]
        public Action<DateTime> Updated { get; set; }

        public Locale Load(string path)
        {
            LocaleDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(StorageHandler.ReadFile(path));
            throw new NotImplementedException();
        }

        object ILoadable.Load(string path) => Load(path);
    }

    public static class LocaleExtensions
    {
        public static string? GetLocale(this Locale locale, string key)
        {
            return locale.LocaleDictionary.TryGetValue(key, out var value) ? value : key;
        }

        public static string? GetLocale(this IEnumerable<Locale> locales, string key)
        {
            return locales.FirstOrDefault(l => l.LocaleDictionary.ContainsKey(key))?.LocaleDictionary[key];
        }
    }
}
