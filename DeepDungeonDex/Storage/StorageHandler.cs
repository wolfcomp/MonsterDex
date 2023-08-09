using System.IO;
using System.Text;
using Dalamud.Game.Gui;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage;

public class StorageHandler : IDisposable
{
    private readonly string _path;
    private readonly ChatGui _chat;
    private bool _debugError;
    public static readonly IDeserializer Deserializer = new DeserializerBuilder().WithTypeConverter(new YamlStringEnumConverter()).Build();
    private static readonly ISerializer _serializer = new SerializerBuilder().WithTypeConverter(new YamlStringEnumConverter()).Build();

    internal readonly Dictionary<string, object> JsonStorage = new();
    internal readonly Dictionary<string, object> YmlStorage = new();

    public event EventHandler<StorageEventArgs> StorageChanged;

    public StorageHandler(DalamudPluginInterface pluginInterface, ChatGui chat)
    {
        _path = pluginInterface.GetPluginConfigDirectory();
        _chat = chat;
        Load();
    }

    public void AddJsonStorage(string path, object storage)
    {
        JsonStorage[path] = storage;
        var args = storage is Storage { Value: not null } obj
            ? new StorageEventArgs(obj.GetType())
            : new StorageEventArgs(storage.GetType());
        StorageChanged?.Invoke(this, args);
    }

    public void AddYmlStorage(string path, object storage)
    {
        YmlStorage[path] = storage;
        var args = storage is Storage { Value: not null } obj
            ? new StorageEventArgs(obj.GetType())
            : new StorageEventArgs(storage.GetType());
        StorageChanged?.Invoke(this, args);
    }

