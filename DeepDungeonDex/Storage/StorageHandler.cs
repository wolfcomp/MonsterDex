using System.IO;
using System.Reflection;
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

    internal readonly Dictionary<string, object> Storage = new();

    public event Action<StorageEventArgs>? StorageChanged;

    public StorageHandler(DalamudPluginInterface pluginInterface, IChatGui chat, IPluginLog log)
    {
        _path = pluginInterface.GetPluginConfigDirectory();
        _chat = chat;
        _log = log;
        AddStorage("config.dat", LoadConfig());
    }

    public void AddStorage(string path, object storage)
    {
        Storage[path] = storage;
        StorageChanged?.Invoke(new StorageEventArgs(storage.GetType()));
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
                byte flags;
                switch (binary.ReadByte())
                {
                    case 1:
                        flags = binary.ReadByte();
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
                    case 2:
                        flags = binary.ReadByte();
                        config.ClickThrough = (flags & (1 << 1)) == (1 << 1);
                        config.HideFloor = (flags & (1 << 2)) == (1 << 2);
                        config.HideSpawns = (flags & (1 << 3)) == (1 << 3);
                        config.Debug = (flags & (1 << 4)) == (1 << 4);
                        config.LoadAll = (flags & (1 << 5)) == (1 << 5);
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

    public T? GetInstance<T>() where T : class
    {
        var list = Storage.Values.ToList();
        return list.FirstOrDefault(x => x is T) as T ?? null;
    }

    public T? GetInstance<T>(string path) where T : class
    {
        var list = Storage.ToList();
        var set = list.Where(t => t.Key.Contains(path)).Select(t => t.Value).ToList();
        return set.FirstOrDefault(x => x is T) as T ?? null;
    }

    public T[] GetInstances<T>() where T : class
    {
        var list = Storage.Values.ToList();
        return list.Where(t => t is T).Cast<T>().ToArray();
    }

    public object[] GetAllExceptInstances<T>() where T : class
    {
        var list = Storage.Values.ToList();
        return list.Where(t => t is not T).ToArray();
    }

    public T[] GetInstances<T>(string path) where T : class
    {
        var list = Storage.ToList();
        return list.Where(t => t.Key.Contains(path)).Select(t => t.Value).Cast<T>().ToArray();
    }

    public object? GetInstance(string path)
    {
        return Storage.GetValueOrDefault(path);
    }

    public string GetFilePath(Type type)
    {
        var list = Storage.ToList();
        var filePath = list.FirstOrDefault(t => t.Value.GetType() == type).Key;
        return Path.Combine(_path, filePath);
    }

    public void Dispose()
    {
        foreach (var (_, obj) in Storage)
        {
            if (obj is IDisposable disposable)
                disposable.Dispose();
        }
        Storage.Clear();
        _serializer = null!;
        Deserializer = null!;
        _log = null!;
        _chat = null!;
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
            parser.TryConsume<SequenceStart>(out var sequence);
            if (sequence != null)
            {
                while (parser.TryConsume<Scalar>(out var scalar))
                {
                    items.Add(scalar.Value);
                }

                parser.TryConsume<SequenceEnd>(out _);
            }
            else
            {
                if (parser.TryConsume<Scalar>(out var scalar))
                    items.Add(scalar.Value);
            }
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