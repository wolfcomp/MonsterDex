using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using DeepDungeonDex.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace DeepDungeonDex
{
    public class Main : IDalamudPlugin
    {
        public string Name => "DeepDungeonDex";
        
        private IServiceProvider _provider;

        public Main(DalamudPluginInterface pluginInterface, Framework framework, CommandManager manager)
        {
            _provider = BuildProvider(this, pluginInterface, framework, manager);

            pluginInterface.UiBuilder.BuildFonts += () => _provider.GetRequiredService<Font>().BuildFonts(_provider.GetRequiredService<StorageHandler>().GetInstance<Configuration>()?.FontSizeScaled ?? 1f);
        }

        public void Dispose()
        {
            _provider.GetRequiredService<StorageHandler>().Dispose();
            _provider.GetRequiredService<WindowSystem>().RemoveAllWindows();
            _provider.GetRequiredService<Font>().Dispose();
        }

        private IServiceProvider BuildProvider(Main main, DalamudPluginInterface pluginInterface, Framework framework, CommandManager manager)
        {
            return new ServiceCollection()
                .AddSingleton(pluginInterface)
                .AddSingleton(framework)
                .AddSingleton(manager)
                .AddSingleton(new WindowSystem("DeepDungeonDex"))
                .AddSingleton(main)
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<StorageHandler>(provider))
                .AddSingleton(provider => ActivatorUtilities.CreateInstance<Font>(provider))
                .BuildServiceProvider();
        }
    }
}
