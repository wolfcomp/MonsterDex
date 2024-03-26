using System.Numerics;

namespace DeepDungeonDex.Windows;
public partial class Main
{
    public void DrawEurekaData()
    {
        ImGui.TextUnformatted($"{_currentMob.Name}{(_config.Debug ? $" ({_currentMob.Id})" : "")}");
        ImGui.TextUnformatted($"{_locale.GetLocale(_currentMob.Aggro.ToString())}\t");
#if DEBUG
        var territoryId = 827u;
#else
        var territoryId = _clientState.TerritoryType;
#endif
        var weatherIds = _weatherManager.GetWeatherIdsFromZone(territoryId);
        if (weatherIds.Any(t => !_currentMob.ElementalChangeTimes.ContainsKey(t)) && _currentMob.ElementalChangeTimes.Any())
        {
            var (id, time) = _weatherManager.GetNextOccurrence(0, territoryId);
            foreach (var (weatherId, elementalChangeTime) in _currentMob.ElementalChangeTimes)
            {
                uint nextId;
                Time nextTime;
                if (elementalChangeTime == ElementalChangeTime.Both)
                {
                    (nextId, nextTime) = _weatherManager.GetNextOccurrence(weatherId, territoryId);
                    if (nextTime < time)
                    {
                        time = nextTime;
                        id = nextId;
                    }
                    continue;
                }
                nextTime = _weatherManager.GetNextOccurrenceDuring(weatherId, territoryId, elementalChangeTime);
                if (nextTime < time)
                {
                    time = nextTime;
                    id = weatherId;
                }
            }
            var size = new Vector2(32 * _config.FontSize / 16f, 32 * _config.FontSize / 16f);
            DrawIcon((uint)_weatherManager.GetWeatherIconId(id), size, _color);
            ImGui.SameLine();
            ImGui.TextUnformatted(_weatherManager.GetWeatherName(id));
            ImGui.SameLine();
            ImGui.TextUnformatted(time.ToString("HH:mm"));
        }
    }
}
