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
        DrawWeakness(_currentMob.Weakness);
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
}
