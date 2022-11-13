using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage
{
    public class StorageHandler : IDisposable
    {
        private readonly string _path;
        private readonly Deserializer _deserializer = new();

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
                if (!type.IsAssignableFrom(typeof(ISave))) continue;
                if (key.Contains(".json"))
                {
                    if (DeserializeFile(key, type) is ISave content) _jsonStorage.Add(key, new Storage(content, time));
                }
                else
                {
                    if (_deserializer.Deserialize(key, type) is ISave content) _ymlStorage.Add(key, new Storage(content, time));
                }
            }
        }

        private static T? DeserializeFile<T>(string path, bool ignoreJsonProperty = true) where T : class
        {
            return DeserializeFile(path, typeof(T), ignoreJsonProperty) as T;
        }

        private static object? DeserializeFile(string path, Type type, bool ignoreJsonProperty = true)
        {
            var reader = new StreamReader(path);
            var result = JsonConvert.DeserializeObject(reader.ReadToEnd(), type, new JsonSerializerSettings()
            {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = ignoreJsonProperty ? new NameContractResolver() : null
            });
            reader.Dispose();
            return result;
        }

        public static void SerializeFile(string path, object obj)
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

        public void Save()
        {
            foreach (var (path, obj) in _jsonStorage)
            {
                if (obj is Storage storage)
                {
                    SerializeFile(path, storage.Value);
                }
                else
                {
                    SerializeFile(path, obj);
                }
            }
        }

        public T? GetInstance<T>() where T : class, ISave
        {
            var list = _jsonStorage.Values.ToList();
            list.AddRange(_ymlStorage.Values);
            return list.FirstOrDefault(t => t is T or Storage { Value: T }) as T;
        }

        public string GetFilePath(Type type)
        {
            var list = _jsonStorage.ToList();
            list.AddRange(_ymlStorage);
            var filePath = list.FirstOrDefault(t => t.Value.GetType() == type || t.Value is Storage { Value: { } value } && value.GetType() == type).Key;
            return filePath.Replace(_path, "");
        }

        public void Dispose()
        {
            Save();
        }
    }

    internal class Storage
    {
        public Storage(ISave value, DateTime lastUpdated)
        {
            Value = value;
            LastUpdated = lastUpdated;
        }

        public ISave Value { get; set; }
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
}
