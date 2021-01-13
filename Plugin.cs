using System;

using Dalamud.Plugin;
using Dalamud.Game.Internal;
using Dalamud.Game.ClientState.Actors.Types;
using Dalamud.Game.Command;

namespace DeepDungeonDex
{
    public class Plugin : IDalamudPlugin
    {
        private DalamudPluginInterface pluginInterface;
        private Configuration config;
        private PluginUI ui;
        private ConfigUI cui;
        private Actor previousTarget;

        public string Name => "DeepDungeonDex";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.config = (Configuration)this.pluginInterface.GetPluginConfig() ?? new Configuration();
            this.config.Initialize(this.pluginInterface);
            this.ui = new PluginUI(config);
            this.cui = new ConfigUI(config.Opacity, config.IsClickthrough, config);
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;
            this.pluginInterface.UiBuilder.OnBuildUi += this.cui.Draw;

            this.pluginInterface.CommandManager.AddHandler("/pddd", new CommandInfo(OpenConfig)
            {
                HelpMessage = "DeepDungeonDex config"
            });

            this.pluginInterface.Framework.OnUpdateEvent += this.GetData;
        }

        public void OpenConfig(string command, string args)
        {
            cui.IsVisible = true;
        }

        public void GetData(Framework framework)
        {
            if (!this.pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InDeepDungeon])
            {
                ui.IsVisible = false;
                return;
            }
            var target = pluginInterface.ClientState.Targets.CurrentTarget;
            if (target == null || target == previousTarget) 
            {
                ui.IsVisible = false;
                return;
            }
            TargetData t = new TargetData();
            if (!t.IsValidTarget(target))
            {
                ui.IsVisible = false;
                return;
            }
            else
            { 
                previousTarget = target;
                ui.IsVisible = true;
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this.pluginInterface.CommandManager.RemoveHandler("/pddd");

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;
            this.pluginInterface.UiBuilder.OnBuildUi -= this.cui.Draw;

            this.pluginInterface.Framework.OnUpdateEvent -= this.GetData;

            this.pluginInterface.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
