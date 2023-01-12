using System;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace DeepDungeonDex.Hooks
{
    public unsafe class AddonAgent : IDisposable
    {
        private readonly EventFramework* _structsFramework;
        private readonly Framework _framework;
        private readonly Condition _condition;
        private readonly ClientState _state;
        public byte Floor { get; private set; }
        public bool Disabled { get; private set; }

        public AddonAgent(Framework framework, Condition condition, ClientState state)
        {
            _structsFramework = EventFramework.Instance();
            _condition = condition;
            _framework = framework;
            _state = state;
            _framework.Update += OnUpdate;
        }

        private void OnUpdate(Framework framework)
        {
            if (!IsInstanceContentSafe())
                return;
            try
            {

                var activeInstance = _structsFramework->GetInstanceContentDeepDungeon();

                if (activeInstance == null)
                    return;

                Floor = activeInstance->Floor;
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "Error trying to fetch InstanceContentDeepDungeon disabling feature.");
                Dispose();
            }
        }

        private bool IsInstanceContentSafe()
        {
            var internalResultValue = (void*)((long*)_structsFramework + 0x158);

            if (internalResultValue == null)
                return false;

            var targetAddress = (void*)((long*)internalResultValue + 0x330);

            return targetAddress != null;
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
            Disabled = true;
        }
    }
}
