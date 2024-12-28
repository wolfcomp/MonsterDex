using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using ContentType = DeepDungeonDex.Storage.ContentType;

namespace DeepDungeonDex.Hooks;

public unsafe class AddonAgent : IDisposable
{
    private IFramework _framework;
    private IPluginLog _log;
    private EventFramework* _eventFramework;
    private EnvManager* _envManager;
    public byte Floor { get; private set; }
    public bool DirectorDisabled { get; private set; }
    public ContentType ContentType { get; private set; }
    public byte Weather { get; private set; }
    public byte[] WeatherIds { get; private set; } = new byte[32];

    public AddonAgent(IFramework framework, IPluginLog log)
    {
        _framework = framework;
        _log = log;
        _framework.Update += OnUpdate;
        OnUpdate(framework);
    }

    private void OnUpdate(IFramework framework)
    {
        _eventFramework = EventFramework.Instance();
        _envManager = EnvManager.Instance();
        if (!DirectorDisabled)
            CheckDirector();
        CheckWeather();
    }

    private void CheckWeather()
    {
        if (_envManager == null)
            return;

        Weather = _envManager->ActiveWeather;
        if (_envManager->EnvScene == null)
            return;

        WeatherIds = _envManager->EnvScene->WeatherIds.ToArray();
    }

    private void CheckDirector()
    {
        try
        {
            if (!IsContentSafe())
            {
                ContentType = ContentType.None;
                return;
            }

            var activeInstance = _eventFramework->GetInstanceContentDirector();

            if (activeInstance != null)
            {
                ContentType = (ContentType)(1 << ((int)activeInstance->InstanceContentType));
                if (ContentType.HasFlag(ContentType.DeepDungeon))
                {
                    Floor = ((InstanceContentDeepDungeon*)activeInstance)->Floor;
                }
            }
            else
            {
                var activePublic = (PublicContentDirectorResearch*)_eventFramework->GetPublicContentDirector();
                if (activePublic != null)
                {
                    ContentType = (ContentType)(1 << (int)(activePublic->PublicContentDirectorType + 20));
                }
                else
                {
                    ContentType = ContentType.None;
                }
            }
        }
        catch (Exception e)
        {
            _log.Error(e, "Error trying to fetch ContentDirector disabling feature.");
            Dispose(false);
        }
    }

    private bool IsContentSafe()
    {
        var contentDirector = _eventFramework->GetContentDirector();

        if ((IntPtr)contentDirector == IntPtr.Zero)
            return false;

        var eventHandlerInfo = contentDirector->Director.EventHandlerInfo;

        return (IntPtr)eventHandlerInfo != IntPtr.Zero;
    }

    public void Restart()
    {
        if (!DirectorDisabled)
            return;
        DirectorDisabled = false;
    }

    public void Dispose(bool disposing)
    {
        DirectorDisabled = true;
        if (!disposing)
            return;

        _framework.Update -= OnUpdate;
        _framework = null!;
        _log = null!;
    }

    public void Dispose() => Dispose(true);
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct PublicContentDirectorResearch
{
    [FieldOffset(0x0)] public PublicContentDirector PublicContentDirector;
    [FieldOffset(0xDB0)] public byte PublicContentDirectorType;
}