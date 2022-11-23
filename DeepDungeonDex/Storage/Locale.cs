using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DeepDungeonDex.Models;
using Newtonsoft.Json;

namespace DeepDungeonDex.Storage
{
    public class Locale : ILoadable
    {
        public Dictionary<string, string> TranslationDictionary { get; set; } = new();
        
        [JsonIgnore]
        private Storage s;

        public NamedType Save(string path)
        {
            StorageHandler.SerializeJsonFile(path, TranslationDictionary);
            return new NamedType { Name = s.Name, Type = GetType() };
        }

        public Storage Load(string path)
        {
            TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(StorageHandler.ReadFile(path));
            return new Storage(this);
        }

        public Storage Load(string path, string name)
        {
            var s = Load(path);
            s.Name = name;
            return s;
        }
    }

    public class LocaleKeys : ILoadable
    {
        public Dictionary<string, string> LocaleDictionary { get; set; } = new();

        public NamedType? Save(string path)
        {
            StorageHandler.SerializeJsonFile(path, LocaleDictionary);
            return null;
        }
        
        public Storage Load(string path)
        {
            LocaleDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(StorageHandler.ReadFile(path));
            return new Storage(this);
        }
        
        public Storage Load(string path, string name)
        {
            throw new NotImplementedException();
        }
    }

    public static class LocaleExtensions
    {
        public static string? GetLocale(this Locale locale, string key)
        {
            return locale.TranslationDictionary.TryGetValue(key, out var value) ? value : key;
        }

        public static string? GetLocale(this IEnumerable<Locale> locales, string key)
        {
            return locales.FirstOrDefault(l => l.TranslationDictionary.ContainsKey(key))?.TranslationDictionary[key] ?? key;
        }
    }
}
