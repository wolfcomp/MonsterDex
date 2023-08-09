using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using DeepDungeonDex.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace DeepDungeonDex;

public class Main : IDalamudPlugin
{
    public string Name => "DeepDungeonDex";

    private IServiceProvider _provider;

    public Main(DalamudPluginInterface pluginInterface)
    {
        _provider = BuildProvider(this, pluginInterface);
        _provider.GetRequiredService<Requests>();
        _provider.GetRequiredService<StorageHandler>().GetInstance<Configuration>()!.OnSizeChange += pluginInterface.UiBuilder.RebuildFonts;
        _provider.GetRequiredService<CommandHandler>().AddCommand(new[] { "refresh", "clear" }, () => { RefreshData(); }, "Refreshes the data internally stored");

        var sys = LoadWindows();
        pluginInterface.UiBuilder.Draw += sys.Draw;
        pluginInterface.UiBuilder.BuildFonts += BuildFont;
        pluginInterface.UiBuilder.RebuildFonts();
    }

    public async Task RefreshData()
    {
        var req = _provider.GetRequiredService<Requests>();
        await req.RefreshFileList(false);
        await req.RefreshLang(false);
    }

    public void BuildFont()
    {
        _provider.GetRequiredService<Font>().BuildFonts(_provider.GetRequiredService<StorageHandler>().GetInstance<Configuration>()?.FontSizeScaled ?? 1f);
    }

    public void Dispose()
    {
        _provider.GetRequiredService<WindowSystem>().DisposeAndRemoveAllWindows();
        _provider.GetRequiredService<DalamudPluginInterface>().UiBuilder.Draw -= _provider.GetRequiredService<WindowSystem>().Draw;
        _provider.GetRequiredService<StorageHandler>().GetInstance<Configuration>()!.OnSizeChange -= _provider.GetRequiredService<DalamudPluginInterface>().UiBuilder.RebuildFonts;
        _provider.GetRequiredService<DalamudPluginInterface>().UiBuilder.BuildFonts -= BuildFont;
        _provider.GetRequiredService<Requests>().Dispose();
        _provider.GetRequiredService<StorageHandler>().Dispose();
        _provider.GetRequiredService<CommandHandler>().Dispose();
        _provider.GetRequiredService<Font>().Dispose();
        _provider.GetRequiredService<AddonAgent>().Dispose();
    }

    public WindowSystem LoadWindows()
    {
        var sys = _provider.GetRequiredService<WindowSystem>();
        Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsSubclassOf(typeof(Window)))
            .ToList()
            .ForEach(t =>
            {
                PluginLog.Verbose($"Loading window: {t.Name}");
                sys.AddWindow((Window)ActivatorUtilities.CreateInstance(_provider, t)!);
            });
        return sys;
    }

    private static IServiceProvider BuildProvider(Main main, DalamudPluginInterface pluginInterface)
    {
        return new ServiceCollection()
            .AddSingleton(pluginInterface)
            .AddDalamudService<Framework>()
            .AddDalamudService<ICommandManager>()
            .AddDalamudService<TargetManager>()
            .AddDalamudService<Condition>()
            .AddDalamudService<IClientState>()
            .AddDalamudService<ChatGui>()
            .AddDalamudService<ITextureProvider>()
            .AddSingleton(new WindowSystem("DeepDungeonDex"))
            .AddSingleton(main)
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<StorageHandler>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<Requests>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<Font>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<CommandHandler>(provider))
            .AddSingleton(provider => ActivatorUtilities.CreateInstance<AddonAgent>(provider))
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
            return new DalamudServiceIntermediate<T>(provider.GetRequiredService<DalamudPluginInterface>()).Service;
        });
    }
}

public class DalamudServiceIntermediate<T> where T : class
{
    [PluginService] public T Service { get; private set; } = null!;

    public DalamudServiceIntermediate(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Inject(this);
    }
}