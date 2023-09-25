using FFXIVClientStructs.FFXIV.Client.Game.Event;

namespace DeepDungeonDex.Hooks;

public unsafe class AddonAgent : IDisposable
{
    private IFramework _framework;
    private IPluginLog _log;
    private EventFramework* _structsFramework;
    public byte Floor { get; private set; }
    public bool Disabled { get; private set; }

    public AddonAgent(IFramework framework, IPluginLog log, CommandHandler handler)
    {
        _framework = framework;
        _log = log;
        _framework.Update += OnUpdate;
        handler.AddCommand(new[] { "enable_floor", "e_floor", "enable_f", "ef" }, () =>
        {
            if(!Disabled)
                return;
            Disabled = false;
            _framework.Update += OnUpdate;
        }, "Resets the floor getter function to try again");
    }

    private void OnUpdate(IFramework framework)
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
            _log.Error(e, "Error trying to fetch InstanceContentDeepDungeon disabling feature.");
            Dispose(false);
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

    public void Dispose(bool disposing)
    {
        _framework.Update -= OnUpdate;
        Disabled = true;
        if (!disposing)
            return;

        _framework = null!;
        _log = null!;
    }

    public void Dispose() => Dispose(true);
}