using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage;

public class StorageHandler : IDisposable
{
    private readonly string _path;
    private IChatGui _chat;
    private static IPluginLog _log = null!;
    public static IDeserializer Deserializer = new DeserializerBuilder().WithTypeConverter(new YamlStringEnumConverter()).Build();
    private static ISerializer _serializer = new SerializerBuilder().WithTypeConverter(new YamlStringEnumConverter()).Build();

    internal readonly Dictionary<string, object> JsonStorage = new();
    internal readonly Dictionary<string, object> YmlStorage = new();
    internal readonly Dictionary<string, IBinaryLoadable> BinaryStorage = new();

    public event Action<StorageEventArgs>? StorageChanged;

    public StorageHandler(DalamudPluginInterface pluginInterface, IChatGui chat, IPluginLog log)
    {
        _path = pluginInterface.GetPluginConfigDirectory();
        _chat = chat;
        _log = log;
        Load();
    }

    public void AddJsonStorage(string path, object storage)
    {
        JsonStorage[path] = storage;
        var args = storage is Storage { Value: not null } obj
            ? new StorageEventArgs(obj.GetType())
            : new StorageEventArgs(storage.GetType());
        StorageChanged?.Invoke(args);
    }

    public void AddYmlStorage(string path, object storage)
    {
        YmlStorage[path] = storage;
        var args = storage is Storage { Value: not null } obj
            ? new StorageEventArgs(obj.GetType())
            : new StorageEventArgs(storage.GetType());
        StorageChanged?.Invoke(args);
    }

    public void AddBinaryStorage(string path, IBinaryLoadable storage)
    {
        BinaryStorage[path] = storage;
        var args = new StorageEventArgs(storage.GetType());
        StorageChanged?.Invoke(args);
    }

