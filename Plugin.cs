using System;
using ImGuiNET;

using Dalamud.Plugin;
using DeepDungeonDex.Attributes;
using Dalamud.Game.Internal;
using Dalamud.Game.ClientState.Actors.Types;

namespace DeepDungeonDex
{
    public class Plugin : IDalamudPlugin
    {
        private DalamudPluginInterface pluginInterface;
        //private PluginCommandManager<Plugin> commandManager;
        private Configuration config;
        private PluginUI ui;
        private Actor previousTarget;

        public string Name => "DeepDungeonDex";

        public void Initialize(DalamudPluginInterface pluginInterface)
        {
            this.pluginInterface = pluginInterface;

            this.config = (Configuration)this.pluginInterface.GetPluginConfig() ?? new Configuration();
            this.config.Initialize(this.pluginInterface);

            this.ui = new PluginUI();
            this.pluginInterface.UiBuilder.OnBuildUi += this.ui.Draw;


            //this.commandManager = new PluginCommandManager<Plugin>(this, this.pluginInterface);

            this.pluginInterface.Framework.OnUpdateEvent += this.GetData;
        }

        public void GetData(Framework framework)
        {
            //var chat = this.pluginInterface.Framework.Gui.Chat;
            //if (!this.pluginInterface.ClientState.Condition[Dalamud.Game.ClientState.ConditionFlag.InDeepDungeon]) return;
            var target = pluginInterface.ClientState.Targets.CurrentTarget;
            if (target == null || target == previousTarget) 
            {
                ui.IsVisible = false;
                return;
            }
            TargetData t = new TargetData();
            t.FetchTarget(target);
            if (TargetData.nameID == null)
            {
                ui.IsVisible = false;
                return;
            }
            previousTarget = target;
            ui.IsVisible = true;
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            //this.commandManager.Dispose();

            this.pluginInterface.SavePluginConfig(this.config);

            this.pluginInterface.UiBuilder.OnBuildUi -= this.ui.Draw;

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
