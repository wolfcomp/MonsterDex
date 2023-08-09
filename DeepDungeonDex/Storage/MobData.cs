using System.Drawing;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage;

public class MobData : ILoadableString
{
    public Dictionary<uint, Mob> MobDictionary { get; set; } = new();

    public Storage Load(string path)
    {
        var mobs = StorageHandler.Deserializer.Deserialize<Dictionary<string, Mob>>(StorageHandler.ReadFile(path));
        foreach (var (key, value) in mobs)
        {
            var splitInd = key.IndexOf('-');
            var id = uint.Parse(key[..splitInd]);
            var name = key[(splitInd + 1)..];
            value.Name = name;
            MobDictionary.Add(id, value);
        }
        return new Storage(this);
    }

    public Storage Load(string path, string name)
    {
        var mobs = StorageHandler.Deserializer.Deserialize<Dictionary<string, Mob>>(StorageHandler.ReadFile(path));
        foreach (var (key, value) in mobs)
        {
            var splitInd = key.IndexOf('-');
            var id = uint.Parse(key[..splitInd]);
            var vName = key[(splitInd + 1)..];
            value.Name = vName;
            value.Id = id;
            MobDictionary.Add(id, value);
        }
            
        return new Storage(this)
        {
            Name = name
        };
    }

    public Storage Load(string str, bool fromFile)
    {
        if (fromFile)
            return Load(str);
        var mobs = StorageHandler.Deserializer.Deserialize<Dictionary<string, Mob>>(str);
        foreach (var (key, value) in mobs)
        {
            var splitInd = key.IndexOf('-');
            var id = uint.Parse(key[..splitInd]);
            var name = key[(splitInd + 1)..];
            value.Name = name;
            value.Id = id;
            MobDictionary.Add(id, value);
        }
        return new Storage(this);
    }

    public NamedType? Save(string path)
    {
        StorageHandler.SerializeYamlFile(path, MobDictionary.Select(t => (t.Key, t.Value)).ToDictionary(t => $"{t.Key}-{t.Value.Name}", t => t.Value));
        return null;
    }
}

public class Mob
{
    [YamlIgnore]
    public string Name { get; set; }
    [YamlIgnore]
    public uint Id { get; set; }
    [YamlIgnore]
    public string[][]? Description { get; set; }

    [YamlIgnore]
    public string[] ProcessedDescription { get; private set; } = Array.Empty<string>();

    public Weakness Weakness { get; set; }
    public Aggro Aggro { get; set; }
    public Threat Threat { get; set; }

    public void ProcessDescription(float width)
    {
        var strList = new List<string>();
        foreach (var t1 in Description)
        {
            var s = "";
            foreach (var t in t1)
            {
                if (ImGui.CalcTextSize(s + t).X < width)
                {
                    s += t + " ";
                }
                else
                {
                    strList.Add(s[..^1]);
                    s = t + " ";
                }
            }
            if (s.Length > 0)
                strList.Add(s[..^1]);
        }
        ProcessedDescription = strList.ToArray()[..^1];
    }
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
    Undead = 0x20,
    StunUnknown = 0x40 | Stun,
    HeavyUnknown = 0x80 | Heavy,
    SlowUnknown = 0x100 | Slow,
    SleepUnknown = 0x200 | Sleep,
    BindUnknown = 0x400 | Bind,
    UndeadUnknown = 0x800 | Undead,
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

public static class MobDataExtensions
{
    public static Mob? GetData(this MobData data, uint key)
    {
        return data.MobDictionary.TryGetValue(key, out var value) ? value : null;
    }

    public static Mob? GetData(this IEnumerable<MobData> data, uint key)
    {
        return data.FirstOrDefault(l => l.MobDictionary.ContainsKey(key))?.MobDictionary[key];
    }

    public static uint GetColor(this Threat threat) => threat switch
    {
        Threat.Easy => 0xFF00FF00,
        Threat.Caution => 0xFF00FFFF,
        Threat.Dangerous => 0xFF0000FF,
        Threat.Vicious => 0xFFFF00FF,
        _ => 0xFFFFFFFF
    };
}