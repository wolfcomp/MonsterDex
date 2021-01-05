using System;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.ClientState.Actors.Types.NonPlayer;

namespace DeepDungeonDex
{
	public class TargetData
	{
		public static int NameID { get; set; }
		public static string Name { get; set; }

		public bool IsValidTarget(Actor target)
		{
			if (target is BattleNpc bnpc)
			{
				Name = bnpc.Name;
				NameID = bnpc.NameId;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}