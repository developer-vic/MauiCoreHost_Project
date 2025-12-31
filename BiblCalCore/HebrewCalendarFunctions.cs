using System;

namespace BiblCalCore
{
    /// <summary>
    /// Hebrew (Rabbinic) calendar calculation functions
    /// Adapted from modHebrewCalendarFunctions.cs
    /// </summary>
    public static class HebrewCalendarFunctions
    {
        // Array for holding length of each Hebrew month
        public static int[] LOM = new int[14];
        
        // Array for holding names of types of Hebrew years
        public static string[] TypeYear = new string[] { "", "", "", "", "", "", "" };
        
        // Array for holding names of Hebrew months
        public static string[] HMonthName = new string[14];
        
        // Array containing flags for years being Common or Leap (0 = common, 1 = leap)
        public static byte[] YR = new byte[20];

        /// <summary>
        /// Loads Hebrew calendar variables and arrays
        /// </summary>
        public static void LoadHebrewVariables()
        {
            // Load up the table of month lengths for Common Deficient year and the 13th month
            LOM[1] = 30; // Nisan
            LOM[2] = 29; // Iyar
            LOM[3] = 30; // Sivan
            LOM[4] = 29; // Tammuz
            LOM[5] = 30; // Av
            LOM[6] = 29; // Elul
            LOM[7] = 30; // Tishri
            LOM[8] = 29; // Heshvan
            LOM[9] = 29; // Kislev
            LOM[10] = 29; // Tevet
            LOM[11] = 30; // Shevat
            LOM[12] = 29; // Adar
            LOM[13] = 29; // Vedar

            // Load up table of Hebrew Year types
            TypeYear[1] = "Common Deficient (353 days)";
            TypeYear[2] = "Common Regular (354 days)";
            TypeYear[3] = "Common Complete (355 days)";
            TypeYear[4] = "Embolismic Deficient (383 days)";
            TypeYear[5] = "Embolismic Regular (384 days)";
            TypeYear[6] = "Embolismic Complete (385 days)";

            // Load up table of Hebrew month names
            HMonthName[1] = "Nisan";
            HMonthName[2] = "Iyar";
            HMonthName[3] = "Sivan";
            HMonthName[4] = "Tammuz";
            HMonthName[5] = "AV";
            HMonthName[6] = "Elul";
            HMonthName[7] = "Tishri";
            HMonthName[8] = "Heshvan";
            HMonthName[9] = "Kislev";
            HMonthName[10] = "Tevet";
            HMonthName[11] = "Shevat";
            HMonthName[12] = "Adar";
            HMonthName[13] = "Vedar";

            // Load up array with flags of what years in the 19 year cycle are leap years
            YR[1] = 0;
            YR[2] = 0;
            YR[3] = 1; // Leap
            YR[4] = 0;
            YR[5] = 0;
            YR[6] = 1; // Leap
            YR[7] = 0;
            YR[8] = 1; // Leap
            YR[9] = 0;
            YR[10] = 0;
            YR[11] = 1; // Leap
            YR[12] = 0;
            YR[13] = 0;
            YR[14] = 1; // Leap
            YR[15] = 0;
            YR[16] = 0;
            YR[17] = 1; // Leap
            YR[18] = 0;
            YR[19] = 1; // Leap
        }

        /// <summary>
        /// Finds the Julian Day count for the 1st of Tishri for the given Gregorian Year Number
        /// Algorithm from Astronomical Algorithms Second Edition by Jean Meeus pages 71 and 72
        /// </summary>
        public static double JD1stOfTishri(int yearNum)
        {
            double x = yearNum;
            
            // Fix for year zero
            if (x <= 0)
            {
                x++;
            }

            double C = Math.Floor(x / 100d);
            double S = Math.Floor(((3 * C) - 5) / 4d);
            double A = x + 3760;

            double lca = (int)Math.Floor((12 * x) + 12) % 19;
            if (lca < 0)
            {
                lca += 19;
            }

            double lcb = (int)x % 4;
            if (lcb < 0)
            {
                lcb += 4;
            }

            double Q = -1.904412361576d + (1.554241796621d * lca) + (0.25d * lcb) - (0.003177794022d * x) + S;
            double j = (int)Math.Floor(Math.Floor(Q) + (3 * x) + (5 * lcb) + 2 - S) % 7;
            if (j < 0)
            {
                j += 7;
            }

            double r = Q - Math.Floor(Q);

            // Postponement rules
            double D = Math.Floor(Q) + 22;
            if (j == 2 || j == 4 || j == 6)
            {
                D = Math.Floor(Q) + 23; // Wed, Fri, or Sun
            }
            if (j == 1 && lca > 6 && r >= 0.63287037d)
            {
                D = Math.Floor(Q) + 24; // Tues, common, >= 9H 204P
            }
            if (j == 0 && lca > 11 && r >= 0.897723765d)
            {
                D = Math.Floor(Q) + 23; // Mon, leap, >= 15H 589P
            }

            // Check to see if month is still March if not then advance month
            int monthNum = 3; // Start with March
            if (D > 31)
            {
                D -= 31;
                monthNum++;
                if (D > 30)
                {
                    D -= 30;
                    monthNum++;
                }
            }

            // Convert to Julian Day
            double jd = ConvertToJulian2(monthNum, D, yearNum);
            jd += 163; // Get Julian day count for 1st of Tishri
            return jd;
        }

        /// <summary>
        /// Convert from Gregorian date to Julian day number
        /// The Jean Meeus formula 7.1 pages 60-61 of Astronomical Algorithms Second Edition
        /// </summary>
        private static double ConvertToJulian2(int monthNum, double dayNum, double yearNum)
        {
            if (yearNum <= 0)
            {
                yearNum++;
            }

            if (monthNum < 3)
            {
                yearNum--;
                monthNum += 12;
            }

            double A = Math.Floor(yearNum / 100d);
            double B = Math.Floor(2 - A + Math.Floor(A / 4d));
            double result = Math.Floor(365.25d * (yearNum + 4716)) + Math.Floor(30.6001d * (monthNum + 1)) + dayNum + B - 1524.5d;
            return result;
        }

        /// <summary>
        /// Returns the length of the Hebrew year given Gregorian Year Number
        /// </summary>
        public static int LengthOfYear(int yearNum)
        {
            double thisYear = JD1stOfTishri(yearNum);
            double nextYear = JD1stOfTishri(yearNum + 1);
            return (int)(nextYear - thisYear);
        }

        /// <summary>
        /// Determines if it is a Hebrew Leap Year
        /// </summary>
        public static bool HebrewLeapYear(double iYear)
        {
            int modResult = (int)((int)((7 * iYear) + 1) % 19);
            if (modResult < 0)
            {
                modResult += 19;
            }
            return modResult < 7;
        }
    }
}

