using System.Globalization;
using System.Text.RegularExpressions;

static partial class TimeMatcherTimezoneUtilities
{

    // Converts a UTC DateTime to the local time in the specified timezone.
    public static DateTime ConvertFromUtc(DateTime utcDateTime, TimezoneTarget timezone)
    {
        if (timezone.TimeZoneInfo is not null)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, timezone.TimeZoneInfo);
        }

        if (timezone.Offset is not null)
        {
            return utcDateTime + timezone.Offset.Value;
        }

        return utcDateTime;
    }
    public static bool TryResolveTimezone(string abbreviation, out TimezoneTarget timezone)
    {
        string normalized = abbreviation.Trim();

        if (TryParseUtcOffset(normalized, out TimeSpan offset))
        {
            timezone = new TimezoneTarget(normalized.ToUpperInvariant(), null, offset);
            return true;
        }

        if (TryGetTimeZoneInfo(normalized, out TimeZoneInfo timeZoneInfo))
        {
            timezone = new TimezoneTarget(normalized.ToUpperInvariant(), timeZoneInfo, null);
            return true;
        }

        timezone = default;
        return false;
    }


    private static bool TryGetTimeZoneInfo(string abbreviation, out TimeZoneInfo timezone)
    {
        Dictionary<string, string> timezoneIds = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UTC"] = "UTC",
            ["GMT"] = "UTC",
            ["Z"] = "UTC",
            ["BST"] = "GMT Standard Time",
            ["CST"] = "Central Standard Time",
            ["CDT"] = "Central Standard Time",
            ["EST"] = "Eastern Standard Time",
            ["EDT"] = "Eastern Standard Time",
            ["MST"] = "Mountain Standard Time",
            ["MDT"] = "Mountain Standard Time",
            ["PST"] = "Pacific Standard Time",
            ["PDT"] = "Pacific Standard Time",
            ["AKST"] = "Alaskan Standard Time",
            ["AKDT"] = "Alaskan Standard Time",
            ["HST"] = "Hawaiian Standard Time"
        };

        if (!timezoneIds.TryGetValue(abbreviation, out string? timezoneId))
        {
            timezone = default!;
            return false;
        }

        try
        {
            timezone = TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            timezone = default!;
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            timezone = default!;
            return false;
        }
    }

    // "UTC+2" or "GMT-3:30" get ripped apart to useable offsets
    private static bool TryParseUtcOffset(string input, out TimeSpan offset)
    {
        offset = default;

        Match match = MyRegex().Match(input);
        if (!match.Success)
        {
            return false;
        }

        int hours = int.Parse(match.Groups["hours"].Value, CultureInfo.InvariantCulture);
        int minutes = match.Groups["minutes"].Success
            ? int.Parse(match.Groups["minutes"].Value, CultureInfo.InvariantCulture)
            : 0;

        if (hours > 14 || minutes > 59)
        {
            return false;
        }

        TimeSpan parsedOffset = new(hours, minutes, 0);
        if (match.Groups["sign"].Value == "-")
        {
            parsedOffset = parsedOffset.Negate();
        }

        offset = parsedOffset;
        return true;
    }
    // regex my beloved
    [GeneratedRegex("^(?:UTC|GMT)?(?<sign>[+-])(?<hours>\\d{1,2})(?::?(?<minutes>\\d{2}))?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
}