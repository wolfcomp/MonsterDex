using System.Drawing;
using System.IO;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage;

public class MobData : ILoadableString, IBinaryLoadable
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

    public void Dispose()
    {
        MobDictionary.Clear();
    }

    public IBinaryLoadable StringLoad(string str)
    {
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

        return this;
    }

    public NamedType? BinarySave(string path)
    {
        var temp = path[..^4];
        var stream = File.Open(temp, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
        var writer = new BinaryWriter(stream);
        writer.Write(MobDictionary.Count);
        foreach (var (key, value) in MobDictionary)
        {
            writer.Write(key);
            writer.Write((ushort)value.Weakness);
            writer.Write((byte)((byte)value.Aggro << 4) + (byte)value.Threat);
        }
        writer.Dispose();
        stream.Dispose();
        File.Delete(path);
        File.Move(temp, path);
        return null;
    }

    public IBinaryLoadable BinaryLoad(string path)
    {
        var stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
        var reader = new BinaryReader(stream);
        if (reader.BaseStream.Length <= 0)
        {
            return null!;
        }

        var count = reader.ReadUInt32();
        for (var i = 0; i < count; i++)
        {
            var key = reader.ReadUInt32();
            var weakness = (Weakness)reader.ReadUInt16();
            var aggro = (Aggro)(reader.ReadByte() >> 4);
            var threat = (Threat)(reader.ReadByte() & 0x0F);
            MobDictionary.Add(key, new Mob
            {
                Aggro = aggro,
                Id = key,
                Threat = threat,
                Weakness = weakness
            });
        }
        return this;
    }
}

public record Mob
{
    private string[][] _description = Array.Empty<string[]>();

    [YamlIgnore] 
    public string Name { get; set; } = "";
    [YamlIgnore]
    public uint Id { get; set; }

    [YamlIgnore]
    public string[][]? Description
    {
        get => _description;
        set
        {
            if(value is null)
                return;
            _description = value;
            JoinedProcessedDescription = string.Join("\n", _description.Select(t => string.Join(" ", t)));
        }
    }

    [YamlIgnore]
    public string[] ProcessedDescription { get; private set; } = Array.Empty<string>();

    [YamlIgnore] 
    public string JoinedProcessedDescription { get; private set; } = "";
    [YamlIgnore]
    public float LastProcessedWidth { get; set; }

    public Weakness Weakness { get; set; }
    public Aggro Aggro { get; set; }
    public Threat Threat { get; set; }

    public void ProcessDescription(float width)
    {
        var strList = new List<string>();
        foreach (var t1 in Description!)
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
        ProcessedDescription = strList.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
    }
}

[Flags]
public enum Weakness : ushort
{
    None = 0x00,
    Stun = 0x01,
    Heavy = 0x02,
    Slow = 0x04,
    Sleep = 0x08,
    Bind = 0x10,
    Undead = 0x20, // currently only for mobs in PotD floors 51-200
    StunUnknown = 0x40 | Stun,
    HeavyUnknown = 0x80 | Heavy,
    SlowUnknown = 0x100 | Slow,
    SleepUnknown = 0x200 | Sleep,
    BindUnknown = 0x400 | Bind,
    UndeadUnknown = 0x800 | Undead,
    All = Stun | Heavy | Slow | Sleep | Bind,
    AllUnknown = StunUnknown | HeavyUnknown | SlowUnknown | SleepUnknown | BindUnknown
}

public enum Aggro : byte
{
    Undefined,
    Sight,
    Sound,
    Proximity,
    Boss,
    // currently unused but will be used in the future for enemies in other content
    Bloodlust,
    Magic
}

public enum Threat : byte
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