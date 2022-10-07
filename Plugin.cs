using System;
using System.Collections.Generic;
using Dalamud.Data;
using Dalamud.Game.ClientState;
using Dalamud.Plugin;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game;
using DeepDungeonDex.Localization;
using YamlDotNet.Serialization;

namespace DeepDungeonDex
{
    public class Plugin : IDalamudPlugin
    {
        #region Dalamud fields
        private readonly DalamudPluginInterface _pluginInterface;
        private readonly Condition _condition;
        private readonly TargetManager _targetManager;
        private readonly Framework _framework;
        private readonly CommandManager _commands;
        internal static DataManager GameData;
        public string Name => "DeepDungeonDex";
        #endregion

        private readonly Configuration _config;
        private readonly Font _font;
        private readonly PluginUI _ui;
        private readonly ConfigUI _cui;
        internal static Locale Locale;
        internal static readonly Deserializer Deserializer = new();
        public static List<uint> LoggedIds = new();

        private TargetData _target;

        public Plugin(DalamudPluginInterface pluginInterface, ClientState clientState, CommandManager commands, Condition condition, Framework framework, TargetManager targets, DataManager gameData)
        {
            #region Initialize
            _pluginInterface = pluginInterface;
            _condition = condition;
            _framework = framework;
            _commands = commands;
            _targetManager = targets;
            GameData = gameData;
            Locale.LoadResources();
            #endregion

            #region Load Config
            _config = (Configuration)_pluginInterface.GetPluginConfig() ?? new Configuration();
            _config.Initialize(_pluginInterface);
            Locale = new Locale(_config.LocaleString);
            #endregion

            #region Load UI Data
            DataHandler.SetupData();
            _font = new Font(_config);
            #endregion

            #region Initialize UI
            _ui = new PluginUI(_config, clientState, Locale);
            _cui = new ConfigUI(_config, Locale);

            _pluginInterface.UiBuilder.Draw += _ui.Draw;
            _pluginInterface.UiBuilder.Draw += _cui.Draw;
            _pluginInterface.UiBuilder.BuildFonts += _font.BuildFonts;
            _pluginInterface.UiBuilder.RebuildFonts();
            #endregion

            #region Initialize Commands
            _commands.AddHandler("/pddd", new CommandInfo(OpenConfig)
            {
                HelpMessage = "DeepDungeonDex config"
            });

            _commands.AddHandler("/pdddbug", new CommandInfo(OpenUIDebug)
            {
                ShowInHelp = false
            });

            _commands.AddHandler("/pdddrefresh", new CommandInfo(Refresh)
            {
                HelpMessage = "DeepDungeonDex refresh mob data"
            });
            #endregion

            _framework.Update += GetData;
        }

        public void GetData(Framework framework)
        {
            if (!_condition[ConditionFlag.InDeepDungeon])
            {
                _ui.SetData(_target);
                return;
            }
            var target = _targetManager.Target;
            
            _ui.SetData(new TargetData().IsValidTarget(target));
        }

        #region Commands

        private void Refresh(string _, string __)
        {
            DataHandler.SetupData();
            Locale.Refresh();
        }

        private void OpenUIDebug(string command, string args)
        {
            _target = string.IsNullOrWhiteSpace(args) ? null : new TargetData().SetName(Convert.ToUInt32(args.Split(' ')[0]));
        }

        public void OpenConfig(string command, string args)
        {
            _cui.IsVisible = !_cui.IsVisible;
        }
        #endregion

        #region IDisposable Support
        protected virtual void Dispose(bool disposing)
        {
            if (!disposing) return;

            _commands.RemoveHandler("/pddd");
            _commands.RemoveHandler("/pdddbug");
            _commands.RemoveHandler("/pdddrefresh");

            _pluginInterface.SavePluginConfig(_config);

            _pluginInterface.UiBuilder.Draw -= _ui.Draw;
            _pluginInterface.UiBuilder.Draw -= _cui.Draw;
            _pluginInterface.UiBuilder.BuildFonts -= _font.BuildFonts;
            _font.Dispose();

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
