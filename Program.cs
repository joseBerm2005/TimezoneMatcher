using System.Collections.Generic;
using TimeMatcher.UserOptions;

partial class Program
{
    static readonly List<string> timezones = [];

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
        List<TimezoneTarget> timezoneTargets = [];

        Console.WriteLine("Current times:");
        foreach (string abbreviation in timezones)
        {
            if (TimeMatcherTimezoneUtilities.TryResolveTimezone(abbreviation, out TimezoneTarget timezone))
            {
                DateTime localTime = TimeMatcherTimezoneUtilities.ConvertFromUtc(utcNow, timezone);
                timezoneTargets.Add(timezone);
                Console.WriteLine($"{timezone.Label}: {localTime:MM/dd/yyyy hh:mm tt}");
            }
            else
            {
                Console.WriteLine($"Unknown timezone abbreviation: {abbreviation}");
            }
        }

        DefaultSearch.Run(timezoneTargets);
    }
}