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
        private readonly string _command = "/pddd";

        public CommandHandler(CommandManager manager)
        {
            _manager = manager;
        }

        public void AddCommand(string command, Action action, string helpText = "", bool show = true)
        {
            _manager.AddHandler(command, new CommandInfo((_, _) => action.Invoke())
            {
                HelpMessage = helpText,
                ShowInHelp = show
            });
        }

        public void AddCommand(string[] commands, Action action, string helpText = "", bool show = true)
        {
            foreach (var _s in commands)
            {
                _manager.AddHandler(_s, new CommandInfo((_, _) => action.Invoke())
                {
                    HelpMessage = helpText,
                    ShowInHelp = show
                });
            }
        }

        public void AddCommand(string command, Action<string> action, string helpText = "", bool show = true)
        {
            _manager.AddHandler(command, new CommandInfo((_, args) => action.Invoke(args))
            {
                HelpMessage = helpText,
                ShowInHelp = show
            });
        }

        public void AddCommand(string command, Action<string, string> action, string helpText = "", bool show = true)
        {
            _manager.AddHandler(command, new CommandInfo(action.Invoke)
            {
                HelpMessage = helpText,
                ShowInHelp = show
            });
        }
    }
}
