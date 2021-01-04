using System;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;

namespace DeepDungeonDex
{
	public class TargetData
	{
		class MobData
		{
			public enum threatLevel;
			public enum aggroType;
			public bool isStunnable;
			public string mobNotes;

			enum threatLevel
            {
				Unspecified,
				Easy,
				Caution,
				Danger,
				DoNotEngage
            }
			


		}

		public static void Mobs()
        {
			var mobs = new Dictionary<int, Mobdata>()
            {
				{7262, new MobData { threatLevel=1, aggroType=1, isStunnable=true, mobNotes="" } }
            }
        }
	}
}