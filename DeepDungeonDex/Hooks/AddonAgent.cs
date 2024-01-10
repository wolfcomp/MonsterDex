using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace DeepDungeonDex.Hooks;

public unsafe class AddonAgent : IDisposable
{
    private IFramework _framework;
    private IPluginLog _log;
    private EventFramework* _structsFramework;
    public byte Floor { get; private set; }
    public bool Disabled { get; private set; }
    public ContentType ContentType { get; private set; }

    public AddonAgent(IFramework framework, IPluginLog log, CommandHandler handler)
    {
        _framework = framework;
        _log = log;
        _framework.Update += OnUpdate;
        handler.AddCommand(new[] { "enable_floor", "e_floor", "enable_f", "ef" }, () =>
        {
            if (!Disabled)
                return;
            Disabled = false;
            _framework.Update += OnUpdate;
        }, "Resets the floor getter function to try again");
    }

    private void OnUpdate(IFramework framework)
    {
        _structsFramework = EventFramework.Instance();
        if (!IsContentSafe())
            return;
        try
        {
            var activeInstance = _structsFramework->GetInstanceContentDeepDungeon();
            var activePublic = (PublicContentDirectorResearch*)_structsFramework->GetPublicContentDirector();

            if (activeInstance != null)
            {
                ContentType = (ContentType)(1 << ((int)activeInstance->InstanceContentDirector.InstanceContentType - 1));
                Floor = activeInstance->Floor;
            }
            else if (activePublic != null)
            {
                ContentType = (ContentType)(1 << (int)(activePublic->PublicContentDirectorType + 19));
            }
            else
            {
                ContentType = ContentType.None;
            }
        }
        catch (Exception e)
        {
            _log.Error(e, "Error trying to fetch InstanceContentDeepDungeon disabling feature.");
            Dispose(false);
        }
    }

    private bool IsContentSafe()
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

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PublicContentDirectorResearch
{
    [FieldOffset(0x0)] public PublicContentDirector PublicContentDirector;
    [FieldOffset(0xC76)] public uint PublicContentDirectorType;
}