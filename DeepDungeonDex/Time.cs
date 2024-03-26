using System.Globalization;

namespace DeepDungeonDex;

/// <summary>
/// A class for representing time in unix milliseconds.
/// This is used as a wrapper for <see cref="DateTimeOffset"/> to allow for easy conversion to and from ffxiv server time.
/// </summary>
public class Time : IComparable<Time>, IEquatable<Time>
{
    public const int MillisecondsPerSecond = 1000;
    public const int MillisecondsPerMinute = MillisecondsPerSecond * SecondsPerMinute;
    public const int MillisecondsPerHour = MillisecondsPerMinute * MinutesPerHour;
    public const int MillisecondsPerDay = MillisecondsPerHour * HoursPerDay;

    public const int SecondsPerMinute = 60;
    public const int SecondsPerHour = SecondsPerMinute * MinutesPerHour;
    public const int SecondsPerDay = SecondsPerHour * HoursPerDay;

    public const int MinutesPerHour = 60;
    public const int MinutesPerDay = MinutesPerHour * HoursPerDay;

    public const int HoursPerDay = 24;

    public const int MillisecondsPerEorzeaHour = 175000;
    public const int SecondsPerEorzeaHour = MillisecondsPerEorzeaHour / MillisecondsPerSecond;
    public const int MillisecondsPerEorzeaWeather = 8 * MillisecondsPerEorzeaHour;
    public const int SecondsPerEorzeaWeather = MillisecondsPerEorzeaWeather / MillisecondsPerSecond;
    public const int MillisecondsPerEorzeaDay = HoursPerDay * MillisecondsPerEorzeaHour;
    public const int SecondsPerEorzeaDay = MillisecondsPerEorzeaDay / MillisecondsPerSecond;

    public long Milliseconds { get; set; }

    public static Time Now => DateTimeOffset.UtcNow;

    public void SyncToWeather()
    {
        Milliseconds -= Milliseconds % MillisecondsPerEorzeaWeather;
    }

    public int CompareTo(Time? other) => Milliseconds.CompareTo(other?.Milliseconds);

    public bool Equals(Time? other) => Milliseconds == other?.Milliseconds;

    public long TotalSeconds
        => Milliseconds / MillisecondsPerSecond;

    public static implicit operator Time(DateTimeOffset dateTimeOffset)
    {
        return new Time { Milliseconds = dateTimeOffset.ToUnixTimeMilliseconds() };
    }

    public static implicit operator DateTime(Time time)
    {
        return DateTimeOffset.FromUnixTimeMilliseconds(time.Milliseconds).LocalDateTime;
    }

    public static explicit operator Time(long milliseconds)
    {
        return new Time { Milliseconds = milliseconds };
    }

    public static explicit operator long(Time time)
    {
        return time.Milliseconds;
    }
    public static long operator -(Time lhs, Time rhs)
        => lhs.Milliseconds - rhs.Milliseconds;

    public static Time operator +(Time lhs, long offset)
        => (Time)(lhs.Milliseconds + offset);

    public static Time operator +(long offset, Time rhs)
        => rhs + offset;

    public static Time operator -(Time lhs, long offset)
        => (Time)(lhs.Milliseconds - offset);

    public static bool operator ==(Time left, Time right)
        => left.Milliseconds == right.Milliseconds;

    public static bool operator !=(Time left, Time right)
        => left.Milliseconds != right.Milliseconds;

    public static bool operator <(Time left, Time right)
        => left.Milliseconds < right.Milliseconds;

    public static bool operator <=(Time left, Time right)
        => left.Milliseconds <= right.Milliseconds;

    public static bool operator >(Time left, Time right)
        => left.Milliseconds > right.Milliseconds;

    public static bool operator >=(Time left, Time right)
        => left.Milliseconds >= right.Milliseconds;

    public override bool Equals(object? obj)
    {
        if (obj is Time time)
            return Equals(time);
        return false;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Milliseconds.GetHashCode();
    }

    public override string ToString()
    {
        return ((DateTime)this).ToString(CultureInfo.InvariantCulture);
    }

    public string ToString(string? format)
    {
        return ((DateTime)this).ToString(format, CultureInfo.InvariantCulture);
    }

    public TimeSpan GetEorzeanTime()
    {
        var utc = Milliseconds;
        var eorzea = utc * 3600 / 175;
        var eorzeaTime = eorzea % (24 * 60 * 60);
        var eorzeaHour = (int)Math.Floor(eorzeaTime / (60d * 60d));
        var eorzeaMinute = (int)Math.Floor((eorzeaTime - eorzeaHour * 60d * 60d) / 60d);
        var eorzeaSecond = (int)Math.Floor(eorzeaTime - eorzeaHour * 60d * 60d - eorzeaMinute * 60d);
        return new TimeSpan(eorzeaHour, eorzeaMinute, eorzeaSecond);
    }

    public bool IsEorzeaDay()
    {
        var eorzeaTime = GetEorzeanTime();
        return eorzeaTime.Hours >= 8;
    }

    public bool IsEorzeaNight()
    {
        var eorzeaTime = GetEorzeanTime();
        return eorzeaTime.Hours is >= 0 and < 8 or >= 16;
    }
}
