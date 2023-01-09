using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Logging;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

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
            state.Login += Subscribe;
            state.Logout += Unsubscribe;
        }

        private void OnUpdate(Framework framework)
        {
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

        public void Subscribe(object? sender, EventArgs eventArgs)
        {
            Task.Delay(3000).GetAwaiter().GetResult(); // should find a better way to fix logout login crash of E8 ? ? ? ? 41 8B 5F 18.
            _framework.Update += OnUpdate;
        }

        public void Unsubscribe(object? sender, EventArgs eventArgs)
        {
            _framework.Update -= OnUpdate;
        }

        public void Dispose()
        {
            _state.Login -= Subscribe;
            _state.Logout -= Unsubscribe;
            _framework.Update -= OnUpdate;
            Disabled = true;
        }
    }
}
