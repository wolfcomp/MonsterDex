using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Logging;
using Dalamud.Plugin;
using DeepDungeonDex.Models;
using DeepDungeonDex.Requests;
using DeepDungeonDex.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DeepDungeonDex
{
    public class Main : IDalamudPlugin
    {
        public string Name => "DeepDungeonDex";
        
        private IServiceProvider _provider;

        public Main(DalamudPluginInterface pluginInterface, Framework framework, CommandManager manager, TargetManager target, Condition condition)
        {
            _provider = BuildProvider(this, pluginInterface, framework, manager, target, condition);
            _provider.GetRequiredService<Data>();
            _provider.GetRequiredService<Language>();

            pluginInterface.UiBuilder.BuildFonts += () => _provider.GetRequiredService<Font>().BuildFonts(_provider.GetRequiredService<StorageHandler>().GetInstance<Configuration>()?.FontSizeScaled ?? 1f);
            var sys = LoadWindows();
            pluginInterface.UiBuilder.Draw += sys.Draw;
        }

        public void Dispose()
        {
            _provider.GetRequiredService<Data>().Dispose();
            _provider.GetRequiredService<Language>().Dispose();
            _provider.GetRequiredService<StorageHandler>().Dispose();
            _provider.GetRequiredService<CommandHandler>().Dispose();
            _provider.GetRequiredService<WindowSystem>().RemoveAllWindows();
            _provider.GetRequiredService<Font>().Dispose();
        }

        public WindowSystem LoadWindows()
        {
            var sys = _provider.GetRequiredService<WindowSystem>();
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(Window)))
                .ToList()
                .ForEach(t => sys.AddWindow((Window)ActivatorUtilities.CreateInstance(_provider, t)!));
            return sys;
        }

        private static IServiceProvider BuildProvider(Main main, DalamudPluginInterface pluginInterface, Framework framework, CommandManager manager, TargetManager target, Condition condition)
        {
            return new ServiceCollection()
                .AddSingleton(pluginInterface)
                .AddSingleton(framework)
                .AddSingleton(manager)
                .AddSingleton(target)
                .AddSingleton(condition)
                .AddSingleton(new WindowSystem("DeepDungeonDex"))
                .AddSingleton(main)
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<StorageHandler>(provider))
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<Data>(provider))
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<Language>(provider))
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<Font>(provider))
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<CommandHandler>(provider))
                .BuildServiceProvider();
        }
    }
}
