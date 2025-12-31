using System;
using System.Collections.Generic;

namespace BiblCalCore
{
    /// <summary>
    /// Flood date calculations
    /// Calculates the number of days between 17th of 2nd Month and 17th of 7th month
    /// to see if there are 150 days as per the flood scriptures.
    /// Adapted from modFlood.cs
    /// </summary>
    public class FloodCalculations
    {
        private readonly IOutputWriter _outputWriter;
        private readonly BiblicalCalendarCalculator _calculator;

        public FloodCalculations(IOutputWriter outputWriter, BiblicalCalendarCalculator calculator)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        /// <summary>
        /// Calculates flood dates for a year range
        /// </summary>
        public FloodCalculationResult CalculateFloodDates(double startYear, double? endYear = null)
        {
            var result = new FloodCalculationResult
            {
                StartYear = startYear,
                EndYear = endYear ?? startYear
            };

            // Validate and adjust years
            if (result.EndYear < -4004) result.EndYear = -4004;
            if (result.EndYear == 0) result.EndYear = -1;
            if (result.EndYear > 9999) result.EndYear = 9999;
            if (result.EndYear < startYear)
            {
                var temp = startYear;
                startYear = result.EndYear;
                result.EndYear = temp;
            }

            bool printTable = result.EndYear > startYear;
            if (!printTable)
            {
                _outputWriter.Clear();
            }

            var oneFifty = new List<double>();
            var plusOne = new List<double>();
            var plusTwo = new List<double>();

            for (double year = startYear; year <= result.EndYear; year++)
            {
                if (year == 0) year = 1;

                var yearResult = CalculateFloodForYear(year);
                
                if (yearResult.DaysBetween == 150)
                {
                    oneFifty.Add(year);
                    if (!printTable)
                    {
                        _outputWriter.WriteLine("*******");
                    }
                }
                else if (yearResult.PossibleDays.Contains(150))
                {
                    plusOne.Add(year);
                    if (!printTable)
                    {
                        _outputWriter.WriteLine("**********");
                    }
                }
                else if (yearResult.PossibleDays.Contains(148) && yearResult.PossibleDays.Contains(150))
                {
                    plusTwo.Add(year);
                    if (!printTable)
                    {
                        _outputWriter.WriteLine("**********");
                    }
                }

                if (!printTable)
                {
                    _outputWriter.WriteLine($"There are {yearResult.DaysBetween} days between these dates.");
                    if (yearResult.PossibleDays.Count > 0)
                    {
                        _outputWriter.WriteLine($"Possibly {string.Join(" or ", yearResult.PossibleDays)} days.");
                    }
                    _outputWriter.WriteLine("");
                }
            }

            result.YearsWith150Days = oneFifty;
            result.YearsWith149Plus1 = plusOne;
            result.YearsWith148Plus2 = plusTwo;

            if (printTable)
            {
                PrintFloodTable(result);
            }

            return result;
        }

        private YearFloodResult CalculateFloodForYear(double year)
        {
            // Set location to Mount Ararat
            _calculator.LG = -44.32d;
            _calculator.LT = 39.69d;
            _calculator.HR = 14;
            _calculator.GregorianYear = year;
            _calculator.InitializeVariables();

            // Calculate first day of year (simplified - would need full FindFirstDay implementation)
            // For now, this is a placeholder that captures the structure
            var firstMonthJD = _calculator.ConvertToJulian(3, 1, year);
            var secondMonth17JD = firstMonthJD + 30 + 16; // Approximate
            var seventhMonth17JD = firstMonthJD + (6 * 30) + 16; // Approximate

            var daysBetween = seventhMonth17JD - secondMonth17JD;

            var result = new YearFloodResult
            {
                Year = year,
                DaysBetween = (int)daysBetween,
                SecondMonth17JD = secondMonth17JD,
                SeventhMonth17JD = seventhMonth17JD
            };

            // Calculate possible variations
            result.PossibleDays.Add((int)daysBetween - 1);
            result.PossibleDays.Add((int)daysBetween);
            result.PossibleDays.Add((int)daysBetween + 1);
            result.PossibleDays.Add((int)daysBetween + 2);

            return result;
        }

        private void PrintFloodTable(FloodCalculationResult result)
        {
            _outputWriter.WriteLine($"First year of run is {FormatYear(result.StartYear)}          Last year of run is {FormatYear(result.EndYear)}");
            _outputWriter.WriteLine(" 150 days           149(+1)             148(+2)");
            _outputWriter.WriteLine(" ===================================================================");

            int maxCount = Math.Max(Math.Max(result.YearsWith150Days.Count, result.YearsWith149Plus1.Count), result.YearsWith148Plus2.Count);

            for (int i = 0; i < maxCount; i++)
            {
                string line = "";
                if (i < result.YearsWith150Days.Count)
                {
                    line += FormatYear(result.YearsWith150Days[i]).PadRight(20);
                }
                else
                {
                    line += "".PadRight(20);
                }

                if (i < result.YearsWith149Plus1.Count)
                {
                    line += FormatYear(result.YearsWith149Plus1[i]).PadRight(20);
                }
                else
                {
                    line += "".PadRight(20);
                }

                if (i < result.YearsWith148Plus2.Count)
                {
                    line += FormatYear(result.YearsWith148Plus2[i]);
                }

                _outputWriter.WriteLine(line);
            }
        }

        private string FormatYear(double year)
        {
            return _calculator.FormatYearString(year);
        }
    }

    public class FloodCalculationResult
    {
        public double StartYear { get; set; }
        public double EndYear { get; set; }
        public List<double> YearsWith150Days { get; set; } = new List<double>();
        public List<double> YearsWith149Plus1 { get; set; } = new List<double>();
        public List<double> YearsWith148Plus2 { get; set; } = new List<double>();
    }

    public class YearFloodResult
    {
        public double Year { get; set; }
        public int DaysBetween { get; set; }
        public double SecondMonth17JD { get; set; }
        public double SeventhMonth17JD { get; set; }
        public List<int> PossibleDays { get; set; } = new List<int>();
    }
}

