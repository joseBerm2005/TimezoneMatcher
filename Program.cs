using System.Globalization;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

partial


// How this program works:
// Prompt the user how many timezones they want to input (CST, EST, etc)
// check the time differences between each timezone inputted
// 

// RULES // algo
// - All timezones MUST be within the predetermined range (or have their own range) 
// - See what time it would be for ALL timezones (they input 7 am and it shows whats the time for the other timezones for it) 
//- allow for a few timezones to be late but still find the most optimal

class Program
{
    static readonly List<string> timezones = []; 

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

        foreach (string abbreviation in timezones) {
            if (TryGetUtcOffset(abbreviation, out TimeSpan offset))
            {
                string sign = offset >= TimeSpan.Zero ? "+" : "-";
                TimeSpan absoluteOffset = offset.Duration();

             //   Console.WriteLine($"{abbreviation.ToUpperInvariant()} -> UTC{sign}{absoluteOffset:hh\\:mm}");
            }
            else {
                Console.WriteLine($"Unknown timezone abbreviation: {abbreviation}");
            }
        }
    }

    static bool TryGetUtcOffset(string abbreviation, out TimeSpan offset)
    {
        string normalized = abbreviation.Trim();

        if (TryParseUtcOffset(normalized, out offset))
        {
            return true;
        }

        Dictionary<string, TimeSpan> offsets = new(StringComparer.OrdinalIgnoreCase)
        {
            ["UTC"] = TimeSpan.Zero,
            ["GMT"] = TimeSpan.Zero,
            ["Z"] = TimeSpan.Zero,
            ["CST"] = TimeSpan.FromHours(-6),
            ["CDT"] = TimeSpan.FromHours(-5),
            ["EST"] = TimeSpan.FromHours(-5),
            ["EDT"] = TimeSpan.FromHours(-4),
            ["MST"] = TimeSpan.FromHours(-7),
            ["MDT"] = TimeSpan.FromHours(-6),
            ["PST"] = TimeSpan.FromHours(-8),
            ["PDT"] = TimeSpan.FromHours(-7),
            ["AKST"] = TimeSpan.FromHours(-9),
            ["AKDT"] = TimeSpan.FromHours(-8),
            ["HST"] = TimeSpan.FromHours(-10)
        };

        return offsets.TryGetValue(normalized, out offset);
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