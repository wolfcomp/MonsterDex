using Dalamud.Game.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepDungeonDex
{
    public class CommandHandler
    {
        private readonly CommandManager _manager;

        public CommandHandler(CommandManager manager)
        {
            _manager = manager;
        }
    }
}
