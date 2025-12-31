using System;
using System.Collections.Generic;

namespace BiblCalCore
{
    /// <summary>
    /// Sunset time calculations for user locations
    /// Adapted from modSunsets.cs
    /// </summary>
    public class SunsetCalculations
    {
        private readonly IOutputWriter _outputWriter;
        private readonly BiblicalCalendarCalculator _calculator;

        public SunsetCalculations(IOutputWriter outputWriter, BiblicalCalendarCalculator calculator)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        /// <summary>
        /// Calculates sunset times for a year
        /// </summary>
        public List<SunsetResult> CalculateSunsets(double year, double longitude, double latitude, double gmtOffset)
        {
            _calculator.LG = longitude;
            _calculator.LT = latitude;
            _calculator.HR = gmtOffset;
            _calculator.GregorianYear = year;
            _calculator.InitializeVariables();

            var results = new List<SunsetResult>();

            _outputWriter.WriteLine($"{FormatYear(year)} CALCULATED SUNSETS");
            _outputWriter.WriteLine($"Location: Longitude {longitude}, Latitude {latitude}, GMT Offset {gmtOffset}");
            _outputWriter.WriteLine("Times do not reflect changes in 'Daylight Saving Time'");
            _outputWriter.WriteLine("________________________________________________________________________________________");

            // Calculate starting date (first month or January 1)
            var startJD = _calculator.ConvertToJulian(1, 1, year);
            int daysToCalculate = 107; // Approximately 3.5 months

            for (int day = 0; day < daysToCalculate; day++)
            {
                var currentJD = startJD + day;
                var sunset = CalculateSunsetForDate(currentJD);

                if (sunset != null)
                {
                    results.Add(sunset);
                    
                    // Print in columns of 4
                    if (day % 4 == 0 && day > 0)
                    {
                        _outputWriter.WriteLine("");
                    }

                    var (month, dayNum, yearNum) = _calculator.JulianToGregorian(currentJD);
                    _outputWriter.Write($"{dayNum}/{month} {sunset.SunsetTime:HH:mm} PM    ");
                }
            }

            _outputWriter.WriteLine("");

            return results;
        }

        private SunsetResult CalculateSunsetForDate(double jd)
        {
            // Simplified sunset calculation
            // Full implementation would use complex astronomical formulas
            var (month, day, year) = _calculator.JulianToGregorian(jd);
            
            // Placeholder calculation - would need full astronomical implementation
            var sunsetHour = 18.0; // 6 PM as placeholder
            var sunsetMinute = 0.0;

            return new SunsetResult
            {
                JulianDay = jd,
                Date = new DateTime((int)year, month, day),
                SunsetTime = new TimeSpan((int)sunsetHour, (int)sunsetMinute, 0)
            };
        }

        private string FormatYear(double year)
        {
            return _calculator.FormatYearString(year);
        }
    }

    public class SunsetResult
    {
        public double JulianDay { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan SunsetTime { get; set; }
    }
}

