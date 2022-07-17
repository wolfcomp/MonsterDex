using System;
using ImGuiNET;
using System.Diagnostics;
using System.Numerics;
using DeepDungeonDex.Localization;

namespace DeepDungeonDex
{
    public class ConfigUI
    {

        public bool IsVisible { get; set; }
        private float _opacity;
        private bool _isClickthrough;
        private bool _hideRedVulns;
        private bool _hideBasedOnJob;
        private int _localeInt;
        private readonly Configuration _config;
        private readonly Locale _locale;
        private float _fontSize;

        public ConfigUI(float opacity, bool isClickthrough, bool hideRedVulns, bool hideBasedOnJob, int localeInt, float fontSize, Configuration config, Locale locale)
        {
            _config = config;
            _opacity = opacity;
            _isClickthrough = isClickthrough;
            _hideRedVulns = hideRedVulns;
            _hideBasedOnJob = hideBasedOnJob;
            _fontSize = fontSize;
            _localeInt = localeInt;
            _locale = locale;
        }
        
        public void Draw()
        {
            if (!IsVisible)
                return;
            ImGui.PushFont(Plugin.RegularFont);
            ImGui.SetNextWindowSizeConstraints(new Vector2(250, 100), new Vector2(400, 300));
            ImGui.Begin("config", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize);
            if (ImGui.SliderFloat(_locale.Opacity, ref _opacity, 0.0f, 1.0f))
            {
                _config.Opacity = _opacity;
            }
            ImGui.Columns(4,null, false);
            ImGui.Text(_locale.FontSize);
            foreach (var f in new float[] { 12, 14, 16, 18, 24, 32 })
            {
                ImGui.NextColumn();
                if (ImGui.RadioButton($"{f}px", Math.Abs(_config.FontSize - f) < float.Epsilon))
                {
                    _config.FontSize = f;
                }
            }
            ImGui.Columns(1);
            if (ImGui.Checkbox(_locale.IsClickthrough, ref _isClickthrough))
            {
                _config.IsClickthrough = _isClickthrough;
            }
            if (ImGui.Checkbox(_locale.HideRedVulns, ref _hideRedVulns))
            {
                _config.HideRedVulns = _hideRedVulns;
            }
            if (ImGui.Checkbox(_locale.HideBasedOnJob, ref _hideBasedOnJob))
            {
                _config.HideBasedOnJob = _hideBasedOnJob;
            }
            if (ImGui.Combo("Locale", ref _localeInt, new [] { "English", "日本語", "Français", "Deutsch", "汉语", "漢語", "한국어" }, 7))
            {
                _config.Locale = _localeInt;
                _locale.ChangeLocale(_config.LocaleString);
            }
            ImGui.NewLine();
            if (ImGui.Button(_locale.Save))
            {
                IsVisible = false;
                _config.Save();
            }
            ImGui.SameLine();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(400f);
                ImGui.TextWrapped(_locale.Thanks);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }; 
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Button, 0xFF5E5BFF);
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xFF5E5BAA);
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xFF5E5BDD);
            ImGui.PopStyleColor(3);
            ImGui.End();
            ImGui.PopFont();
        }
    }
}
