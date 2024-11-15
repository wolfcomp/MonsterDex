using FFXIVClientStructs.FFXIV.Client.System.Framework;
using Lumina.Excel.Sheets;

namespace DeepDungeonDex.Weather;
public class WeatherManager : IDisposable
{
    private readonly IDataManager _data;
    private Dictionary<uint, uint> _zoneWeatherRates = new();
    private Dictionary<uint, (uint id, byte rate)[]> _weatherRates = new();
    private Dictionary<uint, string> _weatherNames = new();
    private Dictionary<uint, int> _weatherIcons = new();

    public WeatherManager(IDataManager data)
    {
        _data = data;
    }

    public string GetWeatherName(uint weatherId)
    {
        if (_weatherNames.TryGetValue(weatherId, out var name))
            return name;

        name = _data.GetExcelSheet<Lumina.Excel.Sheets.Weather>()!.FirstOrDefault(t => t.RowId == weatherId)!.Name.ToString();
        _weatherNames[weatherId] = name;

        return name;
    }

    public int GetWeatherIconId(uint weatherId)
    {
        if (_weatherIcons.TryGetValue(weatherId, out var iconId))
            return iconId;

        iconId = _data.GetExcelSheet<Lumina.Excel.Sheets.Weather>()!.FirstOrDefault(t => t.RowId == weatherId)!.Icon;
        _weatherIcons[weatherId] = iconId;

        return iconId;
    }

    public void BuildWeatherRates(uint weatherRateId)
    {
        var rates = new List<(uint id, byte rate)>();
        var weatherRate = _data.GetExcelSheet<WeatherRate>().GetRow(weatherRateId);
        for (var i = 0; i < weatherRate.Rate.Count; i++)
        {
            if (weatherRate.Weather[i].RowId == 0)
                break;
            var (_, prevRate) = rates.LastOrDefault();
            rates.Add((weatherRate.Weather[i].RowId, (byte)(weatherRate.Rate[i] + prevRate)));
        }
        _weatherRates[weatherRateId] = rates.ToArray();
    }

    private static byte CalculateTarget(Time timestamp)
    {
        var seconds = timestamp.TotalSeconds;
        var hour = seconds / Time.SecondsPerEorzeaHour;
        var shiftedHour = (uint)(hour + 8 - hour % 8) % Time.HoursPerDay;
        var day = seconds / Time.SecondsPerEorzeaDay;

        var ret = (uint)day * 100 + shiftedHour;
        ret = (ret << 11) ^ ret;
        ret = (ret >> 8) ^ ret;
        ret %= 100;
        return (byte)ret;
    }

    private (uint id, byte rate)[] GetWeatherRates(uint zoneId)
    {
        if (!_zoneWeatherRates.TryGetValue(zoneId, out var weatherRateId))
        {
            weatherRateId = _data.GetExcelSheet<TerritoryType>()!.FirstOrDefault(t => t.RowId == zoneId)!.WeatherRate;
        }

        if (_weatherRates.TryGetValue(weatherRateId, out var rates))
            return rates;

        BuildWeatherRates(weatherRateId);
        rates = _weatherRates[weatherRateId];

        return rates;
    }

    public uint[] GetWeatherIdsFromZone(uint zoneId) => GetWeatherRates(zoneId).Select(t => t.id).ToArray();

    public uint GetCurrentWeather(uint zoneId)
    {
        var serverTime = Framework.GetServerTime() * 1000;
        var dateTime = (Time)serverTime;
        dateTime.SyncToWeather();
        var target = CalculateTarget(dateTime);
        var weather = GetWeatherRates(zoneId).FirstOrDefault(t => t.rate > target);
        return weather.id;
    }

    public (uint id, Time time) GetNextOccurrence(uint weatherId, uint zoneId)
    {
        var serverTime = Framework.GetServerTime() * 1000;
        var dateTime = (Time)serverTime;
        dateTime.SyncToWeather();
        var target = CalculateTarget(dateTime);
        var rates = GetWeatherRates(zoneId);

        var weather = rates.FirstOrDefault(t => t.rate > target);
        while (weather.id != weatherId && weatherId != 0)
        {
            dateTime += Time.MillisecondsPerEorzeaWeather;
            target = CalculateTarget(dateTime);
            weather = rates.FirstOrDefault(t => t.rate >= target);
        }

        return (weather.id, dateTime);
    }

    public Time GetNextOccurrenceDuring(uint weatherId, uint zoneId, ElementalChangeTime change)
    {
        if (change.HasFlag(ElementalChangeTime.Day)) return GetNextOccurrence(weatherId, zoneId).time;
        var serverTime = Framework.GetServerTime() * 1000;
        var dateTime = (Time)serverTime;
        dateTime.SyncToWeather();
        var eorzeaTime = dateTime.GetEorzeanTime();
        while (eorzeaTime.Hours is not (16 or 0))
        {
            dateTime += Time.MillisecondsPerEorzeaWeather;
            eorzeaTime = dateTime.GetEorzeanTime();
        }
        var target = CalculateTarget(dateTime);
        var rates = GetWeatherRates(zoneId);

        var weather = rates.FirstOrDefault(t => t.rate > target);
        while (weather.id != weatherId && weatherId != 0)
        {
            dateTime += Time.MillisecondsPerEorzeaWeather;
            target = CalculateTarget(dateTime);
            weather = rates.FirstOrDefault(t => t.rate >= target);
        }
        return dateTime;
    }

    public void Dispose()
    {
        _weatherNames.Clear();
        _weatherRates.Clear();
        _zoneWeatherRates.Clear();
    }
}
