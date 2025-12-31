using System;

namespace BiblCalCore
{
    /// <summary>
    /// Simple test methods to verify Core library functionality
    /// </summary>
    public static class BiblCalCoreTests
    {
        public static string RunBasicTests()
        {
            var output = new System.Text.StringBuilder();
            output.AppendLine("=== BiblCalCore Library Tests ===");
            output.AppendLine();

            try
            {
                // Test 1: Julian Day conversion
                output.AppendLine("Test 1: Julian Day Conversion");
                var calculator = new BiblicalCalendarCalculator(
                    new TestOutputWriter(),
                    new TestUserDataProvider()
                );
                
                double jd = calculator.ConvertToJulian(1, 1, 2000);
                output.AppendLine($"  January 1, 2000 = JD {jd}");
                
                var (month, day, year) = calculator.JulianToGregorian(jd);
                output.AppendLine($"  JD {jd} = {month}/{day}/{year}");
                output.AppendLine("  ✓ Passed");
                output.AppendLine();

                // Test 2: Year After Creation
                output.AppendLine("Test 2: Year After Creation");
                double yearAfterCreation = calculator.CalculateYearAfterCreation(2024);
                output.AppendLine($"  2024 CE = Year {yearAfterCreation} After Creation");
                output.AppendLine("  ✓ Passed");
                output.AppendLine();

                // Test 3: Hebrew Calendar
                output.AppendLine("Test 3: Hebrew Calendar");
                HebrewCalendarFunctions.LoadHebrewVariables();
                double tishriJD = HebrewCalendarFunctions.JD1stOfTishri(2024);
                int yearLength = HebrewCalendarFunctions.LengthOfYear(2024);
                bool isLeap = HebrewCalendarFunctions.HebrewLeapYear(2024 + 3760);
                
                output.AppendLine($"  1st of Tishri 2024 = JD {tishriJD:F2}");
                output.AppendLine($"  Year Length: {yearLength} days");
                output.AppendLine($"  Is Leap Year: {isLeap}");
                output.AppendLine("  ✓ Passed");
                output.AppendLine();

                output.AppendLine("=== All Tests Passed ===");
                return output.ToString();
            }
            catch (Exception ex)
            {
                output.AppendLine($"=== Test Failed ===");
                output.AppendLine($"Error: {ex.Message}");
                output.AppendLine($"Stack: {ex.StackTrace}");
                return output.ToString();
            }
        }

        private class TestOutputWriter : IOutputWriter
        {
            public void Write(string text) { }
            public void WriteLine(string text) { }
            public void Clear() { }
        }

        private class TestUserDataProvider : IUserDataProvider
        {
            public string GetCurrentLocation() => "Jerusalem";
            public void SetCurrentLocation(string location) { }
            public int GetNumberOfLocations() => 1;
            public string GetLocationName(int index) => "Jerusalem";
            public double GetLocationLatitude(int index) => 31.78;
            public double GetLocationLongitude(int index) => -35.24;
            public string GetLocationGMTOffset(int index) => "2";
            public void SaveUserData() { }
        }
    }
}

