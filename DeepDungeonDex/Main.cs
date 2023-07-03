﻿using System.Threading.Tasks;
using Dalamud.Data;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using DeepDungeonDex.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace DeepDungeonDex;

public class Main : IDalamudPlugin
{
    public string Name => "DeepDungeonDex";

    private IServiceProvider _provider;

    public Main(DalamudPluginInterface pluginInterface, Framework framework, CommandManager manager, TargetManager target, Condition condition, DataManager gameData, ClientState state, ChatGui chat)
    {
        _provider = BuildProvider(this, pluginInterface, framework, manager, target, condition, gameData, state, chat);
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

    private static IServiceProvider BuildProvider(Main main, DalamudPluginInterface pluginInterface, Framework framework, CommandManager manager, TargetManager target, Condition condition, DataManager gameData, ClientState state, ChatGui chat)
    {
        return new ServiceCollection()
            .AddSingleton(pluginInterface)
            .AddSingleton(framework)
            .AddSingleton(manager)
            .AddSingleton(target)
            .AddSingleton(condition)
            .AddSingleton(gameData)
            .AddSingleton(state)
            .AddSingleton(chat)
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
}