using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
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
        public byte Floor { get; private set; }
        public bool Disabled { get; private set; }

        public AddonAgent(Framework framework, Condition condition)
        {
            _structsFramework = EventFramework.Instance();
            _condition = condition;
            _framework = framework;
            framework.Update += OnUpdate;
        }

        private void OnUpdate(Framework framework)
        {
            try
            {
                var activeInstance = (InstanceContentDeepDungeon*)_structsFramework->DirectorModule.ActiveInstanceDirector;

                if (activeInstance == null || activeInstance->Unknown80C != 9)
                    return;

                Floor = activeInstance->Floor;
            }
            catch (Exception e)
            {
                PluginLog.Error(e, "Error trying to fetch agent DeepDungeonMap disabling feature.");
                Dispose();
            }
        }

        public void Dispose()
        {
            _framework.Update -= OnUpdate;
            Disabled = true;
        }
    }

    [StructLayout(LayoutKind.Explicit, Size = 0x2308)]
    public unsafe struct InstanceContentDeepDungeon
    {
        [FieldOffset(0x0)] public InstanceContentDirector InstanceContentDirector;
        [FieldOffset(0x80C)] public byte Unknown80C;
        [FieldOffset(0x1870)] public fixed byte PartyLocationThing[0x08 * 4];
        [FieldOffset(0x1890)] public fixed byte ItemInfoThing[0x03 * 16];
        [FieldOffset(0x18C0)] public fixed byte RoomLootThing[0x02 * 16];

        [FieldOffset(0x18E4)] public uint BonusLootItemId;
        [FieldOffset(0x18E8)] public byte Floor;
        [FieldOffset(0x18E9)] public byte ReturnProgress;
        [FieldOffset(0x18EA)] public byte PassageProgress;

        [FieldOffset(0x18EC)] public byte WeaponLevel;
        [FieldOffset(0x18ED)] public byte ArmorLevel;
    }
}
