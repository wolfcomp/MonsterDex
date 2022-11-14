using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using DeepDungeonDex.Models;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage
{
    public class MobData : ILoadable<MobData>
    {
        public Dictionary<int, Mob> MobDictionary { get; set; } = new();

        public MobData Load(string path)
        {
            var mobs = StorageHandler.Deserializer.Deserialize<Dictionary<string, Mob>>(StorageHandler.ReadFile(path));
            foreach (var (key, value) in mobs)
            {
                var splitInd = key.IndexOf('-');
                var id = int.Parse(key[..splitInd]);
                var name = key[(splitInd + 1)..];
                value.Name = name;
                MobDictionary.Add(id, value);
            }
            return this;
        }

        public void Save(string path)
        {
            MobDictionary.Select(t => (t.Key, t.Value)).ToDictionary(t => $"{t.Key}-{t.Value.Name}", t => t.Value);
        }

        public Action<DateTime> Updated { get; set; }
        object ILoadable.Load(string path) => Load(path);
    }

    public class Mob
    {
        [YamlIgnore]
        public string Name { get; set; }
        public Weakness Weakness { get; set; }
        public Aggro Aggro { get; set; }
        public Threat Threat { get; set; }
    }

    [Flags]
    public enum Weakness
    {
        None = 0x00,
        Stun = 0x01,
        Heavy = 0x02,
        Slow = 0x04,
        Sleep = 0x08,
        Bind = 0x10,
        Undead = 0x20
    }

    public enum Aggro
    {
        Undefined,
        Sight,
        Sound,
        Proximity,
        Boss
    }

    public enum Threat
    {
        Undefined,
        Easy,
        Caution,
        Dangerous,
        Vicious
    }
}
