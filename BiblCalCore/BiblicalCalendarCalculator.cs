using System;

namespace BiblCalCore
{
    /// <summary>
    /// Core calculator for Biblical calendar calculations
    /// Extracted from modBiblcalFunctions and adapted for cross-platform use
    /// </summary>
    public class BiblicalCalendarCalculator
    {
        private readonly IOutputWriter _outputWriter;
        private readonly IUserDataProvider _userDataProvider;

        // Constants
        public const double PI = 3.14159265358979d;
        public const double VEQ = 6.79189999999999E-02d; // Constant for Spring Equinox calculation
        public const double JDNJLD = 1170111.5d; // Julian Day number of Jeshua's long day
        public const double JDNJLDBench = 1170111.5d; // Benchmark

        // Core calculation variables
        public double GregorianYear { get; set; }
        public double LG { get; set; } // Longitude
        public double LT { get; set; } // Latitude
        public double HR { get; set; } // Hour location
        public double DR { get; set; } = 0.01745329251993d; // Degrees to radians conversion

        public BiblicalCalendarCalculator(IOutputWriter outputWriter, IUserDataProvider userDataProvider)
        {
            _outputWriter = outputWriter ?? throw new ArgumentNullException(nameof(outputWriter));
            _userDataProvider = userDataProvider ?? throw new ArgumentNullException(nameof(userDataProvider));
        }

        /// <summary>
        /// Converts Gregorian date to Julian Day Number
        /// Based on Jean Meeus formula 7.1 pages 60-61 of Astronomical Algorithms Second Edition
        /// </summary>
        public double ConvertToJulian(double monthNum, double dayNum, double yearNum)
        {
            if (monthNum < 3)
            {
                yearNum--;
                monthNum += 12;
            }

            double A = Math.Floor(yearNum / 100d);
            double B = Math.Floor(2 - A + Math.Floor(A / 4d));
            double result = Math.Floor(365.25d * (yearNum + 4716)) + Math.Floor(30.6001d * (monthNum + 1)) + dayNum + B - 1524.5d;
            return Math.Floor(result) + 1;
        }

        /// <summary>
        /// Converts Julian Day Number to Gregorian date
        /// Based on Jean Meeus formula 3 pages 26-27 of Astronomical Formulae for Calculators Fourth Edition
        /// </summary>
        public (int month, int day, int year) JulianToGregorian(double jd)
        {
            double Z = Math.Floor(jd);
            double F = jd - Z;

            double Alpha = Math.Floor((Z - 1867216.25d) / 36524.25d);
            double A = Z + 1 + Alpha - Math.Floor(Alpha / 4d);
            double B = A + 1524;
            double C = Math.Floor((B - 122.1d) / 365.25d);
            double D = Math.Floor(365.25d * C);
            double E = Math.Floor((B - D) / 30.6001d);
            double dayNum = Math.Round(B - D - Math.Floor(30.6001d * E) + F, 2);

            double monthNum;
            if (E < 14)
            {
                monthNum = E - 1;
            }
            else
            {
                monthNum = E - 13;
            }

            double yearNum;
            if (monthNum > 2)
            {
                yearNum = C - 4716;
            }
            else
            {
                yearNum = C - 4715;
            }

            if (yearNum < 1)
            {
                yearNum--;
            }

            return ((int)monthNum, (int)dayNum, (int)yearNum);
        }

        /// <summary>
        /// Calculates the year after creation
        /// </summary>
        public double CalculateYearAfterCreation(double gregorianYear)
        {
            if (gregorianYear < 0)
            {
                return gregorianYear + 4005; // BCE is 4005
            }
            else
            {
                return gregorianYear + 4004; // CE is 4004
            }
        }

        /// <summary>
        /// Formats a year string with CE or BCE suffix
        /// </summary>
        public string FormatYearString(double year, bool includeSuffix = true)
        {
            string yearStr = Math.Abs(year).ToString();
            if (includeSuffix)
            {
                if (year < 0)
                {
                    yearStr += " BCE";
                }
                else
                {
                    yearStr += " CE";
                }
            }
            return yearStr;
        }

        /// <summary>
        /// Initializes variables for calculations
        /// </summary>
        public void InitializeVariables()
        {
            DR = 0.01745329251993d;
            
            // Default to Jerusalem if no location is set
            if (LG == 0 && LT == 0)
            {
                LG = -35.244d; // Longitude of Jerusalem
                LT = 31.78d;   // Latitude of Jerusalem
                HR = 14;       // Hour location of Jerusalem GMT + 2
            }
        }
    }
}

