using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using YamlDotNet.Serialization;

namespace DeepDungeonDex
{
    [Flags]
    public enum Vulnerabilities
    {
        None = 0x00,
        Stun = 0x01,
        Heavy = 0x02,
        Slow = 0x04,
        Sleep = 0x08,
        Bind = 0x10,
        Undead = 0x20
    }

    public enum ThreatLevel
    {
        Undefined,
        Easy,
        Caution,
        Dangerous,
        Vicious
    }

    public enum AggroType
    {
        Undefined,
        Sight,
        Sound,
        Proximity,
        Boss
    }

    public struct MobData
    {
        public Vulnerabilities Vuln { get; set; }
        public ThreatLevel Threat { get; set; }
        public AggroType Aggro { get; set; }

        #region Constructors
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="vuln">The vulnerabilities for the mob</param>
        public MobData(ThreatLevel level, AggroType aggro, Vulnerabilities vuln)
        {
            Threat = level;
            Aggro = aggro;
            Vuln = vuln;
        }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        public MobData(ThreatLevel level, AggroType aggro) : this(level, aggro, Vulnerabilities.None) { }
        #endregion
    }

    public struct ClassData
    {
        public bool Stun;
        public bool Heavy;
        public bool Slow;
        public bool Sleep;
        public bool Bind;

        #region Constructors
        /// <summary>
        /// Class data struct init
        /// </summary>
        /// <param name="stun">Can job stun</param>
        /// <param name="heavy">Can job heavy</param>
        /// <param name="slow">Can job slow</param>
        /// <param name="sleep">Can job sleep</param>
        /// <param name="bind">Can job bind</param>
        public ClassData(bool stun, bool heavy, bool slow, bool sleep, bool bind)
        {
            Stun = stun;
            Heavy = heavy;
            Slow = slow;
            Sleep = sleep;
            Bind = bind;
        }

        public ClassData(byte data) : this((data & 0x01) == 1, (data & 0x02) == 2, (data & 0x04) == 4, (data & 0x08) == 8, (data & 0x10) == 16) { }
        #endregion

        public bool CheckFromVuln(Vulnerabilities vuln) => vuln switch
        {
            Vulnerabilities.Stun => Stun,
            Vulnerabilities.Heavy => Heavy,
            Vulnerabilities.Slow => Slow,
            Vulnerabilities.Sleep => Sleep,
            Vulnerabilities.Bind => Bind,
            _ => false
        };
    }

    public struct YamlData
    {
        [YamlMember(Alias = "threat")]
        public string Threat;
        [YamlMember(Alias = "aggro")]
        public string Aggro;
        [YamlMember(Alias = "vulns")]
        public List<string> Vulns;

        public MobData ToMobData()
        {
            var threat = (ThreatLevel)Enum.Parse(typeof(ThreatLevel), Threat);
            var aggro = (AggroType)Enum.Parse(typeof(AggroType), Aggro);
            return Vulns is not null ? new MobData(threat, aggro, (Vulnerabilities)Vulns.Select(x => (int)Enum.Parse(typeof(Vulnerabilities), x)).Sum()) : new MobData(threat, aggro);
        }
    }

    public class DataHandler
    {
        public static MobData? Mobs(uint nameId)
        {
            if (_mobs.TryGetValue(nameId, out var value)) return value;
            return null;
        }

        public static bool ShouldRender(uint jobId, Vulnerabilities vuln)
        {
            var ret = false;
            if (_jobs.TryGetValue(jobId, out var value)) ret = value.CheckFromVuln(vuln);
            return ret;
        }

        public static void SetupData()
        {
            _mobs.Clear();
            using var stream = new HttpClient().GetStreamAsync("https://raw.githubusercontent.com/wolfcomp/DeepDungeonDex/dev/mobData.yml").GetAwaiter().GetResult();
            using var reader = new StreamReader(stream);
            var data = Plugin.Deserializer.Deserialize<Dictionary<uint, YamlData>>(reader);
            foreach (var (key, value) in data)
            {
                _mobs.Add(key, value.ToMobData());
            }
        }

        private static readonly Dictionary<uint, MobData> _mobs = new();

        private static readonly Dictionary<uint, ClassData> _jobs = new()
        {
            {1, new ClassData(31)},
            {2, new ClassData(5)},
            {3, new ClassData(5)},
            {4, new ClassData(5)},
            {5, new ClassData(5)},
            {6, new ClassData(22)},
            {7, new ClassData(8)},
            {8, new ClassData(8)},
            {9, new ClassData(0)},
            {10, new ClassData(0)},
            {11, new ClassData(0)},
            {12, new ClassData(0)},
            {13, new ClassData(0)},
            {14, new ClassData(0)},
            {15, new ClassData(0)},
            {16, new ClassData(0)},
            {17, new ClassData(0)},
            {18, new ClassData(0)},
            {19, new ClassData(0)},
            {20, new ClassData(5)},
            {21, new ClassData(5)},
            {22, new ClassData(5)},
            {23, new ClassData(5)},
            {24, new ClassData(22)},
            {25, new ClassData(9)},
            {26, new ClassData(8)},
            {27, new ClassData(8)},
            {28, new ClassData(8)},
            {29, new ClassData(8)},
            {30, new ClassData(5)},
            {31, new ClassData(23)},
            {32, new ClassData(22)},
            {33, new ClassData(5)},
            {34, new ClassData(8)},
            {35, new ClassData(5)},
            {36, new ClassData(8)},
            {37, new ClassData(31)},
            {38, new ClassData(5)},
            {39, new ClassData(22)},
            {40, new ClassData(5)},
            {41, new ClassData(8)}
        };
    }
}
