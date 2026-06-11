using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;

// How this program works:
// Prompt the user how many timezones they want to input (CST, EST, etc)
// check the time differences between each timezone inputted

// RULES // algo
// - All timezones MUST be within the predetermined range (or have their own range)
// - See what time it would be for ALL timezones (they input 7 am and it shows whats the time for the other timezones for it)
// - allow for a few timezones to be late but still find the most optimal

partial class Program
{
    static readonly List<string> timezones = [];
    static readonly List<DateTime> timezoneTimes = [];
    static readonly List<TimezoneTarget> timezoneTargets = [];

    static void DefaultSearch(List<TimezoneTarget> timezoneList)
    {
        if (timezoneList.Count == 0)
        {
            Console.WriteLine("No timezones were entered.");
            return;
        }

        DateTime searchDate = DateTime.UtcNow.Date;

        for (int hour = 7; hour <= 21; hour++)
        {
            DateTime candidateUtc = new(searchDate.Year, searchDate.Month, searchDate.Day, hour, 0, 0, DateTimeKind.Utc);
            List<DateTime> localTimes = [];
            bool hasEarlyTime = false;

            foreach (TimezoneTarget timezone in timezoneList)
            {
                DateTime localTime = ConvertFromUtc(candidateUtc, timezone);
                localTimes.Add(localTime);

                if (localTime.Hour < 7)
                {
                    hasEarlyTime = true;
                    break;
                }
            }

            if (!hasEarlyTime)
            {
                foreach (DateTime localTime in localTimes)
                {
                    Console.WriteLine(localTime.ToString("MM/dd/yyyy hh:mm tt"));
                }

                return;
            }
        }

        Console.WriteLine("No optimal time was found between 07:00 and 21:00 that keeps every timezone out of 00:00-06:59.");
    }

    static void Main(string[] args)
    {
        do
        {
            Console.Write("Enter a timezone (for example CST, EST, UTC, UTC+2, GMT-3): ");
            string abbreviation = Console.ReadLine() ?? string.Empty;

            if (abbreviation == "")
            {
                break;
            }

            timezones.Add(abbreviation);
        } while (true);

        DateTime utcNow = DateTime.UtcNow;

        foreach (string abbreviation in timezones)
        {
            if (TryResolveTimezone(abbreviation, out TimezoneTarget timezone))
            {
                DateTime localTime = ConvertFromUtc(utcNow, timezone);
                timezoneTimes.Add(localTime);
                timezoneTargets.Add(timezone);
            }
            else
            {
                Console.WriteLine($"Unknown timezone abbreviation: {abbreviation}");
            }
        }

        DefaultSearch(timezoneTargets);
    }

    static DateTime ConvertFromUtc(DateTime utcDateTime, TimezoneTarget timezone)
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

    static bool TryResolveTimezone(string abbreviation, out TimezoneTarget timezone)
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

    static bool TryGetTimeZoneInfo(string abbreviation, out TimeZoneInfo timezone)
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

    static bool TryParseUtcOffset(string input, out TimeSpan offset)
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

    [GeneratedRegex("^(?:UTC|GMT)?(?<sign>[+-])(?<hours>\\d{1,2})(?::?(?<minutes>\\d{2}))?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();

    readonly record struct TimezoneTarget(string Label, TimeZoneInfo? TimeZoneInfo, TimeSpan? Offset);
}