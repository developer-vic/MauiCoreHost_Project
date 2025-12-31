using System;
using System.Collections.Generic;

namespace BiblCalCore
{
    /// <summary>
    /// Golgotha, Jordan Crossing, and Creation date calculations
    /// Adapted from modGolgotha.cs
    /// </summary>
    public class GolgothaCalculations
    {
        private readonly IOutputWriter _outputWriter;
        private readonly BiblicalCalendarCalculator _calculator;

        public GolgothaCalculations(IOutputWriter outputWriter, BiblicalCalendarCalculator calculator)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
            _calculator = calculator ?? throw new ArgumentNullException(nameof(calculator));
        }

        /// <summary>
        /// Calculates possible dates for Jesus' Crucifixion (Golgotha)
        /// </summary>
        public List<GolgothaResult> CalculateGolgothaDates(double startYear, double? endYear = null)
        {
            var results = new List<GolgothaResult>();
            double end = endYear ?? startYear;

            if (end > startYear)
            {
                _outputWriter.WriteLine("The following years may have the Passover sacrifice on Wednesday.");
                _outputWriter.WriteLine($"First year of run is {FormatYear(startYear)}");
                _outputWriter.WriteLine($"Last year of run is {FormatYear(end)}");
                _outputWriter.WriteLine("==============================");
            }
            else
            {
                _outputWriter.WriteLine($"Possible Dates for Jesus' Crucifixion, Resurrection and Pentecost in {FormatYear(startYear)}");
            }

            for (double year = startYear; year <= end; year++)
            {
                if (year == 0) year = 1;

                var result = CalculateGolgothaForYear(year);
                if (result != null)
                {
                    results.Add(result);
                    if (end > startYear)
                    {
                        _outputWriter.Write($"{FormatYear(year)}  ");
                    }
                    else
                    {
                        PrintDetailedGolgotha(result);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Calculates possible dates for Jordan Crossing
        /// </summary>
        public List<JordanCrossingResult> CalculateJordanCrossingDates(double startYear, double? endYear = null)
        {
            var results = new List<JordanCrossingResult>();
            double end = endYear ?? startYear;

            if (end > startYear)
            {
                _outputWriter.WriteLine("The following years may have the Passover sacrifice on the Sabbath.");
            }
            else
            {
                _outputWriter.WriteLine($"Possible Dates for the Jordan Crossing in {FormatYear(startYear)}");
            }

            for (double year = startYear; year <= end; year++)
            {
                if (year == 0) year = 1;

                var result = CalculateJordanForYear(year);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        /// <summary>
        /// Calculates possible dates for Creation
        /// </summary>
        public List<CreationResult> CalculateCreationDates(double startYear, double? endYear = null)
        {
            var results = new List<CreationResult>();
            double end = endYear ?? startYear;

            if (end > startYear)
            {
                _outputWriter.WriteLine("The following years may have Abib 1 on Sunday.");
            }
            else
            {
                _outputWriter.WriteLine($"Possible Dates for Creation in {FormatYear(startYear)}");
            }

            for (double year = startYear; year <= end; year++)
            {
                if (year == 0) year = 1;

                var result = CalculateCreationForYear(year);
                if (result != null)
                {
                    results.Add(result);
                }
            }

            return results;
        }

        private GolgothaResult CalculateGolgothaForYear(double year)
        {
            // Set location to Jerusalem
            _calculator.LG = -35.244d;
            _calculator.LT = 31.78d;
            _calculator.HR = 14;
            _calculator.GregorianYear = year;
            _calculator.InitializeVariables();

            // Simplified calculation - would need full implementation
            // This captures the structure
            var abib1JD = _calculator.ConvertToJulian(3, 1, year);
            var passoverJD = abib1JD + 13;
            var dayOfWeek = (int)(passoverJD % 7);

            // Check if Passover falls on Wednesday (day 3)
            if (dayOfWeek == 3)
            {
                return new GolgothaResult
                {
                    Year = year,
                    Abib1JD = abib1JD,
                    PassoverJD = passoverJD,
                    FeastOfUnleavenedBreadStart = passoverJD + 1,
                    FeastOfUnleavenedBreadEnd = passoverJD + 7,
                    WaveOfferingJD = CalculateWaveOffering(passoverJD, dayOfWeek),
                    PentecostJD = CalculateWaveOffering(passoverJD, dayOfWeek) + 49
                };
            }

            return null;
        }

        private JordanCrossingResult CalculateJordanForYear(double year)
        {
            _calculator.LG = -35.244d;
            _calculator.LT = 31.78d;
            _calculator.HR = 14;
            _calculator.GregorianYear = year;
            _calculator.InitializeVariables();

            var abib1JD = _calculator.ConvertToJulian(3, 1, year);
            var passoverJD = abib1JD + 13;
            var dayOfWeek = (int)(passoverJD % 7);

            // Check if Passover falls on Sabbath (day 6)
            if (dayOfWeek == 6)
            {
                return new JordanCrossingResult
                {
                    Year = year,
                    PassoverJD = passoverJD,
                    WaveOfferingJD = CalculateWaveOffering(passoverJD, dayOfWeek),
                    PentecostJD = CalculateWaveOffering(passoverJD, dayOfWeek) + 49
                };
            }

            return null;
        }

        private CreationResult CalculateCreationForYear(double year)
        {
            // Set location to Garden of Eden
            _calculator.LG = -42;
            _calculator.LT = 38.5d;
            _calculator.HR = 14;
            _calculator.GregorianYear = year;
            _calculator.InitializeVariables();

            var abib1JD = _calculator.ConvertToJulian(3, 1, year);
            var dayOfWeek = (int)(abib1JD % 7);

            // Check if Abib 1 falls on Sunday (day 0)
            if (dayOfWeek == 0)
            {
                return new CreationResult
                {
                    Year = year,
                    Abib1JD = abib1JD
                };
            }

            return null;
        }

        private double CalculateWaveOffering(double passoverJD, int dayOfWeek)
        {
            // Wave Offering is the day after the Sabbath during Feast of Unleavened Bread
            // Simplified calculation
            var feastStart = passoverJD + 1;
            var daysToNextSunday = (7 - dayOfWeek) % 7;
            if (daysToNextSunday == 0) daysToNextSunday = 7;
            return feastStart + daysToNextSunday;
        }

        private void PrintDetailedGolgotha(GolgothaResult result)
        {
            _outputWriter.WriteLine($"Abib 1 is {FormatJD(result.Abib1JD)}");
            _outputWriter.WriteLine($"Passover sacrifice is {FormatJD(result.PassoverJD)}");
            _outputWriter.WriteLine("********** This year has the proper day of the week for Christ's death. **********");
            _outputWriter.WriteLine($"Feast of Unleavened Bread runs from {FormatJD(result.FeastOfUnleavenedBreadStart)} to {FormatJD(result.FeastOfUnleavenedBreadEnd)}");
            _outputWriter.WriteLine($"The Wave Offering (the First-Fruit) is {FormatJD(result.WaveOfferingJD)}");
            _outputWriter.WriteLine($"First-Fruits (Pentecost) is {FormatJD(result.PentecostJD)}");
        }

        private string FormatYear(double year)
        {
            return _calculator.FormatYearString(year);
        }

        private string FormatJD(double jd)
        {
            var (month, day, year) = _calculator.JulianToGregorian(jd);
            return $"{day}/{month}/{year}";
        }
    }

    public class GolgothaResult
    {
        public double Year { get; set; }
        public double Abib1JD { get; set; }
        public double PassoverJD { get; set; }
        public double FeastOfUnleavenedBreadStart { get; set; }
        public double FeastOfUnleavenedBreadEnd { get; set; }
        public double WaveOfferingJD { get; set; }
        public double PentecostJD { get; set; }
    }

    public class JordanCrossingResult
    {
        public double Year { get; set; }
        public double PassoverJD { get; set; }
        public double WaveOfferingJD { get; set; }
        public double PentecostJD { get; set; }
    }

    public class CreationResult
    {
        public double Year { get; set; }
        public double Abib1JD { get; set; }
    }
}

