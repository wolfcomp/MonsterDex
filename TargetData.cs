using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;

namespace DeepDungeonDex
{
	public class TargetData
	{
		public uint NameID { get; set; }
		public SeString Name { get; set; }
		public TargetData IsValidTarget(GameObject target)
        {
            if (!(target is BattleNpc bnpc)) return null;
            Name = bnpc.Name;
            NameID = bnpc.NameId;
            return this;
        }

        public TargetData SetName(uint id)
        {
            NameID = id;
            Name = Plugin.GameData.GetExcelSheet<Lumina.Excel.GeneratedSheets.BNpcName>()?.GetRow(id)?.Singular.ToString() ?? "";
            return this;
        }
	}
}