using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Plugin;
using DeepDungeonDex.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage
{
    public class StorageHandler : IDisposable
    {
        private readonly string _path;
        public static readonly IDeserializer Deserializer = new DeserializerBuilder().WithTypeConverter(new YamlStringEnumConverter()).Build();
        private static readonly Serializer Serializer = new();

        private readonly Dictionary<string, object> _jsonStorage = new();
        private readonly Dictionary<string, object> _ymlStorage = new();

        public StorageHandler(DalamudPluginInterface pluginInterface)
        {
            _path = pluginInterface.GetPluginConfigDirectory();
        }

        private void Load()
        {
            var oldConfig = new FileInfo(_path + ".json");
            var configPath = oldConfig.Exists ? oldConfig.FullName : Path.Combine(_path, "config.json");
            var config = DeserializeFile<Configuration>(configPath)!;
            _jsonStorage.Add(configPath, config);
            var storagePath = Path.Combine(_path, "storage.json");
            var storage = DeserializeFile<Dictionary<string, KeyValuePair<Type, DateTime>>>(storagePath)!;
            _jsonStorage.Add(storagePath, storage);
            foreach (var (key, value) in storage)
            {
                var (type, time) = value;
                if (!type.IsAssignableFrom(typeof(ISaveable))) continue;
                if (key.Contains(".json"))
                {
                    if (type.IsAssignableFrom(typeof(ILoadable)))
                    {
                        var loadable = Activator.CreateInstance(type) as ILoadable;
                        var obj = loadable?.Load(key);
                        if (obj != null)
                        {
                            _jsonStorage.Add(key, obj);
                        }
                    }
                    else if (DeserializeFile(key, type) is ISaveable content) _jsonStorage.Add(key, new Storage(content, time));
                }
                else
                {
                    if (type.IsAssignableFrom(typeof(ILoadable)))
                    {
                        var loadable = Activator.CreateInstance(type) as ILoadable;
                        var obj = loadable?.Load(key);
                        if (obj != null)
                        {
                            _ymlStorage.Add(key, obj);
                        }
                    }
                    else if (Deserializer.Deserialize(key, type) is ISaveable content) _ymlStorage.Add(key, new Storage(content, time));
                }
            }
        }

        public static T? DeserializeFile<T>(string path, bool ignoreJsonProperty = true) where T : class
        {
            return DeserializeFile(path, typeof(T), ignoreJsonProperty) as T;
        }

        public static object? DeserializeFile(string path, Type type, bool ignoreJsonProperty = true)
        {
            var result = JsonConvert.DeserializeObject(ReadFile(path), type, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = ignoreJsonProperty ? new NameContractResolver() : null
            });
            return result;
        }

        public static string ReadFile(string path)
        {
            var reader = new StreamReader(path);
            var result = reader.ReadToEnd();
            reader.Dispose();
            return result;
        }

        public static void SerializeJsonFile(string path, object obj)
        {
            var writer = new StreamWriter(path);
            writer.Write(JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new NameContractResolver()
            }));
            writer.Dispose();
        }

        public static void SerializeYamlFile(string path, object obj)
        {
            var writer = new StreamWriter(path);
            writer.Write(Serializer.Serialize(obj));
            writer.Dispose();
        }

        public void Save()
        {
            foreach (var (path, obj) in _jsonStorage)
            {
                if (obj is Storage storage)
                {
                    storage.Value.Save(path);
                }
                else
                {
                    SerializeJsonFile(path, obj);
                }
            }

            foreach (var (path, obj) in _jsonStorage)
            {
                if (obj is Storage storage)
                {
                    storage.Value.Save(path);
                }
                else
                {
                    SerializeYamlFile(path, obj);
                }
            }
        }

        public T? GetInstance<T>() where T : class, ISaveable
        {
            var list = _jsonStorage.Values.ToList();
            list.AddRange(_ymlStorage.Values);
            return list.FirstOrDefault(t => t is T or Storage { Value: T }) as T;
        }

        public T[] GetInstances<T>() where T : class, ISaveable
        {
            var list = _jsonStorage.Values.ToList();
            list.AddRange(_ymlStorage.Values);
            return list.Where(t => t is T or Storage { Value: T }).Select(t => t is T ? t : (t as Storage)?.Value).Cast<T>().ToArray();
        }

        public string GetFilePath(Type type)
        {
            var list = _jsonStorage.ToList();
            list.AddRange(_ymlStorage);
            var filePath = list.FirstOrDefault(t => t.Value.GetType() == type || t.Value is Storage { Value: { } value } && value.GetType() == type).Key;
            return filePath.Replace(_path, "");
        }

        public bool IsOld(Type? type = null, string? path = null)
        {
            if (type != null)
            {
                path = GetFilePath(type);
            }
            if (path == null) return false;
            var list = _jsonStorage.ToList();
            list.AddRange(_ymlStorage);
            var obj = list.FirstOrDefault(t => t.Key == path).Value;
            if (obj is Storage { Value: { } } storage)
            {
                return storage.LastUpdated < DateTime.Now.AddHours(-4);
            }
            return false;
        }

        public void Dispose()
        {
            Save();
        }
    }

    internal class Storage
    {
        public Storage(ISaveable value, DateTime lastUpdated)
        {
            Value = value;
            LastUpdated = lastUpdated;
            value.Updated += time => LastUpdated = time;
        }

        public ISaveable Value { get; set; }
        public DateTime LastUpdated { get; set; }

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
}
