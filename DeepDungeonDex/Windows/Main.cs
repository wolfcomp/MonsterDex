using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Windowing;
using DeepDungeonDex.Models;
using DeepDungeonDex.Storage;
using ImGuiNET;

namespace DeepDungeonDex.Windows
{
    public class Main : Window, IDisposable
    {
        private readonly Condition _condition;
        private readonly TargetManager _target;
        private readonly StorageHandler _storage;
        private readonly Framework _framework;
        private uint _targetId;
        private bool _debug;

        public Main(StorageHandler storage, CommandHandler command, TargetManager target, Framework framework, Condition condition) : base("DeepDungeonDex MobView", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
        {
            _condition = condition;
            _target = target;
            _storage = storage;
            _framework = framework;
            framework.Update += GetData;
            command.AddCommand("debug", (args) =>
            {
                if (!uint.TryParse(args.Split(' ')[0], out var id))
                {
                    IsOpen = false;
                    return;
                }

                _targetId = id;

                if (_storage.GetInstances<MobData>().GetData(id) is null)
                {
                    IsOpen = false;
                    return;
                }
                IsOpen = true;
            }, show: false);
        }

        public void Dispose()
        {
            _framework.Update -= GetData;
        }

        private void GetData(Framework framework)
        {
            if (!_condition[ConditionFlag.InDeepDungeon] && !_debug)
            {
                IsOpen = false;
                return;
            }
            
            if (_target.Target is not BattleNpc npc)
            {
                if(!_debug)
                    IsOpen = false;
                return;
            }

            _targetId = npc.NameId;
            if (_storage.GetInstances<MobData>().GetData(_targetId) is null)
            {
                IsOpen = false;
                return;
            }
            IsOpen = true;
        }

        public override void Draw()
        {
            var data = _storage.GetInstances<MobData>().GetData(_targetId);
            if (data == null)
                return;
            var legacy = _storage.GetInstance<Configuration>()?.LegacyWindow ?? false;
            if(legacy)
                DrawLegacy(data);
            DrawCompact(data);
        }

        public void DrawCompact(Mob data)
        {

        }

        public void DrawLegacy(Mob data)
        {

        }
    }
}
