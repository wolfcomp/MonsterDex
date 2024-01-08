namespace DeepDungeonDex.Storage;

public class Locale : ILoad<Locale>
{
    public Dictionary<string, string> TranslationDictionary { get; set; } = new();

    public Locale Load(string path)
    {
        TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(path);
        return this;
    }

    public void Dispose()
    {
        TranslationDictionary.Clear();
    }
    object ILoad.Load(string str) => Load(str);
}

public class LocaleKeys : ILoad<LocaleKeys>
{
    public Dictionary<string, string> LocaleDictionary { get; set; } = new();
        
    public LocaleKeys Load(string path)
    {
        LocaleDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(path);
        return this;
    }

    public void Dispose()
    {
        LocaleDictionary.Clear();
    }

    object ILoad.Load(string str) => Load(str);
}

public static class LocaleExtensions
{
    public static string GetLocale(this Locale locale, string key)
    {
        return locale.TranslationDictionary.TryGetValue(key, out var value) ? value : key;
    }

    public static string GetLocale(this IEnumerable<Locale> locales, string key)
    {
        var ret = locales.FirstOrDefault(l => l.TranslationDictionary.Keys.Any(t => string.Equals(t.Trim(), key.Trim(), StringComparison.InvariantCultureIgnoreCase)));
        return ret != null ? ret.TranslationDictionary.First(t => string.Equals(t.Key.Trim(), key.Trim(), StringComparison.InvariantCultureIgnoreCase)).Value : key;
    }

    public static IEnumerable<KeyValuePair<string, string>> GetLocaleList(this IEnumerable<Locale> locales, string key)
    {
        foreach (var locale in locales)
        {
            foreach (var (s, value) in locale.TranslationDictionary)
            {
                if(s == key)
                    yield return new KeyValuePair<string, string>(s, value);
            }
        }
    }
}