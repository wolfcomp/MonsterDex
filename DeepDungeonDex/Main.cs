using System.Collections.Concurrent;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.IoC;
using DeepDungeonDex.Hooks;
using DeepDungeonDex.Weather;
using Microsoft.Extensions.DependencyInjection;

namespace DeepDungeonDex;

public class Main : IDalamudPlugin
{
    private ServiceProvider _provider;
    private bool isDisposed;

    internal static ConcurrentBag<IDisposable> Services = new();

    public Main(IDalamudPluginInterface pluginInterface)
    {
        _provider = BuildProvider(this, pluginInterface);
        _provider.GetRequiredService<Requests>();
#pragma warning disable CS4014 
        _provider.GetRequiredService<CommandHandler>().AddCommand(new[] { "refresh", "clear" }, () => { RefreshData(); }, "Refreshes the data internally stored");
#pragma warning restore CS4014 

        var sys = LoadWindows();
        pluginInterface.UiBuilder.Draw += sys.Draw;
    }

    public async Task RefreshData()
    {
        var req = _provider.GetRequiredService<Requests>();
        await req.RefreshFileList(false);
        await req.RefreshLang(false);
    }

    public void Dispose()
    {
        if (isDisposed)
            return;
        isDisposed = true;
        _provider.GetRequiredService<WindowSystem>().DisposeAndRemoveAllWindows();
        _provider.GetRequiredService<IDalamudPluginInterface>().UiBuilder.Draw -= _provider.GetRequiredService<WindowSystem>().Draw;
        // _provider.GetRequiredService<StorageHandler>().GetInstance<Configuration>()!.OnSizeChange -= _provider.GetRequiredService<DalamudPluginInterface>().UiBuilder.RebuildFonts;
        // _provider.GetRequiredService<DalamudPluginInterface>().UiBuilder.BuildFonts -= BuildFont;
        _provider.DisposeDI();
        _provider = null!;
        foreach (var dalamudServiceIntermediate in Services)
        {
            dalamudServiceIntermediate.Dispose();
        }
        Services.Clear();
    }

    public WindowSystem LoadWindows()
    {
        var sys = _provider.GetRequiredService<WindowSystem>();
        var log = _provider.GetRequiredService<IPluginLog>();
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Window)))
            .ToList()
            .ForEach(t =>
            {
                log.Verbose($"Loading window: {t.Name}");
                sys.AddWindow((Window)ActivatorUtilities.CreateInstance(_provider, t)!);
            });
        return sys;
    }

    private static ServiceProvider BuildProvider(Main main, IDalamudPluginInterface pluginInterface)
    {
        var fontAtlas =
            pluginInterface.UiBuilder.CreateFontAtlas(FontAtlasAutoRebuildMode.Async, false, "DeepDungeonDexFontAtlas");

        return new ServiceCollection()
            .AddSingleton(pluginInterface)
            .AddDalamudService<IFramework>()
            .AddDalamudService<ICommandManager>()
            .AddDalamudService<ITargetManager>()
            .AddDalamudService<ICondition>()
            .AddDalamudService<IClientState>()
            .AddDalamudService<IChatGui>()
            .AddDalamudService<ITextureProvider>()
            .AddDalamudService<IDataManager>()
            .AddDalamudService<IPluginLog>()
            .AddSingleton(fontAtlas)
            .AddSingleton(new WindowSystem("DeepDungeonDex"))
            .AddSingleton(main)
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<StorageHandler>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<Requests>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<Font.Font>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<CommandHandler>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<AddonAgent>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<WeatherManager>(provider))
            .BuildServiceProvider();
    }
}

public static class Extensions
{
    public static void DisposeAndRemoveAllWindows(this WindowSystem windowSystem)
    {
        foreach (var windowSystemWindow in windowSystem.Windows)
        {
            if (windowSystemWindow is IDisposable disposable)
                disposable.Dispose();
        }
        windowSystem.RemoveAllWindows();
    }

    public static IServiceCollection AddDalamudService<T>(this IServiceCollection collection) where T : class
    {
        return collection.AddSingleton(provider =>
        {
            var k = new DalamudServiceIntermediate<T>(provider.GetRequiredService<IDalamudPluginInterface>());
            return k.Service;
        });
    }

    // Only used to circumvent the fact that Framework would be disposed through Microsoft.Extensions.DependencyInjection.ServiceProvider
    public static void DisposeDI(this ServiceProvider provider)
    {
        foreach (var obj in provider.GetInternalObject<IServiceScope>("Root").GetInternalObject<IList<object>>("Disposables"))
        {
            if (obj is IDisposable disposable && (disposable.GetType().AssemblyQualifiedName?.StartsWith("DeepDungeonDex") ?? false))
                disposable.Dispose();
        }
    }

    // Only used to circumvent the fact that Framework would be disposed through Microsoft.Extensions.DependencyInjection.ServiceProvider
    public static T GetInternalObject<T>(this object obj, string fieldName)
    {
        var type = obj.GetType();
        var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (field != null)
            return (T)field.GetValue(obj)!;
        var prop = type.GetProperty(fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (prop != null)
            return (T)prop.GetValue(obj)!;
        return default!;
    }
}

public class DalamudServiceIntermediate<T> : IDisposable
    where T : class
{
    [PluginService] public T Service { get; private set; } = null!;

    public DalamudServiceIntermediate(T service)
    {
        Service = service;
    }

    public DalamudServiceIntermediate(IDalamudPluginInterface pluginInterface)
    {
        pluginInterface.Inject(this);
        Main.Services.Add(this);
    }

    public void Dispose()
    {
        Service = null!;
    }
}