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
        private readonly Framework _framework;
        private readonly Condition _condition;
        private readonly ClientState _state;
        private EventFramework* _structsFramework;
        public byte Floor { get; private set; }
        public bool Disabled { get; private set; }

        public AddonAgent(Framework framework, Condition condition, ClientState state, CommandHandler handler)
        {
            _condition = condition;
            _framework = framework;
            _state = state;
            _framework.Update += OnUpdate;
            handler.AddCommand(new[] { "enablefloor", "efloor", "enablef" }, () =>
            {
                if(!Disabled)
                    return;
                Disabled = false;
                _framework.Update += OnUpdate;
            });
        }

        private void OnUpdate(Framework framework)
        {
            _structsFramework = EventFramework.Instance();
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
            var contentDirector = _structsFramework->GetContentDirector();

            if ((IntPtr)contentDirector == IntPtr.Zero)
                return false;

            var eventHandlerInfo = contentDirector->Director.EventHandlerInfo;

            return (IntPtr)eventHandlerInfo != IntPtr.Zero;
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
            Disabled = true;
        }
    }
}
