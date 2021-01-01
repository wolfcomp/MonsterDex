using System;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;

namespace DeepDungeonDex
{
	public class TargetData
	{
		public static int nameID { get; set; }
		public static int dataID { get; set; }
		public static string name { get; set; }

		public void fetchTarget(Actor target)
        {
			if (target is BattleNpc bnpc)
			{
                nameID = bnpc.NameId;
				dataID = bnpc.DataId;
				name = bnpc.Name;
			}
		}


	}
}