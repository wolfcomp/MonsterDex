namespace DeepDungeonDex.Storage;

public class Locale : ILoadableString
{
    public Dictionary<string, string> TranslationDictionary { get; set; } = new();
        
    [JsonIgnore]
    private Storage _s = null!;

    public NamedType Save(string path)
    {
        StorageHandler.SerializeJsonFile(path, TranslationDictionary);
        return new NamedType { Name = _s.Name, Type = GetType() };
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

    public Storage Load(string str, bool fromFile)
    {
        if(fromFile)
            return Load(str);
        TranslationDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<string, string>>(str);
        return new Storage(this);
    }

    public void Dispose()
    {
        _s = null!;
        TranslationDictionary.Clear();
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

    public void Dispose()
    {
        LocaleDictionary.Clear();
    }
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