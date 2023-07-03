using Dalamud.Game.Command;

namespace DeepDungeonDex;

public class CommandHandler : IDisposable
{
    private readonly CommandManager _manager;
    private readonly string _command = "/pddd";
    private readonly List<string> _commands = new();

    public CommandHandler(CommandManager manager)
    {
        _manager = manager;
    }

    public void AddCommand(string command, Action action, string helpText = "", bool show = true)
    {
        _commands.Add(_command + command);
        _manager.AddHandler(_command + command, new CommandInfo((_, _) => action.Invoke())
        {
            HelpMessage = helpText,
            ShowInHelp = show
        });
    }

    public void AddCommand(string[] commands, Action action, string helpText = "", bool show = true)
    {
        foreach (var _s in commands)
        {
            _commands.Add(_command + _s);
            _manager.AddHandler(_command + _s, new CommandInfo((_, _) => action.Invoke())
            {
                HelpMessage = helpText,
                ShowInHelp = show
            });
        }
    }

    public void AddCommand(string command, Action<string> action, string helpText = "", bool show = true)
    {
        _commands.Add(_command + command);
        _manager.AddHandler(_command + command, new CommandInfo((_, args) => action.Invoke(args))
        {
            HelpMessage = helpText,
            ShowInHelp = show
        });
    }

    public void AddCommand(string command, Action<string, string> action, string helpText = "", bool show = true)
    {
        _commands.Add(_command + command);
        _manager.AddHandler(_command + command, new CommandInfo(action.Invoke)
        {
            HelpMessage = helpText,
            ShowInHelp = show
        });
    }

    public void Dispose()
    {
        _commands.ForEach(s => _manager.RemoveHandler(s));
    }
}