using DeepDungeonDex.Models;
using System.Collections.Generic;

namespace DeepDungeonDex.Storage
{ 
    public class JobData : ILoadableString
    {
        public Dictionary<uint, Weakness> JobDictionary { get; set; } = new();

        public NamedType? Save(string path)
        {
            StorageHandler.SerializeYamlFile(path, JobDictionary);
            return null;
        }

        public Storage Load(string path)
        {
            JobDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<uint, Weakness>>(StorageHandler.ReadFile(path));
            return new Storage(this);
        }

        public Storage Load(string path, string name)
        {
            JobDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<uint, Weakness>>(StorageHandler.ReadFile(path));
            return new Storage(this)
            {
                Name = name
            };
        }

        public Storage Load(string str, bool fromFile)
        {
            if (fromFile)
                return Load(str);
            JobDictionary = StorageHandler.Deserializer.Deserialize<Dictionary<uint, Weakness>>(str);
            return new Storage(this);
        }
    }
}
