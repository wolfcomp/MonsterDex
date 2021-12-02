using System;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Resolvers;
using Lumina.Excel.GeneratedSheets;

namespace DeepDungeonDex
{
	public class TargetData
	{
		public static uint NameID { get; set; }
		public static SeString Name { get; set; }
		public bool IsValidTarget(GameObject target)
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