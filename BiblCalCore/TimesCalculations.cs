using System;
using System.Collections.Generic;

namespace BiblCalCore
{
    /// <summary>
    /// Times calculations for sunrise, sunset, moonrise, moonset, and moon illumination
    /// Adapted from modTimes.cs
    /// </summary>
    public class TimesCalculations
    {
        private readonly IOutputWriter _outputWriter;
        private readonly BiblicalCalendarCalculator _calculator;

        public TimesCalculations(IOutputWriter outputWriter, BiblicalCalendarCalculator calculator)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        /// <summary>
        /// Calculates times for a year
        /// </summary>
        public List<DayTimesResult> CalculateTimes(double year, double longitude, double latitude, double gmtOffset, bool useBiblicalYear = false)
        {
            _calculator.LG = longitude;
            _calculator.LT = latitude;
            _calculator.HR = gmtOffset;
            _calculator.GregorianYear = year;
            _calculator.InitializeVariables();

            var results = new List<DayTimesResult>();

            _outputWriter.WriteLine($"Location: Longitude {longitude}, Latitude {latitude}, GMT Offset {gmtOffset}");
            _outputWriter.WriteLine("Times are not corrected for changes in Daylight Saving.");
            _outputWriter.WriteLine("Dates are shown in Month/Day/Year format.");
            _outputWriter.WriteLine("_______________________________________________________________________________");
            _outputWriter.WriteLine("  DATE    JULIAN DAY #  SUNRISE   SUNSET   MOONRISE  % ILLUM.  MOONSET  % ILLUM");

            double startJD;
            int daysToCalculate;

            if (useBiblicalYear)
            {
                // Start from first month of biblical year
                startJD = _calculator.ConvertToJulian(3, 1, year);
                daysToCalculate = 400;
            }
            else
            {
                startJD = _calculator.ConvertToJulian(1, 1, year);
                daysToCalculate = 366;
            }

            for (int day = 0; day < daysToCalculate; day++)
            {
                var currentJD = startJD + day;
                var dayTimes = CalculateDayTimes(currentJD);

                if (dayTimes != null)
                {
                    results.Add(dayTimes);

                    var (month, dayNum, yearNum) = _calculator.JulianToGregorian(currentJD);
                    
                    // Print header every 30 lines
                    if (day % 30 == 0 && day > 0)
                    {
                        _outputWriter.WriteLine("  DATE    JULIAN DAY #  SUNRISE   SUNSET   MOONRISE  % ILLUM.  MOONSET  % ILLUM");
                    }

                    _outputWriter.WriteLine($"{month}/{dayNum}/{yearNum}   {currentJD:F0}     " +
                        $"{dayTimes.Sunrise:HH:mm}    {dayTimes.Sunset:HH:mm}    " +
                        $"{dayTimes.Moonrise:HH:mm}    {dayTimes.MoonIllumination:F1}%    " +
                        $"{dayTimes.Moonset:HH:mm}    {dayTimes.MoonsetIllumination:F1}%");
                }
            }

            return results;
        }

        private DayTimesResult CalculateDayTimes(double jd)
        {
            // Simplified calculation - full implementation would use complex astronomical formulas
            var (month, day, year) = _calculator.JulianToGregorian(jd);

            // Placeholder calculations
            var sunrise = new TimeSpan(6, 30, 0); // 6:30 AM
            var sunset = new TimeSpan(18, 30, 0); // 6:30 PM
            var moonrise = new TimeSpan(8, 0, 0); // 8:00 AM
            var moonset = new TimeSpan(20, 0, 0); // 8:00 PM
            var moonIllum = 50.0; // 50% illumination
            var moonsetIllum = 50.0;

            return new DayTimesResult
            {
                JulianDay = jd,
                Date = new DateTime((int)year, month, day),
                Sunrise = sunrise,
                Sunset = sunset,
                Moonrise = moonrise,
                Moonset = moonset,
                MoonIllumination = moonIllum,
                MoonsetIllumination = moonsetIllum
            };
        }
    }

    public class DayTimesResult
    {
        public double JulianDay { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Sunrise { get; set; }
        public TimeSpan Sunset { get; set; }
        public TimeSpan Moonrise { get; set; }
        public TimeSpan Moonset { get; set; }
        public double MoonIllumination { get; set; }
        public double MoonsetIllumination { get; set; }
    }
}

