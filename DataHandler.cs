using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

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
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="stun">Can the mob be stunned</param>
        public MobData(ThreatLevel level, AggroType aggro, bool stun) : this(level, aggro, Tuple.Create(stun)) { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="vulnTuple">Can the mob be afflicted with stun, heavy, slow, sleep and bind and is the mob undead. (stun, heavy, slow, sleep, bind, undead)</param>
        /// <param name="mobNotes">Notes for the mob</param>
        public MobData(ThreatLevel level, AggroType aggro, ITuple vulnTuple) : this(level, aggro, vulnTuple.Get()) { }
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

        private static readonly Dictionary<uint, MobData> _mobs = new Dictionary<uint, MobData>
        {
			// HoH floors 1-9
            {7262, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7263, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7264, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7265, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, true)},
            {7266, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7267, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7268, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7269, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {7270, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7271, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7272, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7273, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7274, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            // HoH floors 11-19
            {7275, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7276, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {7277, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7278, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7279, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7280, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {7281, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7282, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7283, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7284, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7285, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7286, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7287, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            // HoH floors 21-29
            {7288, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {7289, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {7290, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {7291, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {7292, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7293, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7294, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {7295, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7296, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7297, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true))},
            {7298, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {7299, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7300, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            // HoH floors 31-39
            {7301, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7302, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7303, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7304, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7305, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7306, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7307, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7308, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7309, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7310, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7311, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7312, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7313, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            // HoH floors 41-49
            {7314, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7315, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {7316, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7317, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7318, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7319, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7320, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7321, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7322, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7323, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7324, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7325, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7326, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            // HoH floors 51-59
            {7327, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            {7328, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            {7329, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7330, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7331, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7332, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7333, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7334, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7335, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7336, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7337, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7338, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7339, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            // HoH floors 61-69
            {7340, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            {7341, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7342, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7343, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7344, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {7345, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7346, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7347, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7348, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7349, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7350, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7351, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            // HoH floors 71-79
            {7352, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7353, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7354, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7355, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7356, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7357, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {7358, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7359, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, true)},
            {7360, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7361, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7362, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7363, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7364, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            // HoH floors 81-89
            {7365, new MobData(ThreatLevel.Dangerous, AggroType.Proximity)},
            {7366, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {7367, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7368, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7369, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {7370, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7371, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7372, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7373, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7374, new MobData(ThreatLevel.Easy, AggroType.Proximity)},
            {7375, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {7376, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {7377, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            // HoH floors 91-99
            {7378, new MobData(ThreatLevel.Vicious, AggroType.Sight, true)},
            {7379, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7380, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7381, new MobData(ThreatLevel.Dangerous, AggroType.Proximity)},
            {7382, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7383, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7384, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7385, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7386, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {7387, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {7388, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {7389, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {7390, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {7391, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {7584, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            // HoH bosses and misc.
            {7392, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7393, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {7394, new MobData(ThreatLevel.Caution, AggroType.Proximity)},
            {7478, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7480, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7481, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7483, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7485, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7487, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7489, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7490, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7493, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            {7610, new MobData(ThreatLevel.Easy, AggroType.Proximity)},

            // PotD data
            // PotD 1-10
            {4975, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4976, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4977, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {4978, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4979, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {4980, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4981, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {4982, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, false, false, true))},
            {4983, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4984, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4985, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {4986, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 11-20
            {4987, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, false, true))},
            {4988, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {4989, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {4990, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            {4991, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true))},
            {4992, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true))},
            {4993, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {4994, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {4995, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {4996, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {4997, new MobData(ThreatLevel.Dangerous, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {4998, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, true))},
            {4999, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 21-30
            {5000, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5001, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5002, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, true))},
            {5003, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5004, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {5005, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5006, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {5007, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, true))},
            {5008, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5009, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true))},
            {5010, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true))},
            {5011, new MobData(ThreatLevel.Caution, AggroType.Sound, Tuple.Create(true, true))},
            {5012, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 31-40
            {5013, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5014, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5015, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5016, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, false, false, true))},
            {5017, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5018, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5019, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5020, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5021, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5022, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {5023, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5024, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5025, new MobData(ThreatLevel.Easy, AggroType.Boss, Tuple.Create(false, false, false, false, false, true))},
            // PotD 41-50
            {5026, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5027, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5028, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5029, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5030, new MobData(ThreatLevel.Caution, AggroType.Sound, Tuple.Create(true, false, false, true))},
            {5031, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5032, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {5033, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5034, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5035, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5036, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5037, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(false, false, false, false, false, true))},
            {5038, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD special NPCs and misc.
            {5039, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5040, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5041, new MobData(ThreatLevel.Easy, AggroType.Proximity)},
            {5046, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5047, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5048, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5049, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5050, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5051, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5052, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5053, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5283, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5284, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5285, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5286, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5287, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5288, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5289, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5290, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5291, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5292, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5293, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5294, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5295, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5296, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5297, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            {5298, new MobData(ThreatLevel.Caution, AggroType.Sight)},
            // PotD 51-60
            {5299, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5300, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5301, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5302, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5303, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5304, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5305, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5306, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5307, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5308, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5309, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 61-70
            {5311, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5312, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5313, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5314, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5315, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5316, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5317, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5318, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5319, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5320, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5321, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 71-80
            {5322, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5323, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, false, false, true))},
            {5324, new MobData(ThreatLevel.Easy, AggroType.Proximity)},
            {5325, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5326, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5327, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5328, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5329, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5330, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5331, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5332, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5333, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 81-90
            {5334, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5335, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5336, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5337, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5338, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5339, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5340, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5341, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5342, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, true))},
            {5343, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5344, new MobData(ThreatLevel.Easy, AggroType.Proximity)},
            {5345, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 91-100
            {5346, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true))},
            {5347, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, false, false, true))},
            {5348, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5349, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5350, new MobData(ThreatLevel.Easy, AggroType.Sound)},
            {5351, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5352, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true))},
            {5353, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, false, false, true))},
            {5354, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5355, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(false, false, false, false, false, true))},
            {5356, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 101-110
            {5360, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5361, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5362, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5363, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5364, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5365, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5366, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {5367, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5368, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5369, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5370, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5371, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 111-120
            {5372, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5373, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5374, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5375, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            {5376, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, false, false, true))},
            {5377, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(false, false, false, true))},
            {5378, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5379, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, false, false, true))},
            {5380, new MobData(ThreatLevel.Caution, AggroType.Proximity, true)},
            {5381, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5382, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {5383, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5384, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 121-130
            {5385, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5386, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5387, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, false, false, true))},
            {5388, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5389, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5390, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5391, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, true))},
            {5392, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5393, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5394, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5395, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5396, new MobData(ThreatLevel.Caution, AggroType.Sound, true)},
            {5397, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 131-140
            {5398, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5399, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5400, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5401, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true))},
            {5402, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {5403, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5404, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5405, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5406, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true))},
            {5407, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5408, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5409, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, true))},
            {5410, new MobData(ThreatLevel.Caution, AggroType.Boss, Tuple.Create(false, false, false, false, false, true))},
            // PotD 141-150
            {5411, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5412, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5413, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            {5414, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true)},
            {5415, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, true)},
            {5416, new MobData(ThreatLevel.Easy, AggroType.Sound, true)},
            {5417, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5418, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5419, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5420, new MobData(ThreatLevel.Easy, AggroType.Proximity)},
            {5421, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            {5422, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true))},
            {5423, new MobData(ThreatLevel.Dangerous, AggroType.Sound, Tuple.Create(false, false, false, false, false, true))},
            {5424, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 151-160
            {5429, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true))},
            {5430, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5431, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5432, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5433, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5434, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5435, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5436, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5437, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true))},
            {5438, new MobData(ThreatLevel.Caution, AggroType.Boss)},
            // PotD 161-170
            {5439, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5440, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5441, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true))},
            {5442, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true))},
            {5443, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true))},
            {5444, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5445, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5446, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5447, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            {5448, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true))},
            {5449, new MobData(ThreatLevel.Easy, AggroType.Boss)},
            // PotD 171-180
            {5450, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true))},
            {5451, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(false, false, false, true))},
            {5452, new MobData(ThreatLevel.Easy, AggroType.Proximity)},
            {5453, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true))},
            {5454, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true))},
            {5455, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5456, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5457, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true))},
            {5458, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true))},
            {5459, new MobData(ThreatLevel.Vicious, AggroType.Sight, true)},
            {5460, new MobData(ThreatLevel.Dangerous, AggroType.Sight, Tuple.Create(true, true))},
            {5461, new MobData(ThreatLevel.Dangerous, AggroType.Boss)},
            // PotD 181-190
            {5462, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            {5463, new MobData(ThreatLevel.Caution, AggroType.Sound, Tuple.Create(true, true))},
            {5464, new MobData(ThreatLevel.Vicious, AggroType.Sight, Tuple.Create(false, true))},
            {5465, new MobData(ThreatLevel.Dangerous, AggroType.Sound, Tuple.Create(false, true))},
            {5466, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(false, true))},
            {5467, new MobData(ThreatLevel.Dangerous, AggroType.Sound, Tuple.Create(true, true))},
            {5468, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, true))},
            {5469, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true))},
            {5470, new MobData(ThreatLevel.Vicious, AggroType.Proximity, Tuple.Create(false, true))},
            {5471, new MobData(ThreatLevel.Dangerous, AggroType.Boss)},
            // PotD 191-200
            {5472, new MobData(ThreatLevel.Easy, AggroType.Sound)},
            {5473, new MobData(ThreatLevel.Dangerous, AggroType.Sight)},
            {5474, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, true))},
            {5475, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true))},
            {5479, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {5480, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            {2566, new MobData(ThreatLevel.Caution, AggroType.Proximity)}
        };

        private static readonly Dictionary<uint, ClassData> _jobs = new Dictionary<uint, ClassData>
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

    public static class Extensions
    {
        public static unsafe Vulnerabilities Get(this ITuple vulnTuple)
        {
            bool stun, heavy, slow, sleep, bind, undead;
            switch (vulnTuple)
            {
                case Tuple<bool> tuple when tuple.GetType() == typeof(Tuple<bool>):
                    stun = tuple.Item1;
                    break;
                case Tuple<bool, bool> tuple when tuple.GetType() == typeof(Tuple<bool, bool>):
                    (stun, heavy) = tuple;
                    break;
                case Tuple<bool, bool, bool> tuple when tuple.GetType() == typeof(Tuple<bool, bool, bool>):
                    (stun, heavy, slow) = tuple;
                    break;
                case Tuple<bool, bool, bool, bool> tuple when tuple.GetType() == typeof(Tuple<bool, bool, bool, bool>):
                    (stun, heavy, slow, sleep) = tuple;
                    break;
                case Tuple<bool, bool, bool, bool, bool> tuple when tuple.GetType() == typeof(Tuple<bool, bool, bool, bool, bool>):
                    (stun, heavy, slow, sleep, bind) = tuple;
                    break;
                case Tuple<bool, bool, bool, bool, bool, bool> tuple when tuple.GetType() == typeof(Tuple<bool, bool, bool, bool, bool, bool>):
                    (stun, heavy, slow, sleep, bind, undead) = tuple;
                    break;
            }
            return (Vulnerabilities)(*(byte*)&stun + (*(byte*)&heavy << 1) + (*(byte*)&slow << 2) + (*(byte*)&sleep << 3) + (*(byte*)&bind << 4) + (*(byte*)&undead << 5));
        }
    }
}
