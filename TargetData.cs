using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;

namespace DeepDungeonDex
{
	public class TargetData
	{
		public static uint NameID { get; set; }
		public static SeString Name { get; set; }
		public bool IsValidTarget(GameObject target)
        {
            if (!(target is BattleNpc bnpc)) return false;
            Name = bnpc.Name;
            NameID = bnpc.NameId;
            return true;

        }
	}
}