using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;



// How this program works:
// Prompt the user how many timezones they want to input (CST, EST, etc)
// check the time differences between each timezone inputted
// 

// RULES // algo
// - All timezones MUST be within the predetermined range (or have their own range) 
// - See what time it would be for ALL timezones (they input 7 am and it shows whats the time for the other timezones for it) 
//- allow for a few timezones to be late but still find the most optimal

partial class Program
{
    static readonly List<string> timezones = [];
    static readonly List<DateTime> timezoneTimes = [];

    // my plan
    // add three methods (from the algo)
    // modify the main program to take in which 3 options
    // use an switch statement to work with the three options (see which one)


    static void DefaultSearch(List<DateTime> timezoneList)
    {
        
    }

    static void Main(string[] args)
    {

        do {
            Console.Write("Enter a timezone abbreviation (for example CST, EST, UTC): ");
            string abbreviation = Console.ReadLine() ?? string.Empty;
            
            if (abbreviation == "") {
                break;
            } else {
                timezones.Add(abbreviation);
            }
        } while(true);

        DateTime utcNow = DateTime.UtcNow;

        foreach (string abbreviation in timezones) {
            if (TryGetLocalTime(abbreviation, utcNow, out DateTime localTime))
            {
                timezoneTimes.Add(localTime);
                Console.WriteLine($"{abbreviation.ToUpperInvariant()}: {localTime:yyyy-MM-dd HH:mm:ss}");
            }
            else {
                Console.WriteLine($"Unknown timezone abbreviation: {abbreviation}");
            }
        }
    }

    static bool TryGetLocalTime(string abbreviation, DateTime utcNow, out DateTime localTime)
    {
        localTime = default;
        string normalized = abbreviation.Trim();

        if (TryParseUtcOffset(normalized, out TimeSpan offset))
        {
            localTime = utcNow + offset;
            return true;
        }

        if (TryGetTimeZoneInfo(normalized, out TimeZoneInfo timezone))
        {
            localTime = TimeZoneInfo.ConvertTimeFromUtc(utcNow, timezone);
            return true;
        }

        return false;
    }

    static bool TryGetTimeZoneInfo(string abbreviation, out TimeZoneInfo timezone)
    {
        Dictionary<string, string> timezoneIds = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UTC"] = "UTC",
            ["GMT"] = "UTC",
            ["Z"] = "UTC",
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

        TimeSpan parsedOffset = new TimeSpan(hours, minutes, 0);
        if (match.Groups["sign"].Value == "-")
        {
            parsedOffset = parsedOffset.Negate();
        }

        offset = parsedOffset;
        return true;
    }

    [GeneratedRegex("^(?:UTC|GMT)?(?<sign>[+-])(?<hours>\\d{1,2})(?::?(?<minutes>\\d{2}))?$", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex MyRegex();
}