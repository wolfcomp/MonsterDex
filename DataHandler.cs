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
        Unspecified,
        Easy,
        Caution,
        Dangerous,
        Vicious
    }

    public enum AggroType
    {
        Unspecified,
        Sight,
        Sound,
        Proximity,
        Boss
    }

    public struct MobData
    {
        public Vulnerabilities Vuln { get; set; }
        public string MobNotes { get; set; }
        public ThreatLevel Threat { get; set; }
        public AggroType Aggro { get; set; }

        #region Constructors
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="mobNotes">Notes for the mob</param>
        /// <param name="vuln">The vulnerabilities for the mob</param>
        public MobData(ThreatLevel level, AggroType aggro, Vulnerabilities vuln, string mobNotes)
        {
            Threat = level;
            Aggro = aggro;
            MobNotes = mobNotes;
            Vuln = vuln;
        }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="mobNotes">Notes for the mob</param>
        public MobData(ThreatLevel level, AggroType aggro, string mobNotes) : this(level, aggro, Vulnerabilities.None, mobNotes) { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        public MobData(ThreatLevel level, AggroType aggro) : this(level, aggro, "") { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="stun">Can the mob be stunned</param>
        public MobData(ThreatLevel level, AggroType aggro, bool stun) : this(level, aggro, stun, "") { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="vuln">The vulnerabilities for the mob</param>
        public MobData(ThreatLevel level, AggroType aggro, Vulnerabilities vuln) : this(level, aggro, vuln, "") { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="vulnTuple">Can the mob be afflicted with stun, heavy, slow, sleep and bind and is the mob undead. (stun, heavy, slow, sleep, bind, undead)</param>
        public MobData(ThreatLevel level, AggroType aggro, ITuple vulnTuple) : this(level, aggro, vulnTuple, "") { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="mobNotes">Notes for the mob</param>
        /// <param name="stun">Can the mob be stunned</param>
        public MobData(ThreatLevel level, AggroType aggro, bool stun, string mobNotes) : this(level, aggro, Tuple.Create(stun), mobNotes) { }
        /// <summary>
        /// Mob data struct init
        /// </summary>
        /// <param name="level">The threat level</param>
        /// <param name="aggro">The aggro type</param>
        /// <param name="vulnTuple">Can the mob be afflicted with stun, heavy, slow, sleep and bind and is the mob undead. (stun, heavy, slow, sleep, bind, undead)</param>
        /// <param name="mobNotes">Notes for the mob</param>
        public MobData(ThreatLevel level, AggroType aggro, ITuple vulnTuple, string mobNotes) : this(level, aggro, vulnTuple.Get(), mobNotes) { }
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
            { 7262, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Auto inflicts Heavy debuff")},
            { 7263, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Auto applies Physical Vuln Up every 10s")},
            { 7264, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "AoE applies Paralysis")},
            { 7265, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, true, "Triple auto inflicts Bleed")},
            { 7266, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Untelegraphed Sleep followed by AoE")},
            { 7267, new MobData(ThreatLevel.Easy, AggroType.Sight, "AoE applies Bleed")},
            { 7268, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Gaze")},
            { 7269, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            { 7270, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "AoE inflicts knockback")},
            { 7271, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Conal AoE inflicts Bleed\nCircle AoE inflicts knockback")},
            { 7272, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Unavoidable tankbuster-like \"Jaws\"")},
            { 7273, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Untelegraphed buster inflicts Bleed and knockback")},
            { 7274, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
			// HoH floors 11-19
            { 7275, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7276, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            { 7277, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7278, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7279, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Lite buster \"Scissor Run\" followed by AoE")},
            { 7280, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            { 7281, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Gaze inflicts Seduce, followed by large AoE that inflicts Minimum")},
            { 7282, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7283, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7284, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            { 7285, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Buster and triple auto")},
            { 7286, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Room wide ENRAGE")},
            { 7287, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
			// HoH floors 21-29
            { 7288, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true), "Gaze inflicts Blind")},
            { 7289, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true), "Cures self and allies")},
            { 7290, new MobData(ThreatLevel.Easy, AggroType.Proximity, true, "Casts AoEs with knockback unaggroed\nLine AoE inflicts Bleed")},
            { 7291, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true), "Buffs own damage")},
            { 7292, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Untelegraphed conal AoE with knockback, buster")},
            { 7293, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            { 7294, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true))},
            { 7295, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Draw-in followed by cleave")},
            { 7296, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Gaze")},
            { 7297, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true), "Line AoE inflicts Bleed")},
            { 7298, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true), "Cross AoE inflicts Suppuration")},
            { 7299, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Large AoE inflicts Paralysis")},
            { 7300, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Circle AoE inflicts Suppuration")},
            //HoH floors 31-39
            { 7301, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7302, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Casts AoEs unaggroed")},
            { 7303, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Double auto inflicts Bleed\nLow health ENRAGE")},
            { 7304, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Low health ENRAGE")},
            { 7305, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Line AoE inflicts Bleed\nLow health ENRAGE")},
            { 7306, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Cleaves every other auto")},
            { 7307, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7308, new MobData(ThreatLevel.Easy, AggroType.Sight, "Weak stack attack")},
            { 7309, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7310, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Extremely large AoE")},
            { 7311, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Line AoE inflicts Bleed\nLow health ENRAGE")},
            { 7312, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Frontal cleave without cast or telegraph")},
            { 7313, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Gaze inflicts Otter")},
			// HoH floors 41-49
            { 7314, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Casts AoEs unaggroed")},
            { 7315, new MobData(ThreatLevel.Easy, AggroType.Proximity, true)},
            { 7316, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7317, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7318, new MobData(ThreatLevel.Caution, AggroType.Sight, "Large line AoE\nEventual ENRAGE")},
            { 7319, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Casts AoEs unaggroed")},
            { 7320, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Purple: double auto")},
            { 7321, new MobData(ThreatLevel.Easy, AggroType.Sight, "Large cone AoE")},
            { 7322, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7323, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Green: Casts AoEs unaggroed")},
            { 7324, new MobData(ThreatLevel.Caution, AggroType.Sight, "Very wide line AoE")},
            { 7325, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7326, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "Eventual ENRAGE")},
			//HoH floors 51-59
            { 7327, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "Autos inflict stacking vuln up")},
            { 7328, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "Buster inflicts Bleed")},
            { 7329, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Buffs own damage")},
            { 7330, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Eventual instant ENRAGE")},
            { 7331, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Cone AoE inflicts Bleed")},
            { 7332, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Exclusively fatal line AoEs")},
            { 7333, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7334, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7335, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Draw-in attack")},
            { 7336, new MobData(ThreatLevel.Caution, AggroType.Sight, "Instant AoEs on targeted player unaggroed")},
            { 7337, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Conal gaze, very quick low health ENRAGE")},
            { 7338, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            { 7339, new MobData(ThreatLevel.Easy, AggroType.Sight)},
			// HoH floors 61-69
            { 7340, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "Inflicts stacking Poison that lasts 30s")},
            { 7341, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Inflicts stacking vuln up")},
            { 7342, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7343, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Fast alternating line AoEs that inflict Paralysis")},
            { 7344, new MobData(ThreatLevel.Easy, AggroType.Sound, true, "Caster, double auto")},
            { 7345, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Conal AoE inflicts Paralysis")},
            { 7346, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Cleave and potent Poison")},
            { 7347, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Large doughnut AoE, gaze attack inflicts Fear")},
            { 7348, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Large circular AoE inflicts Bleed")},
            { 7349, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Buffs own or ally's defense")},
            { 7350, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "AoE inflicts numerous debuffs at once")},
            { 7351, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
			// HoH floors 71-79
            { 7352, new MobData(ThreatLevel.Easy, AggroType.Sight, true)},
            { 7353, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Casts large AoE unaggroed\nExtremely large circular AoE")},
            { 7354, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Untelegraphed knockback on rear")},
            { 7355, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Double auto inflicts Bleed")},
            { 7356, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Casts AoEs unaggroed that inflict Deep Freeze")},
            { 7357, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Casts room wide AoEs unaggroed\nLarge conal draw-in attack followed by heavy damage")},
            { 7358, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Buffs own damage")},
            { 7359, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, true, "Haste, eventual ENRAGE")},
            { 7360, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Very large AoEs")},
            { 7361, new MobData(ThreatLevel.Caution, AggroType.Sight, "Draw-in attack, extremely large AoE, eventual ENRAGE")},
            { 7362, new MobData(ThreatLevel.Caution, AggroType.Sight, "Extremely large conal AoE, gaze inflicts Fear")},
            { 7363, new MobData(ThreatLevel.Easy, AggroType.Sight)},
            { 7364, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Double auto and very large AoE")},
			// HoH floors 81-89
            { 7365, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, "Ram's Voice - get out\nDragon's Voice - get in\nTelegraphed cleaves")},
            { 7366, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Buffs own damage unaggroed\nLarge AoE unaggroed that inflicts vuln up and stacks")},
            { 7367, new MobData(ThreatLevel.Easy, AggroType.Sight, "Charges on aggro")},
            { 7368, new MobData(ThreatLevel.Caution, AggroType.Sight, "Untelegraphed conal AoE on random player, gaze attack")},
            { 7369, new MobData(ThreatLevel.Easy, AggroType.Sight, "Casts AoEs unaggroed")},
            { 7370, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Double autos, very strong rear cleave if behind")},
            { 7371, new MobData(ThreatLevel.Caution, AggroType.Sight, "Alternates line and circle AoEs untelegraphed")},
            { 7372, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Buffs own damage and double autos")},
            { 7373, new MobData(ThreatLevel.Caution, AggroType.Sight, "Draw-in attack, tons of bleed, and a stacking poison")},
            { 7374, new MobData(ThreatLevel.Easy, AggroType.Proximity, "Large doughnut AoE")},
            { 7375, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Cone AoE, circle AoE, party wide damage")},
            { 7376, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Charges, buffs own damage, double autos, electricity Bleed")},
            { 7377, new MobData(ThreatLevel.Caution, AggroType.Sight, "Charges, buffs own damage, untelegraphed buster \"Ripper Claw\"")},
			// HoH floors 91-99
            { 7378, new MobData(ThreatLevel.Vicious, AggroType.Sight, true, "WAR: Triple knockback with heavy damage\nBuffs own attack\nExtremely high damage cleave with knockback")},
            { 7379, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "MNK: Haste buff, short invuln")},
            { 7380, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "WHM: double autos\n\"Stone\" can be line of sighted")},
            { 7381, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, "Cleave\nLarge line AoE that can be line of sighted")},
            { 7382, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "\"Charybdis\" AoE that leaves tornadoes on random players")},
            { 7383, new MobData(ThreatLevel.Caution, AggroType.Sight, true)},
            { 7384, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Casts targeted AoEs unaggroed, buffs own defense")},
            { 7385, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Targeted AoEs, cleaves")},
            { 7386, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Extremely quick line AoE \"Death's Door\" that instantly kills")},
            { 7387, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Deals heavy damage to random players")},
            { 7388, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Charges\nUntelegraphed line AoE \"Swipe\"\nUntelegraphed wide circle AoE \"Swing\"")},
            { 7389, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Repeatedly cleaves for high damage, lifesteal, buffs own damage, three stacks of damage up casts ENRAGE \"Black Nebula\"")},
            { 7390, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Rapid double autos and untelegraphed line AoE \"Quasar\"")},
            { 7391, new MobData(ThreatLevel.Caution, AggroType.Sight, "Double autos, cone AoE inflicts Sleep")},
            { 7584, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Permanent stacking damage buff\nMassive enrage on random player\"Allagan Meteor\"\nGaze attack")},
            // HoH bosses and misc.
            { 7392, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Floors 1-30: Bronze chests only\nHigh damage autos and instant kill AoE\n\"Malice\" can be interrupted with silence/stun/knockback/witching/\ninterject")},
            { 7393, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Floors 31-60: Silver chests only\nHigh damage autos and instant kill AoE\n\"Malice\" can be interrupted with silence/stun/interject")},
            { 7394, new MobData(ThreatLevel.Caution, AggroType.Proximity, "Floors 61+: Gold chests only\nHigh damage autos and instant kill AoE\n\"Malice\" can only be interrupted with interject\nCANNOT STUN")},
            { 7478, new MobData(ThreatLevel.Caution, AggroType.Boss, "Summons lightning clouds that inflict stacking vuln up when they explode\nBoss does proximity AoE under itself that knocks players into the air\nGet knocked into a cloud to dispel it and avoid vuln\nHalf-room wide AoE")},
            { 7480, new MobData(ThreatLevel.Caution, AggroType.Boss, "Goes to center of arena and casts knockback to wall (cannot be knockback invulned)\nFollows immediately with a half-room wide AoE")},
            { 7481, new MobData(ThreatLevel.Caution, AggroType.Boss, "Summons butterflies on edges of arena\nDoes gaze mechanic that inflicts Fear\nButterflies explode untelegraphed")},
            { 7483, new MobData(ThreatLevel.Caution, AggroType.Boss, "Summons clouds on edge of arena\nDoes to-wall knockback that ignores knockback invuln (look for safe spot in clouds!)\nFollows immediately with targeted line AoE")},
            { 7485, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) Untelegraphed swipe\n2) Untelegraphed line AoE on random player\n3) Gaze mechanic that inflicts Fear\n4) Summons pulsating bombs over arena and does a proximity AoE\n5) Repeats after bombs explode for the last time")},
            { 7487, new MobData(ThreatLevel.Caution, AggroType.Boss, "Summons staffs that do various AoEs\nStaffs then do line AoEs targeting players\nRoom wide AoE")},
            { 7489, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) Untelegraphed frontal cleave\n2) Targets random player with \"Innerspace\" puddle (standing in puddle inflicts Minimum)\n3) Targets random player with \"Hound out of Hell\"\n4) Targeted player must stand in puddle to dodge \"Hound out of Hell\" and \"Devour\" (will instant-kill if not in puddle and give the boss a stack of damage up)\n5) Repeat")},
            { 7490, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) Summons balls of ice\n2) Summons icicle that pierces through room, detonating any ice balls it hits\n3) \"Lunar Cry\" detonates remaining ice balls\n4) Exploding ice balls inflict Deep Freeze if they hit a player\n5) Boss jumps to random player, instantly killing if player is frozen (light damage otherwise)")},
            { 7493, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) Heavy room wide damage \"Ancient Quaga\"\n2) Pulsing rocks appear over arena, causing moderate damage and Heavy debuff if player is hit by one\n3) \"Meteor Impact\" summons proximity AoE at boss's current location\n4) Line AoE \"Aura Cannon\"\n5) Targeted circle AoE \"Burning Rave\"\n6) Point-blank circle AoE \"Knuckle Press\"\n7) Repeat")},
            { 7610, new MobData(ThreatLevel.Easy, AggroType.Proximity, "Does not interact, wide stun and immediately dies when attacked")},

            //PotD data
            //PotD 1-10
            { 4975, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true), "Casts Haste on itself") },
            { 4976, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 4977, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 4978, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, false, true)) },
            { 4979, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, true, true)) },
            { 4980, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, false, true), "Inflicts Poison") },
            { 4981, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "High damage \"Final Sting\"") },
            { 4982, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, false, false, true), "Inflicts vulnerability up") },
            { 4983, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, false, true)) },
            { 4984, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 4985, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, false, true), "Mini buster \"Rhino Charge\"") },
            { 4986, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) \"Whipcrack\" - light tankbuster\n2) \"Stormwind\" - conal AOE\n3) \"Bombination\" - circular AOE on boss inflicts Slow\n4) \"Lumisphere\" - targeted AOE on random player\n5) \"Aeroblast\" - room wide AOE inflicts Bleed") },
            //PotD 11-20
            { 4987, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, false, true)) },
            { 4988, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Inflicts poison") },
            { 4989, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true), "\"Sticky Tongue\" does not stun if facing towards") },
            { 4990, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "Eventual ENRAGE") },
            { 4991, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true, false, true, true)) },
            { 4992, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true, false, true, true)) },
            { 4993, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, false, true), "Area of effect Slow") },
            { 4994, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, true, true)) },
            { 4995, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true)) },
            { 4996, new MobData(ThreatLevel.Easy, AggroType.Proximity, true, "Buffs own damage") },
            { 4997, new MobData(ThreatLevel.Dangerous, AggroType.Sight, Tuple.Create(true, false, false, true), "Gaze attack inflicts Petrify, \"Devour\" instantly kills players inflicted with Toad") },
            { 4998, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, true, false, false, true)) },
            { 4999, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) \"Bloody Caress\" - high damage cleave\n2) Two telegraphed AOEs and a room wide AOE\n3) Summons two hornets that must be killed before they \"Final Sting\"\n4) \"Rotten Stench\" - high damage line AOE") },
            //PotD 21-30
            { 5000, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5001, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5002, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, true, false, true, true)) },
            { 5003, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true)) },
            { 5004, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, true)) },
            { 5005, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5006, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, true, true)) },
            { 5007, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, true)) },
            { 5008, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5009, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true, false, false, true)) },
            { 5010, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true, false, true), "Untelegraphed AOE does moderate damage and knockback") },
            { 5011, new MobData(ThreatLevel.Caution, AggroType.Sound, Tuple.Create(true, true, false, true, true), "\"Chirp\" inflicts Sleep for 15s") },
            { 5012, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) Spread out fire and ice AOEs and don't drop them in center because: \n2) Get inside boss's hit box for \"Fear Itself\" - will inflict high damage and Terror if not avoided") },
            //PotD 31-40
            { 5013, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5014, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5015, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5016, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, false, false, true)) },
            { 5017, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5018, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5019, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5020, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true)) },
            { 5021, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5022, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "\"Dark Mist\" inflicts Terror") },
            { 5023, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5024, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true, false, true)) },
            { 5025, new MobData(ThreatLevel.Easy, AggroType.Boss, Tuple.Create(false, false, false, false, false, true), "1) Summons four lingering AoEs\n2) Summons two adds -- they must be killed before boss casts \"Scream\", adds will target player with high damage AoEs if not dead") },
            // PotD 41-50
            { 5026, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5027, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5028, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5029, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5030, new MobData(ThreatLevel.Caution, AggroType.Sound, Tuple.Create(true, false, false, true), "Inflicts Paralysis") },
            { 5031, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5032, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Inflicts Paralysis") },
            { 5033, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5034, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5035, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5036, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5037, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(false, false, false, false, false, true)) },
            { 5038, new MobData(ThreatLevel.Caution, AggroType.Boss, "FOLLOW MECHANICS -- failed mechanics power up an unavoidable room AoE\nBoss will occasionally inflict Disease which slows\n1) \"In Health\" -- can be room wide AoE with safe spot on boss or targeted AoE under boss \n2) \"Cold Feet\" -- Gaze") },
            // PotD special NPCs and misc.
            { 5039, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5040, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5041, new MobData(ThreatLevel.Easy, AggroType.Proximity, "Does not interact, wide stun and immediately dies when attacked") },
            { 5046, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5047, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5048, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5049, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5050, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5051, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5052, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5053, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5283, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5284, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5285, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5286, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5287, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5288, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5289, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5290, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5291, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5292, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5293, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5294, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5295, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5296, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5297, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            { 5298, new MobData(ThreatLevel.Caution, AggroType.Sight, "Immune to Pomander of Witching") },
            // PotD 51-60
            { 5299, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true), "Gaze inflicts Paralysis") },
            { 5300, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5301, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true, false, true)) },
            { 5302, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5303, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5304, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5305, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5306, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5307, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5308, new MobData(ThreatLevel.Easy, AggroType.Proximity, true, "Gaze inflicts Blind and does high damage") },
            { 5309, new MobData(ThreatLevel.Caution, AggroType.Boss, "Drops large puddle AoEs that inflict Bleed if stood in\n\"Valfodr\" -- targeted unavoidable line AoE centered on player that causes strong knockback, avoid AoEs surrounding outer edge") },
            // PotD 61-70
            { 5311, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5312, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5313, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5314, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true)) },
            { 5315, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5316, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5317, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5318, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5319, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5320, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true)) },
            { 5321, new MobData(ThreatLevel.Caution, AggroType.Boss, "\"Douse\" -- lingering ground AoE that inflicts Bleed if stood in and buffs boss with Haste if left in it\nOccasionally casts targeted ground AoEs") },
            // PotD 71-80
            { 5322, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5323, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, false, false, true)) },
            { 5324, new MobData(ThreatLevel.Easy, AggroType.Proximity) },
            { 5325, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5326, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true)) },
            { 5327, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5328, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5329, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true)) },
            { 5330, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5331, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5332, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5333, new MobData(ThreatLevel.Caution, AggroType.Boss, "\"Charybdis\" -- lingering ground tornadoes that cause high damage if sucked into\nBoss will run to edge of arena and cast \"Trounce\" - wide conal AoE\nAt 17% casts \"Ecliptic Meteor\" - HIGH DAMAGE room wide with long cast that deals 80% of total health damage") },
            // PotD 81-90
            { 5334, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, false, false, true), "Casts wide \"Self Destruct\" if not killed in time") },
            { 5335, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5336, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5337, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5338, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5339, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5340, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5341, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5342, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, true)) },
            { 5343, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5344, new MobData(ThreatLevel.Easy, AggroType.Proximity) },
            { 5345, new MobData(ThreatLevel.Caution, AggroType.Boss, "Casts large AoEs\nSummons \"Grey Bomb\" - must be killed before it does high room wide damage\nBegins long cast \"Massive Burst\" and summons \"Giddy Bomb\" that must be knocked towards the boss to interrupt cast") },
            // PotD 91-100
            { 5346, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true)) },
            { 5347, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, false, false, true)) },
            { 5348, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5349, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5350, new MobData(ThreatLevel.Easy, AggroType.Sound) },
            { 5351, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5352, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true)) },
            { 5353, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, false, false, true)) },
            { 5354, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5355, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(false, false, false, false, false, true)) },
            { 5356, new MobData(ThreatLevel.Caution, AggroType.Boss, "Summons adds and does large targeted AoEs -- adds are vulnerable to Pomander of Resolution's attacks") },
            // PotD 101-110
            { 5360, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5361, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5362, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5363, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5364, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, true), "Floors 101-110: \nNothing notable (ignore threat level)\nFloors 191-200: \nDouble autos") },
            { 5365, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Inflicts Poison") },
            { 5366, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "High damage \"Final Sting\"") },
            { 5367, new MobData(ThreatLevel.Easy, AggroType.Sound, true, "Inflicts vulnerability up") },
            { 5368, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5369, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5370, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Mini buster \"Rhino Charge\"") },
            { 5371, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) \"Whipcrack\" - light tankbuster\n2) \"Stormwind\" - conal AOE\n3) \"Bombination\" - circular AOE on boss inflicts Slow\n4) \"Lumisphere\" - targeted AOE on random player\n5) \"Aeroblast\" - room wide AOE inflicts Bleed") },
            // PotD 111-120
            { 5372, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5373, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5374, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, false, false, true), "\"Sticky Tongue\" draw-in and stun attack if not facing, followed by \"Labored Leap\" AoE centered on enemy") },
            { 5375, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "Eventual ENRAGE") },
            { 5376, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, false, false, true)) },
            { 5377, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(false, false, false, true), "Casts invuln buff on itself") },
            { 5378, new MobData(ThreatLevel.Easy, AggroType.Proximity, true, "Area of effect Slow") },
            { 5379, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, false, false, true)) },
            { 5380, new MobData(ThreatLevel.Caution, AggroType.Proximity, true, "Will inflict Sleep before casting \"Bad Breath\"") },
            { 5381, new MobData(ThreatLevel.Easy, AggroType.Proximity, true, "Buffs own damage") },
            { 5382, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Gaze attack inflicts Petrify, \"Regorge\" inflicts Poison\nWill one-shot kill anyone inflicted with Toad") },
            { 5383, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5384, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) \"Bloody Caress\" - high damage cleave\n2) Two telegraphed AOEs and a room wide AOE\n3) Summons two hornets that must be killed before they \"Final Sting\"\n4) \"Rotten Stench\" - high damage line AOE") },
            // PotD 121-130
            { 5385, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5386, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5387, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(false, false, false, true)) },
            { 5388, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5389, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, true), "Double autos") },
            { 5390, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5391, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, false, false, true)) },
            { 5392, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5393, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5394, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5395, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, false, false, true), "\"11-Tonze Swing\" - point-blank untelegraphed AoE that does high damage and knockback") },
            { 5396, new MobData(ThreatLevel.Caution, AggroType.Sound, true, "\"Chirp\" inflicts Sleep for 15s") },
            { 5397, new MobData(ThreatLevel.Caution, AggroType.Boss, "1) Spread out fire and ice AOEs and don't drop them in center because: \n2) Get inside boss's hit box for fast cast \"Fear Itself\" - will inflict high damage and Terror if not avoided") },
            // PotD 131-140
            { 5398, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5399, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5400, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5401, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true)) },
            { 5402, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "Untelegraphed conal AoE \"Level 5 Petrify\" inflicts Petrify") },
            { 5403, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true)) },
            { 5404, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5405, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, false, false, true)) },
            { 5406, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, false, false, true), "Casts targeted AoE that inflicts Bleed") },
            { 5407, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5408, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5409, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, true, false, true), "Floors 131-140: \nNothing notable (ignore threat level)\nFloors 191-200: \nDouble autos") },
            { 5410, new MobData(ThreatLevel.Caution, AggroType.Boss, Tuple.Create(false, false, false, false, false, true), "1) Summons four lingering AoEs\n2) Summons two adds -- they must be killed before boss casts \"Scream\", adds will target player with high damage AoEs if not dead") },
            // PotD 141-150
            { 5411, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5412, new MobData(ThreatLevel.Easy, AggroType.Sight) },
            { 5413, new MobData(ThreatLevel.Caution, AggroType.Sight, true, "\"Charybdis\" - semi-enrage that drops party health to 1%") },
            { 5414, new MobData(ThreatLevel.Dangerous, AggroType.Sight, true, "Very high damage, inflicts Poison") },
            { 5415, new MobData(ThreatLevel.Dangerous, AggroType.Proximity, true, "Floors 141-150: \nNothing notable (ignore threat level)\nFloors 191-200: \nCasts large doughnut AoE \"Death Spiral\" that deals heavy damage\nHas soft enrage of a strong damage buff") },
            { 5416, new MobData(ThreatLevel.Easy, AggroType.Sound, true) },
            { 5417, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5418, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5419, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5420, new MobData(ThreatLevel.Easy, AggroType.Proximity, "Casts Gaze \"Evil Eye\"") },
            { 5421, new MobData(ThreatLevel.Easy, AggroType.Sight, "Buffs own damage, untelegraphed high damage \"Ripper Claw\" - can be avoided by walking behind") },
            { 5422, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true), "High health and very large AoE \"Scream\"") },
            { 5423, new MobData(ThreatLevel.Dangerous, AggroType.Sound, Tuple.Create(false, false, false, false, false, true), "Very high damage for the floors it appears on") },
            { 5424, new MobData(ThreatLevel.Caution, AggroType.Boss, "Summons adds\n\"Fanatic Zombie\" will grab player and root in place until killed\n\"Fanatic Succubus\" will heal boss if it reaches it") },
            // PotD 151-160
            { 5429, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true, false, true, true), "Double autos\nGaze inflicts Paralysis") },
            { 5430, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "Inflicts Vuln Up debuff") },
            { 5431, new MobData(ThreatLevel.Easy, AggroType.Sight, true, "\"Ice Spikes\" reflects damage\n\"Void Blizzard\" inflicts Slow") },
            { 5432, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5433, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, false, true), "Double autos that lifesteal") },
            { 5434, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5435, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5436, new MobData(ThreatLevel.Easy, AggroType.Proximity, true, "Double autos") },
            { 5437, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true, false, false, true), "Double autos\nGaze inflicts heavy damage and Blind") },
            { 5438, new MobData(ThreatLevel.Caution, AggroType.Boss, "Drops lingering AoEs that cause heavy Bleed if stood in\n\"Valfodr\" -- targeted unavoidable line AoE centered on player that causes strong knockback, avoid AoEs surrounding outer edge") },
            // PotD 161-170
            { 5439, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5440, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true)) },
            { 5441, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true, false, true, true)) },
            { 5442, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true, false, true), "Double autos") },
            { 5443, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true, false, true, true), "Double autos") },
            { 5444, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5445, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5446, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5447, new MobData(ThreatLevel.Easy, AggroType.Sight, true) },
            { 5448, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true, false, true, true)) },
            { 5449, new MobData(ThreatLevel.Easy, AggroType.Boss, "\"Douse\" -- lingering ground AoE that inflicts Bleed if stood in and buffs boss with Haste and Damage Up if left in it\nOccasionally inflicts Heavy and casts targeted ground AoEs ") },
            // PotD 171-180
            { 5450, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true, false, true, true), "Double autos") },
            { 5451, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(false, false, false, true), "Has semi-enrage around 30s in combat") },
            { 5452, new MobData(ThreatLevel.Easy, AggroType.Proximity, "\"Revelation\" inflicts Confusion\n\"Tropical Wind\" gives enemy a large Haste and damage buff") },
            { 5453, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true, false, true), "\"Glower\" - untelegraphed line AoE\n\"100-Tonze Swing\" - untelegraphed point-blank AoE") },
            { 5454, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, true, false, true), "Buffs own damage and inflicts Physical Vuln Up with AoE damage out of combat") },
            { 5455, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5456, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5457, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(true, true, false, true, true)) },
            { 5458, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(true, true, false, true, true), "Double autos") },
            { 5459, new MobData(ThreatLevel.Vicious, AggroType.Sight, true, "Cleave inflicts Bleed\n\"Flying Frenzy\" targets a player and does heavy damage, Vuln Down, and stuns") },
            { 5460, new MobData(ThreatLevel.Dangerous, AggroType.Sight, Tuple.Create(true, true, false, true, true), "Cleave does heavy damage and inflicts potent Bleed") },
            { 5461, new MobData(ThreatLevel.Dangerous, AggroType.Boss, "\"Charybdis\" -- lingering ground tornadoes cast twice in a row that cause high damage if sucked into\nBoss will run to top or bottom of arena and cast \"Trounce\" - wide conal AoE\nAt 15% casts FAST CAST \"Ecliptic Meteor\" - HIGH DAMAGE room wide with long cast that deals 80% of total health damage every 9 seconds") },
            // PotD 181-190
            { 5462, new MobData(ThreatLevel.Easy, AggroType.Sight, Tuple.Create(true, true, false, true, true)) },
            { 5463, new MobData(ThreatLevel.Caution, AggroType.Sound, Tuple.Create(true, true, false, false, true), "Double autos") },
            { 5464, new MobData(ThreatLevel.Vicious, AggroType.Sight, Tuple.Create(false, true, false, false, true), "Instant AoE that inflicts heavy Bleed") },
            { 5465, new MobData(ThreatLevel.Dangerous, AggroType.Sound, Tuple.Create(false, true, false, false, true), "Instant AoE on pull, double autos\nAt 30 seconds will cast semi-enrage") },
            { 5466, new MobData(ThreatLevel.Caution, AggroType.Sight, Tuple.Create(false, true, false, false, true), "Sucks in player and does heavy damage\n\"Tail Screw\" does damage and inflicts Slow") },
            { 5467, new MobData(ThreatLevel.Dangerous, AggroType.Sound, Tuple.Create(true, true, false, false, true), "Instant AoE burst does heavy damage and inflicts Slow\nInstant cone inflicts Poison") },
            { 5468, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, true, false, false, true)) },
            { 5469, new MobData(ThreatLevel.Easy, AggroType.Sound, Tuple.Create(true, true, false, false, true)) },
            { 5470, new MobData(ThreatLevel.Vicious, AggroType.Proximity, Tuple.Create(false, true, false, true, true), "If familiar with chimera mechanics can be engaged\n\"The Dragon's Voice\" - be inside hit box\n\"The Ram's Voice\" - be outside of melee range") },
            { 5471, new MobData(ThreatLevel.Dangerous, AggroType.Boss, "Kill blue bomb when it appears\nPush red bomb into boss during \"Massive Burst\" cast, will wipe party if not stunned\nBoss has cleave that does heavy damage") },
            // PotD 191-200
            { 5472, new MobData(ThreatLevel.Easy, AggroType.Sound) },
            { 5473, new MobData(ThreatLevel.Dangerous, AggroType.Sight, "Casts untelegraphed cone \"Level 5 Death\"") },
            { 5474, new MobData(ThreatLevel.Easy, AggroType.Proximity, Tuple.Create(false, true)) },
            { 5475, new MobData(ThreatLevel.Caution, AggroType.Proximity, Tuple.Create(true, false, false, false, false, true), "Double auto") },
            { 5479, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 5480, new MobData(ThreatLevel.Easy, AggroType.Proximity, true) },
            { 2566, new MobData(ThreatLevel.Caution, AggroType.Proximity, "High damage autos and instant kill AoE\n\"Infatuation\" can only be interrupted with interject") },
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
        public static Vulnerabilities Get(this ITuple vulnTuple)
        {
            unsafe
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
}
