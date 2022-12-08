using System;
using Dalamud.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

namespace DeepDungeonDex.Hooks
{
    public unsafe class AddonAgent : IDisposable
    {
        private FFXIVClientStructs.FFXIV.Client.System.Framework.Framework* _structsFramework;
        private Framework _framework;

        public AddonAgent(Framework framework)
        {
            _structsFramework = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
            _framework = framework;
            framework.Update += OnUpdate;
        }

        private void OnUpdate(Framework framework)
        {
            var agentDeepDungeonMap = _structsFramework->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.DeepDungeonMap);
            if (agentDeepDungeonMap == null || agentDeepDungeonMap->IsAgentActive())
                return;
                
            var check = (ushort*)((long*)agentDeepDungeonMap)[0x66];
            var deepDungeonMap = (byte*)agentDeepDungeonMap;
            if (check[1] != 0x8003 && deepDungeonMap[0x80C] != 9)
                return;
            
            var floor = deepDungeonMap[0x18E8];

            //use ClientState.TerritoryType to get the current instance, 0xF1E7 is PotD and 0xE3ED is HoH

            var agentDeepDungeonStatus = _structsFramework->UIModule->GetAgentModule()->GetAgentByInternalId(AgentId.DeepDungeonStatus);
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
        }
    }
}