    private void Load()
    {
        try
        {
#if DEBUG
            if (!_debugError)
            {
                _debugError = true;
                throw new Exception("yeet error out");
            }
#endif
            PluginLog.Verbose("Loading Storage");
            var configPath = Path.Combine(_path, "config.json");
            PluginLog.Verbose("Loading config from {0}", configPath);
            Configuration config;
            try
            {
                PluginLog.Verbose("Deserializing config");
                config = DeserializeFile<Configuration>(configPath)!;
            }
            catch
            {
                PluginLog.Verbose("Failed to deserialize config, creating new config");
                config = new Configuration();
            }

            config.PrevLocale = config.Locale;
            JsonStorage.Add("config.json", config);
            var storagePath = new FileInfo(Path.Combine(_path, "storage.json"));
            if (storagePath.Exists)
            {
                PluginLog.Verbose("Loading storage from {0}", storagePath);
                var storage = DeserializeFile<Dictionary<string, Tuple<string, string?>>>(storagePath.FullName)!;
                JsonStorage.Add(storagePath.Name, storage);
                foreach (var (key, value) in storage)
                {
                    try
                    {
                        var (typeString, name) = value;
                        var type = Type.GetType(typeString);
                        if (!type!.IsAssignableFrom(typeof(ISaveable))) continue;
                        PluginLog.Verbose("Loading {0}, Type: {1}, Name: {2}", key, typeString, name ?? "");
                        if (key.Contains(".json"))
                        {
                            if (type.IsAssignableFrom(typeof(ILoadable)))
                            {
                                var loadable = (ILoadable)Activator.CreateInstance(type)!;
                                var obj = name != null
                                    ? loadable.Load(Path.Join(_path, key), name)
                                    : loadable.Load(Path.Join(_path, key));
                                JsonStorage.Add(key, obj);
                            }
                            else if (DeserializeFile(key, type) is ISaveable content) JsonStorage.Add(key, new Storage(content));
                        }
                        else
                        {
                            if (type.IsAssignableFrom(typeof(ILoadable)))
                            {
                                var loadable = (ILoadable)Activator.CreateInstance(type)!;
                                var obj = name != null
                                    ? loadable.Load(Path.Join(_path, key), name)
                                    : loadable.Load(Path.Join(_path, key));
                                YmlStorage.Add(key, obj);
                            }
                            else if (Deserializer.Deserialize(key, type) is ISaveable content) YmlStorage.Add(key, new Storage(content));
                        }
                    }
                    catch (Exception e)
                    {
                        PluginLog.Error(e, e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            PluginLog.Error(e, e.Message);
            var sb = new StringBuilder();
            sb.AppendLine("Could not load DeepDungeonDex storage. Clearing storage and retrying.");
            sb.Append("If this error persists, please report it on the DeepDungeonDex channel in the plugin help forum.");
            var errorMsg = sb.ToString();
            PluginLog.Error(errorMsg);
            _chat.PrintError(errorMsg);
            Directory.Delete(_path, true);
            Directory.CreateDirectory(_path);
            Load();
        }
        Save();
    }

    public static T? DeserializeFile<T>(string path, bool ignoreJsonProperty = true) where T : class
    {
        return DeserializeFile(path, typeof(T), ignoreJsonProperty) as T;
    }

    public static object? DeserializeFile(string path, Type type, bool ignoreJsonProperty = true)
    {
        PluginLog.Verbose("Deserializing {0}", path);
        var result = JsonConvert.DeserializeObject(ReadFile(path), type, new JsonSerializerSettings()
        {
            ContractResolver = ignoreJsonProperty ? new NameContractResolver() : null
        });
        PluginLog.Verbose("Deserialized {0}, Type: {1}", path, result?.GetType().ToString() ?? "");
        return result;
    }

    public static string ReadFile(string path)
    {
        PluginLog.Verbose("Reading file {0}", path);
        var reader = new StreamReader(path);
        var result = reader.ReadToEnd();
        PluginLog.Verbose("Read file {0}", path);
        PluginLog.Verbose("Content: \n{0}", result);
        reader.Dispose();
        return result;
    }

    public static void SerializeJsonFile(string path, object obj)
    {
        var writer = new StreamWriter(path);
        writer.Write(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
        {
            ContractResolver = new NameContractResolver()
        }));
        writer.Dispose();
    }

    public static void SerializeYamlFile(string path, object obj)
    {
        var writer = new StreamWriter(path);
        writer.Write(_serializer.Serialize(obj));
        writer.Dispose();
    }

    public void Save()
    {
        var storageDict = new Dictionary<string, Tuple<Type, string?>>();
            
        bool processObj(object obj, string path)
        {
            var fileInfo = new FileInfo(Path.Join(_path, path));
            PluginLog.Verbose("Saving: {0}", fileInfo);
            Directory.CreateDirectory(fileInfo.DirectoryName!);

            if (obj is Storage storage)
            {
                PluginLog.Verbose("Saving inner obj");
                var k = storage.Value.Save(fileInfo.FullName)?.GetTuple();
                if (k != null)
                {
                    PluginLog.Verbose($"Wrote inner obj of type {k.Item1.Name}");
                    storageDict.Add(path, k);
                }
            }
            else
            {
                if (!obj.GetType().IsAssignableFrom(typeof(ISaveable)))
                {
                    return true;
                }

                PluginLog.Verbose("Saving obj");
                var k = (obj as ISaveable)!.Save(fileInfo.FullName)?.GetTuple();
                if (k != null)
                {
                    PluginLog.Verbose($"Wrote obj of type {k.Item1.Name}");
                    storageDict.Add(path, k);
                }
            }

            return false;
        }

        PluginLog.Verbose("Saving Json Storage");
        foreach (var (path, obj) in JsonStorage.ToDictionary(t => t.Key, t => t.Value))
        {
            if (!processObj(obj, path))
                continue;

            SerializeJsonFile(Path.Join(_path, path), obj);
            PluginLog.Verbose($"Wrote {obj.GetType()}");
        }

        PluginLog.Verbose("Saving Yaml Storage");
        foreach (var (path, obj) in YmlStorage.ToDictionary(t => t.Key, t => t.Value))
        {
            if (!processObj(obj, path))
                continue;

            SerializeYamlFile(Path.Join(_path, path), obj);
            PluginLog.Verbose($"Wrote {obj.GetType()}");
        }

        PluginLog.Verbose("Filling missing storage data");
        JsonStorage.AsEnumerable()
            .Concat(YmlStorage)
            .ToList()
            .ForEach(x =>
            {
                var (path, obj) = x;
                var type = obj is Storage storage ? storage.Value.GetType() : obj.GetType();
                if (!storageDict.ContainsKey(path))
                    storageDict.Add(path, new Tuple<Type, string?>(type, null));
            });
        var storagePath = Path.Combine(_path, "storage.json");
        PluginLog.Verbose($"Writing {storagePath}");
        SerializeJsonFile(storagePath, storageDict.Where(t => t.Key is not ("storage.json" or "config.json")).ToDictionary(t => t.Key, t =>
        {
            var (type, name) = t.Value;
            return new Tuple<string, string?>(type.FullName!, name);
        }));
        PluginLog.Verbose("Saved");
    }

    public T? GetInstance<T>() where T : class, ISaveable
    {
        var list = JsonStorage.Values.ToList();
        list.AddRange(YmlStorage.Values);
        return (list.FirstOrDefault(x => x is T) ?? (list.FirstOrDefault(x => x is Storage { Value: T }) as Storage)?.Value) as T ?? null;
    }

    public T? GetInstance<T>(string path) where T : class, ISaveable
    {
        var list = JsonStorage.ToList();
        list.AddRange(YmlStorage);
        var set = list.Where(t => t.Key.Contains(path)).Select(t => t.Value).ToList();
        return (set.FirstOrDefault(x => x is T) ?? (set.FirstOrDefault(x => x is Storage { Value: T }) as Storage)?.Value) as T ?? null;
    }

    public T[] GetInstances<T>() where T : class, ISaveable
    {
        var list = JsonStorage.Values.ToList();
        list.AddRange(YmlStorage.Values);
        return list.Where(t => t is T or Storage { Value: T }).Select(t => t is T ? t : (t as Storage)?.Value).Cast<T>().ToArray();
    }

    public T[] GetInstances<T>(string name) where T : class, ISaveable
    {
        var list = JsonStorage.Values.ToList();
        list.AddRange(YmlStorage.Values);
        return list.Where(t => t is Storage { Value: T } storage && storage.Name.StartsWith(name)).Select(t => t is T ? t : (t as Storage)?.Value).Cast<T>().ToArray();
    }

    public object? GetInstance(string path)
    {
        return JsonStorage.TryGetValue(path, out var obj) ? obj : YmlStorage.TryGetValue(path, out obj) ? obj : null;
    }

    public string GetFilePath(Type type)
    {
        var list = JsonStorage.ToList();
        list.AddRange(YmlStorage);
        var filePath = list.FirstOrDefault(t => t.Value.GetType() == type || t.Value is Storage { Value: { } value } && value.GetType() == type).Key;
        return Path.Combine(_path, filePath);
    }

    public void Dispose()
    {
        Save();
    }
}

public class Storage
{
    public Storage(ISaveable value, string name = "")
    {
        Value = value;
        Name = name;
    }

    public ISaveable Value { get; set; }
    public string Name { get; set; }
}

internal class NameContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        //Generate names with properties
        var list = base.CreateProperties(type, memberSerialization);

        //Override JsonProperty names with the default .NET property names
        foreach (var prop in list)
        {
            prop.PropertyName = prop.UnderlyingName;
        }

        return list;
    }
}

internal class YamlStringEnumConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type.IsEnum;

    public object ReadYaml(IParser parser, Type type)
    {
        var items = new List<string>();
        if (type.GetCustomAttributes<FlagsAttribute>().Any())
        {
            parser.TryConsume<SequenceStart>(out _);
            while (parser.TryConsume<Scalar>(out var scalar))
            {
                items.Add(scalar.Value);
            }
            parser.TryConsume<SequenceEnd>(out _);
        }
        else if (parser.TryConsume<Scalar>(out var scalar))
            items.Add(scalar.Value);
        return Enum.Parse(type, string.Join(", ", items));
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        if (value == null) return;
        if (type.GetCustomAttributes<FlagsAttribute>().Any())
        {
            var str = value.ToString()!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            emitter.Emit(new SequenceStart(default, default, false, SequenceStyle.Any));
            foreach (var s in str)
            {
                emitter.Emit(new Scalar(s));
            }
            emitter.Emit(new SequenceEnd());
        }
        else
        {
            emitter.Emit(new Scalar(value.ToString()!));
        }
    }
}

public class StorageEventArgs : EventArgs
{
    public Type StorageType { get; set; }

    public StorageEventArgs(Type storageType)
    {
        StorageType = storageType;
    }
}