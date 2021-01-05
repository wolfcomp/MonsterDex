using System;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;
using System.Collections.Generic;

namespace DeepDungeonDex
{
	 public class DataHandler
	{
		public class MobData
		{
			public bool IsStunnable { get; set; }
			public string MobNotes { get; set; }

			public enum ThreatLevel
            {
				Unspecified,
				Easy,
				Caution,
				Danger,
				DoNotEngage
            }
			public ThreatLevel Threat { get; set; }

			public enum AggroType
            {
				Unspecified,
				Sight,
				Sound,
				Proximity,
				BOSS
            }
			public AggroType Aggro { get; set; }
		}

		public static MobData Mobs(int nameID)
        {
			if (mobs.TryGetValue(nameID, out MobData value)) return value;
			else return null;
        }

		private static readonly Dictionary<int, MobData> mobs = new Dictionary<int, MobData>()
			{
				{ 7262, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Auto inflicts Heavy debuff" } },
				{ 7263, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Auto applies Physical Vuln Up every 10s" } },
				{ 7264, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="AoE applies Paralysis" } },
				{ 7265, new MobData { Threat=MobData.ThreatLevel.Danger, Aggro=MobData.AggroType.Proximity, IsStunnable=true, MobNotes="Triple auto inflicts Bleed" } },
				{ 7266, new MobData { Threat=MobData.ThreatLevel.Caution, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Untelegraphed Sleep followed by AoE" } },
				{ 7267, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="AoE applies Bleed" } },
				{ 7268, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Gaze" } },
				{ 7269, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="" } },
				{ 7270, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="AoE inflicts knockback" } },
				{ 7271, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Conal AoE inflicts Bleed\nCircle AoE inflicts knockback" } },
				{ 7272, new MobData { Threat=MobData.ThreatLevel.Danger, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Unavoidable tankbuster-like \"Jaws\"" } },
				{ 7273, new MobData { Threat=MobData.ThreatLevel.Caution, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="Untelegraphed buster inflicts Bleed and knockback" } },
				{ 7274, new MobData { Threat=MobData.ThreatLevel.Easy, Aggro=MobData.AggroType.Sight, IsStunnable=true, MobNotes="" } }
			};
	}
}