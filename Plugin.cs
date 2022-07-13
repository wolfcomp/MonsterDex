using System;
using Dalamud.Game.ClientState;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Command;
using Dalamud.Game;
using DeepDungeonDex.Localization;

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
        private readonly Locale _locale;

        public string Name => "DeepDungeonDex";

        public Plugin(DalamudPluginInterface pluginInterface, ClientState clientState, CommandManager commands, Condition condition, Framework framework, TargetManager targets)
        {
            _pluginInterface = pluginInterface;
            _clientState = clientState;
            _condition = condition;
            _framework = framework;
            _commands = commands;
            _targetManager = targets;

            _locale = new Locale();
            _config = (Configuration)_pluginInterface.GetPluginConfig() ?? new Configuration();
            _config.Initialize(_pluginInterface);
            _ui = new PluginUI(_config, clientState, _locale);
            _cui = new ConfigUI(_config.Opacity, _config.IsClickthrough, _config.HideRedVulns, _config.HideBasedOnJob, _config.Locale, _config, _locale);
            _pluginInterface.UiBuilder.Draw += _ui.Draw;
            _pluginInterface.UiBuilder.Draw += _cui.Draw;

            _commands.AddHandler("/pddd", new CommandInfo(OpenConfig)
            {
                HelpMessage = "DeepDungeonDex config"
            });

            _framework.Update += GetData;
        }

        public void OpenConfig(string command, string args)
        {
            _cui.IsVisible = true;
        }

        public void GetData(Framework framework)
        {
            if (!_condition[ConditionFlag.InDeepDungeon])
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

            _commands.RemoveHandler("/pddd");

            _pluginInterface.SavePluginConfig(_config);

            _pluginInterface.UiBuilder.Draw -= _ui.Draw;
            _pluginInterface.UiBuilder.Draw -= _cui.Draw;

            _framework.Update -= GetData;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
