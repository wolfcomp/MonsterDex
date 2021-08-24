#pragma warning disable CA1416
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;

namespace DeepDungeonDex
{
	public class TargetData
	{
		public static int NameID { get; set; }
		public static SeString Name { get; set; }

		public bool IsValidTarget(GameObject target)
		{
			if (target is BattleNpc bnpc)
			{
				Name = bnpc.Name;
				NameID = (int)bnpc.NameId;
				return true;
			}
			else
			{
				return false;
			}
		}
	}
}
#pragma warning restore CA1416