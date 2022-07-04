using System;
using Dalamud.Game.ClientState;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Game;

namespace DeepDungeonDex
{
    public class Plugin : IDalamudPlugin
    {
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly Configuration _config;
        private readonly PluginUI _ui;
        private readonly ConfigUI _cui;
        private GameObject _previousTarget;
        private ClientState _clientState;
        private readonly Condition _condition;
        private readonly TargetManager _targetManager;
        private readonly Framework _framework;
        private readonly CommandManager _commands;

        public string Name => "DeepDungeonDex";

        public Plugin(
            DalamudPluginInterface pluginInterface,
            ClientState clientState,
            CommandManager commands,
            Condition condition,
            Framework framework,
            //SeStringManager seStringManager,
            TargetManager targets)
        {
            this._pluginInterface = pluginInterface;
            this._clientState = clientState;
            this._condition = condition;
            this._framework = framework;
            this._commands = commands;
            this._targetManager = targets;

            this._config = (Configuration)this._pluginInterface.GetPluginConfig() ?? new Configuration();
            this._config.Initialize(this._pluginInterface);
            this._ui = new PluginUI(_config, clientState);
            this._cui = new ConfigUI(_config.Opacity, _config.IsClickThrough, _config.HideRedVulns, _config.HideBasedOnJob, _config);
            this._pluginInterface.UiBuilder.Draw += this._ui.Draw;
            this._pluginInterface.UiBuilder.Draw += this._cui.Draw;

            this._commands.AddHandler("/pddd", new CommandInfo(OpenConfig)
            {
                HelpMessage = "DeepDungeonDex config"
            });

            this._framework.Update += this.GetData;
        }

        public void OpenConfig(string command, string args)
        {
            _cui.IsVisible = true;
        }

        public void GetData(Framework framework)
        {
            if (!this._condition[ConditionFlag.InDeepDungeon])
            {
                _ui.IsVisible = false;
                return;
            }
            var target = _targetManager.Target;

            var targetData = new TargetData();
            if (!targetData.IsValidTarget(target))
            {
                _ui.IsVisible = false;
            }
            else
            { 
                _previousTarget = target;
                _ui.IsVisible = true;
            }
        }

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            this._commands.RemoveHandler("/pddd");

            this._pluginInterface.SavePluginConfig(this._config);

            this._pluginInterface.UiBuilder.Draw -= this._ui.Draw;
            this._pluginInterface.UiBuilder.Draw -= this._cui.Draw;

            this._framework.Update -= this.GetData;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
