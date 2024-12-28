using Dalamud.Game.Command;

namespace DeepDungeonDex;

public class CommandHandler : IDisposable
{
    private ICommandManager _manager;
    private IChatGui _chat;
    private readonly string _command = "/pdex";
    private readonly Dictionary<string[], Tuple<object, string, bool>> _actions = new();
    private readonly string[] _help = new[] { "help", "h" };

    public CommandHandler(ICommandManager manager, IChatGui chat)
    {
        _manager = manager;
        _chat = chat;
    }

    private string CommandStrings => string.Join("\n\t", _actions.Where(t => t.Value.Item3).Select(t => $"{string.Join(", ", t.Key)} → {t.Value.Item2}"));

    private void AddMainHandler()
    {
        _manager.AddHandler(_command, new CommandInfo(ProcessCommand)
        {
            HelpMessage = $"Monster Dex commands\n\thelp, h → shows all commands in chat\n\t{CommandStrings}",
            ShowInHelp = true
        });
    }

    public void ProcessCommand(string command, string argument)
    {
        var args = argument.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (args.Length == 0 || _help.Any(t => args.First().ToLowerInvariant() == t))
        {
            if (args.Length == 0)
                _chat.PrintError("[MonsterDex] Expected additional args");
            _chat.Print($"[MonsterDex] Available commands:");
            foreach (var (key, val) in _actions)
            {
                var (_, help, show) = val;
                if (show)
                    _chat.Print($"  {string.Join(", ", key)} → {help}");
            }

            return;
        }

        var action = args[0];
        if (!_actions.Keys.SelectMany(t => t).Contains(action, StringComparer.InvariantCultureIgnoreCase))
        {
            _chat.PrintError($"[MonsterDex] Unknown action {action}");
            return;
        }

        var (act, _, _) = _actions.First(t => t.Key.Contains(action, StringComparer.InvariantCultureIgnoreCase)).Value;
        var actType = act.GetType();
        if (actType == typeof(Action<string>))
            (act as Action<string>)!.Invoke(string.Join(' ', args.Skip(1)));
        else
            (act as Action)!.Invoke();
    }

    public void AddCommand(string command, Action action, string helpText = "", bool show = true)
    {
        AddCommand(new[] { command }, action, helpText, show);
    }

    public void AddCommand(string[] commands, Action action, string helpText = "", bool show = true)
    {
        RemoveMainHandler();
        _actions.Add(commands, new Tuple<object, string, bool>(action, helpText, show));
        AddMainHandler();
    }

    public void AddCommand(string command, Action<string> action, string helpText = "", bool show = true)
    {
        RemoveMainHandler();
        _actions.Add(new[] { command }, new Tuple<object, string, bool>(action, helpText, show));
        AddMainHandler();
    }

    public void Dispose()
    {
        RemoveMainHandler();
        _actions.Clear();
        _chat = null!;
        _manager = null!;
    }

    public void RemoveMainHandler()
    {
        if (_manager.Commands.ContainsKey(_command))
            _manager.RemoveHandler(_command);
    }
}