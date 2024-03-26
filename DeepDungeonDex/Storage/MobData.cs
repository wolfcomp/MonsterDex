using FFXIVClientStructs.FFXIV.Client.Game.Character;
using YamlDotNet.Serialization;

namespace DeepDungeonDex.Storage;

public class MobData : ILoad<MobData>
{
    public Dictionary<uint, Mob> MobDictionary { get; set; } = new();

    public MobData Load(string str)
    {
        var mobs = StorageHandler.Deserializer.Deserialize<Dictionary<string, Mob>>(str);
        foreach (var (key, value) in mobs)
        {
            var splitInd = key.IndexOf('-');
            var id = uint.Parse(key[..splitInd]);
            var name = key[(splitInd + 1)..];
            value.Name = name;
            MobDictionary.Add(id, value);
        }
        return this;
    }

    public void Dispose()
    {
        MobDictionary.Clear();
    }
    object ILoad.Load(string str) => Load(str);
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
    public ContentType InstanceContentType { get; set; }
    public Dictionary<uint, ElementalChangeTime> ElementalChangeTimes { get; set; } = new();
    public Character.EurekaElement MutatedElementalType { get; set; }
    public bool IsMutation { get; set; }
    public bool IsAaptation { get; set; }

    public void ProcessDescription(float width)
    {
        var strList = new List<string>();
        foreach (var t1 in Description!)
        {
            var s = "";
            foreach (var t in t1)
            {
                if (ImGui.CalcTextSize(s + t + " ").X < width)
                {
                    s += t + " ";
                }
                else
                {
                    strList.Add(s.Length > 0 ? s[..^1] : "");
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

[Flags]
public enum ContentType : uint
{
    None = 0,
    Raid = 1,
    Dungeon = 1 << 1,
    GuildOrder = 1 << 2,
    Trial = 1 << 3,
    CrystallineConflict = 1 << 4,
    Frontlines = 1 << 5,
    QuestBattle = 1 << 6,
    BeginnerTraining = 1 << 7,
    DeepDungeon = 1 << 8,
    TreasureHuntDungeon = 1 << 9,
    SeasonalDungeon = 1 << 10,
    RivalWing = 1 << 11,
    MaskedCarnivale = 1 << 12,
    Mahjong = 1 << 13,
    GoldSaucer = 1 << 14,
    OceanFishing = 1 << 15,
    UnrealTrial = 1 << 16,
    TripleTriad = 1 << 17,
    VariantDungeon = 1 << 18,
    CriterionDungeon = 1 << 19,
    BondingCeremony = 1 << 20,
    PublicTripleTriad = 1 << 21,
    Eureka = 1 << 22,
    CalamityRetold = 1 << 23, // seems to be only for the rising event in 2018
    LeapOfFaith = 1 << 24,
    Diadem = 1 << 25,
    Bozja = 1 << 26,
    Delubrum = 1 << 27,
    IslandSanctuary = 1 << 28,
    FallGuys = 1 << 29,
}

[Flags]
public enum ElementalChangeTime : byte
{
    None,
    Night,
    Day,
    Both = Night | Day
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

    public static bool HasAnyFlag(this ContentType type, ContentType flag) => (type & flag) != 0;
}