    private void Load()
    {
        try
        {
            _log.Verbose("Loading Storage");
            Configuration config = LoadConfig();
            config.PrevLocale = config.Locale;
            BinaryStorage.Add("config.dat", config);
            var storagePath = new FileInfo(Path.Combine(_path, "storage.json"));
            if (storagePath.Exists)
            {
                _log.Verbose("Loading storage from {0}", storagePath);
                var storage = DeserializeFile<Dictionary<string, Tuple<string, string?>>>(storagePath.FullName)!;
                JsonStorage.Add(storagePath.Name, storage);
                foreach (var (key, value) in storage)
                {
                    try
                    {
                        var (typeString, name) = value;
                        var type = Type.GetType(typeString);
                        if (!type!.IsAssignableFrom(typeof(ISaveable))) continue;
                        _log.Verbose("Loading {0}, Type: {1}, Name: {2}", key, typeString, name ?? "");
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
                        _log.Error(e, e.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            _log.Error(e, e.Message);
            var sb = new StringBuilder();
            sb.AppendLine("Could not load DeepDungeonDex storage. Clearing storage and retrying.");
            sb.Append("If this error persists, please report it on the DeepDungeonDex channel in the plugin help forum.");
            var errorMsg = sb.ToString();
            _log.Error(errorMsg);
            _chat.PrintError(errorMsg);
            Directory.Delete(_path, true);
            Directory.CreateDirectory(_path);
            Load();
        }
        Save();
    }

    private Configuration LoadConfig()
    {
        var configPath = Path.Combine(_path, "config.json");
        _log.Verbose("Loading config from {0}", configPath);
    otherFile:
        try
        {
            _log.Verbose("Deserializing config");
            Stream file = new FileStream(configPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            var binary = new BinaryReader(file);
            var config = new Configuration();
            if (binary.ReadByte() == 0x7B)
            {
                file.Position = 0;
                TextReader text = new StreamReader(file);
                JsonReader reader = new JsonTextReader(text);
                var obj = (JObject)JToken.ReadFrom(reader);
                if (obj.GetValue("Version")!.Value<int>() == 0)
                {
                    if (obj.TryGetValue("ClickThrough", out var clickThrough))
                        config.ClickThrough = clickThrough.Value<bool>();
                    if (obj.TryGetValue("HideRed", out var hideRed))
                        config.HideRed = hideRed.Value<bool>();
                    if (obj.TryGetValue("HideJob", out var hideJob))
                        config.HideJob = hideJob.Value<bool>();
                    if (obj.TryGetValue("HideFloor", out var hideFloor))
                        config.HideFloor = hideFloor.Value<bool>();
                    if (obj.TryGetValue("Debug", out var debug))
                        config.Debug = debug.Value<bool>();
                    if (obj.TryGetValue("Locale", out var locale))
                        config.Locale = locale.Value<int>();
                    if (obj.TryGetValue("FontSize", out var fontSize))
                        config.FontSize = fontSize.Value<int>();
                    if (obj.TryGetValue("Opacity", out var opacity))
                        config.Opacity = opacity.Value<float>();
                    if (obj.TryGetValue("LoadAll", out var loadAll))
                        config.LoadAll = loadAll.Value<bool>();
                }
                reader.Close();
                text.Close();
                file.Close();
                File.Delete(configPath);
            }
            else
            {
                file.Position = 0;
                switch (binary.ReadByte())
                {
                    case 1:
                        var flags = binary.ReadByte();
                        config.ClickThrough = (flags & 1) == 1;
                        config.HideRed = (flags & 2) == 2;
                        config.HideJob = (flags & 4) == 4;
                        config.HideFloor = (flags & 8) == 8;
                        config.HideSpawns = (flags & 16) == 16;
                        config.Debug = (flags & 32) == 32;
                        config.LoadAll = (flags & 64) == 64;
                        config.Locale = binary.ReadInt32();
                        config.FontSize = binary.ReadInt32();
                        config.Opacity = binary.ReadSingle();
                        break;
                    default:
                        throw new Exception("Invalid config version");
                }
            }
            binary.Close();
            file.Close();
            return config;
        }
        catch
        {
            if (!configPath.EndsWith(".dat"))
            {
                _log.Verbose("Failed to find old config file checking new file.");
                configPath = Path.Combine(_path, "config.dat");
                goto otherFile;
            }
            _log.Verbose("Failed to deserialize config, creating new config");
            return new Configuration();
        }
    }

    public static T? DeserializeFile<T>(string path, bool ignoreJsonProperty = true) where T : class
    {
        return DeserializeFile(path, typeof(T), ignoreJsonProperty) as T;
    }

    public static object? DeserializeFile(string path, Type type, bool ignoreJsonProperty = true)
    {
        _log.Verbose("Deserializing {0}", path);
        var result = JsonConvert.DeserializeObject(ReadFile(path), type, new JsonSerializerSettings()
        {
            ContractResolver = ignoreJsonProperty ? new NameContractResolver() : null
        });
        _log.Verbose("Deserialized {0}, Type: {1}", path, result?.GetType().ToString() ?? "");
        return result;
    }

    public static string ReadFile(string path)
    {
        _log.Verbose("Reading file {0}", path);
        var reader = new StreamReader(path);
        var result = reader.ReadToEnd();
        _log.Verbose("Read file {0}", path);
        _log.Verbose("Content: \n{0}", result);
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
            _log.Verbose("Saving: {0}", fileInfo);
            Directory.CreateDirectory(fileInfo.DirectoryName!);

            if (obj is Storage storage)
            {
                _log.Verbose("Saving inner obj");
                var k = storage.Value.Save(fileInfo.FullName)?.GetTuple();
                if (k == null)
                {
                    return false;
                }

                _log.Verbose($"Wrote inner obj of type {k.Item1.Name}");
                storageDict.Add(path, k);
            }
            else
            {
                if (!obj.GetType().IsAssignableFrom(typeof(ISaveable)))
                {
                    return true;
                }

                _log.Verbose("Saving obj");
                var k = (obj as ISaveable)!.Save(fileInfo.FullName)?.GetTuple();
                if (k == null)
                {
                    return false;
                }

                _log.Verbose($"Wrote obj of type {k.Item1.Name}");
                storageDict.Add(path, k);

            }

            return false;
        }

        _log.Verbose("Saving Json Storage");
        foreach (var (path, obj) in JsonStorage.ToDictionary(t => t.Key, t => t.Value))
        {
            if (!processObj(obj, path))
                continue;

            SerializeJsonFile(Path.Join(_path, path), obj);
            _log.Verbose($"Wrote {obj.GetType()}");
        }

        _log.Verbose("Saving Yaml Storage");
        foreach (var (path, obj) in YmlStorage.ToDictionary(t => t.Key, t => t.Value))
        {
            if (!processObj(obj, path))
                continue;

            SerializeYamlFile(Path.Join(_path, path), obj);
            _log.Verbose($"Wrote {obj.GetType()}");
        }

        _log.Verbose("Saving Binary Storage");
        foreach (var (path, obj) in BinaryStorage.ToDictionary(t => t.Key, t => t.Value))
        {
            var fullPath = Path.Join(_path, path);
            var named = obj.BinarySave(fullPath);
            if(named != null)
                storageDict.Add(fullPath, named.GetTuple());
            _log.Verbose($"Wrote {obj.GetType()}");
        }

        _log.Verbose("Filling missing storage data");
        JsonStorage.AsEnumerable()
            .Concat(YmlStorage)
            .Concat(BinaryStorage.ToDictionary(t => t.Key, t => (object)t.Value))
            .ToList()
            .ForEach(x =>
            {
                var (path, obj) = x;
                var type = obj is Storage storage ? storage.Value.GetType() : obj.GetType();
                if (!storageDict.ContainsKey(path))
                    storageDict.Add(path, new Tuple<Type, string?>(type, null));
            });
        var storagePath = Path.Combine(_path, "storage.json");
        _log.Verbose($"Writing {storagePath}");
        var storageInfo = storageDict.Where(t => t.Key is not ("storage.json" or "config.json" or "")).ToDictionary(
            t => t.Key, t =>
            {
                var (type, name) = t.Value;
                return new Tuple<string, string?>(type.FullName!, name);
            });
        SerializeJsonFile(storagePath, storageInfo);
        _log.Verbose("Saved");
        storageDict.Clear();
        storageInfo.Clear();
    }

    public T? GetInstance<T>() where T : class, ISaveable
    {
        var list = JsonStorage.Values.ToList();
        list.AddRange(YmlStorage.Values);
        list.AddRange(BinaryStorage.Select(t => (object)t.Value));
        return (list.FirstOrDefault(x => x is T) ?? (list.FirstOrDefault(x => x is Storage { Value: T }) as Storage)?.Value) as T ?? null;
    }

    public T? GetInstance<T>(string path) where T : class, ISaveable
    {
        var list = JsonStorage.ToList();
        list.AddRange(YmlStorage);
        list.AddRange(BinaryStorage.ToDictionary(t => t.Key, t => (object)t.Value));
        var set = list.Where(t => t.Key.Contains(path)).Select(t => t.Value).ToList();
        return (set.FirstOrDefault(x => x is T) ?? (set.FirstOrDefault(x => x is Storage { Value: T }) as Storage)?.Value) as T ?? null;
    }

    public T[] GetInstances<T>() where T : class, ISaveable
    {
        var list = JsonStorage.Values.ToList();
        list.AddRange(YmlStorage.Values);
        list.AddRange(BinaryStorage.Select(t => (object)t.Value));
        return list.Where(t => t is T or Storage { Value: T }).Select(t => t is T ? t : (t as Storage)?.Value).Cast<T>().ToArray();
    }

    public T[] GetInstances<T>(string name) where T : class, ISaveable
    {
        var list = JsonStorage.Values.ToList();
        list.AddRange(YmlStorage.Values);
        list.AddRange(BinaryStorage.Select(t => (object)t.Value));
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
        list.AddRange(BinaryStorage.ToDictionary(t => t.Key, t => (object)t.Value));
        var filePath = list.FirstOrDefault(t => t.Value.GetType() == type || t.Value is Storage { Value: { } value } && value.GetType() == type).Key;
        return Path.Combine(_path, filePath);
    }

    public void Dispose()
    {
        Save();
        foreach (var (_, obj) in JsonStorage)
        {
            (obj as IDisposable)?.Dispose();
        }

        foreach (var (_, obj) in YmlStorage)
        {
            (obj as IDisposable)?.Dispose();
        }

        foreach (var (_, obj) in BinaryStorage)
        {
            (obj as IDisposable)?.Dispose();
        }
        JsonStorage.Clear();
        YmlStorage.Clear();
        _serializer = null!;
        Deserializer = null!;
        _log = null!;
        _chat = null!;
    }
}

public class Storage : IDisposable
{
    public Storage(ISaveable value, string name = "")
    {
        Value = value;
        Name = name;
    }

    public ISaveable Value { get; set; }
    public string Name { get; set; }

    public void Dispose()
    {
        Value.Dispose();
        Value = null!;
    }
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