using System.Numerics;

namespace DeepDungeonDex.Windows;
public partial class Main
{
    public unsafe void DrawDeepDungeonData()
    {
        ImGui.TextUnformatted($"{_currentMob.Name}{(_config.Debug ? $" ({_currentNpc->NameId})" : "")}");
        ImGui.TextUnformatted($"{_locale.GetLocale(_currentMob.Aggro.ToString())}\t");
        ImGui.SameLine();
        PrintTextWithColor(_locale.GetLocale(_currentMob.Threat.ToString()), _currentMob.Threat.GetColor());
        ImGui.NewLine();
        ImGui.TextUnformatted(_locale.GetLocale("Vulns"));
        ImGui.SameLine();
        DrawDDWeakness(_currentMob.Weakness);
        // ReSharper disable once InvertIf
        if (!string.IsNullOrWhiteSpace(_currentMob.JoinedProcessedDescription))
        {
            ImGui.NewLine();
            ImGui.TextUnformatted(_locale.GetLocale("Notes") + ":\n");
            var size = ImGui.GetWindowContentRegionMax();
            var desc = _currentMob.ProcessedDescription;
            if (desc.Length == 0 || Math.Abs(size.X - _currentMob.LastProcessedWidth) > float.Epsilon)
                _currentMob.ProcessDescription(size.X);
            foreach (var s in desc)
            {
                ImGui.TextUnformatted(s);
            }
        }
    }

    public void DrawDDWeakness(Weakness weakness)
    {
        var size = new Vector2(24 * _config.FontSize / 16f, 32 * _config.FontSize / 16f);
        DrawWeaknessIcon(15004, size, weakness, Weakness.Stun);
        ImGui.SameLine();
        DrawWeaknessIcon(15002, size, weakness, Weakness.Heavy);
        ImGui.SameLine();
        DrawWeaknessIcon(15009, size, weakness, Weakness.Slow);
        ImGui.SameLine();
        DrawWeaknessIcon(15013, size, weakness, Weakness.Sleep);
        ImGui.SameLine();
        DrawWeaknessIcon(15003, size, weakness, Weakness.Bind);

        // ReSharper disable once InvertIf
        if (_currentMob.Id is not (>= 7262 and <= 7610) && _clientState.TerritoryType is >= 561 and <= 565 or >= 593 and <= 607 || weakness.HasFlag(Weakness.Undead))
        {
            ImGui.SameLine();
            DrawWeaknessIcon(15461, size, weakness, Weakness.Undead);
        }
    }
}
