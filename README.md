# BiblCal MAUI Project

This project contains a .NET MAUI application that uses a shared Core library for Biblical calendar calculations. The Core library has been extracted from the original BiblCal Windows Forms application and adapted for cross-platform use.

## Project Structure

- **BiblCalCore/** - Shared class library containing non-UI business logic
  - Targets .NET Standard 2.1 for maximum compatibility
  - Contains ALL calculation functions from the original codebase
  - Uses abstraction interfaces to avoid platform-specific dependencies

- **BiblCalMaui/** - .NET MAUI application
  - Targets iOS and macOS (Mac Catalyst)
  - References the BiblCalCore library
  - Contains platform-specific UI implementations

## Implementation Status

### Completed ✅

1. **Complete Core Library**
   - Created `BiblCalCore` class library targeting .NET Standard 2.1
   - Migrated ALL modules from the original Windows application:
     - `BiblicalCalendarCalculator` - Core calendar functions
     - `HebrewCalendarFunctions` - Hebrew calendar calculations
     - `FloodCalculations` - Flood date calculations
     - `GolgothaCalculations` - Golgotha/Jordan/Creation calculations
     - `SunsetCalculations` - Sunset time calculations
     - `TimesCalculations` - Sun/Moon rise/set calculations
     - `Documentation` - Help text and documentation
   - Removed all Windows-specific dependencies (Win32, System.Drawing, System.Media, Windows.Forms)

2. **Abstraction Interfaces**
   - `IOutputWriter` - Interface for output operations (replaces TPrint)
   - `IUserDataProvider` - Interface for user data operations (replaces file I/O)

3. **MAUI Application**
   - Updated to reference BiblCalCore library
   - Created MAUI implementations of abstraction interfaces
   - UI demonstrating Core library usage
   - Successfully builds for iOS and macOS (Mac Catalyst)

### Available Functionality

The implementation provides complete functionality:

- **Julian Day Number conversions** - Convert between Gregorian dates and Julian Day Numbers
- **Hebrew calendar calculations** - Calculate Hebrew calendar dates (1st of Tishri, year length, leap years)
- **Year calculations** - Calculate "Year After Creation" from Gregorian years
- **Flood calculations** - Calculate 150 days between specific dates for Flood analysis
- **Golgotha calculations** - Calculate possible dates for Jesus' crucifixion
- **Jordan Crossing calculations** - Calculate dates for Jordan crossing
- **Creation date calculations** - Calculate possible Creation dates
- **Sunset calculations** - Calculate sunset times for locations
- **Times calculations** - Calculate sunrise/sunset, moonrise/moonset, moon illumination
- **Documentation** - Complete help text for all modules

## Building and Running

### Prerequisites

- .NET 9.0 SDK
- For iOS: Xcode and iOS Simulator
- For macOS: Xcode and Mac development tools

### Build Commands

```bash
# Build Core library
dotnet build BiblCalCore/BiblCalCore.csproj

# Build for iOS
dotnet build BiblCalMaui/BiblCalMaui.csproj -f net9.0-ios

# Build for macOS (Mac Catalyst)
dotnet build BiblCalMaui/BiblCalMaui.csproj -f net9.0-maccatalyst
```

### Running

```bash
# Run on iOS Simulator
dotnet build BiblCalMaui/BiblCalMaui.csproj -f net9.0-ios
# Then run from Xcode or Visual Studio

# Run on macOS
dotnet build BiblCalMaui/BiblCalMaui.csproj -f net9.0-maccatalyst
# Then run from Xcode or Visual Studio
```

## How to Use BiblCalCore Library in Your .NET MAUI App

### Step 1: Add Project Reference

Add a reference to the BiblCalCore project in your MAUI app's `.csproj` file:

```xml
<ItemGroup>
  <ProjectReference Include="..\BiblCalCore\BiblCalCore.csproj" />
</ItemGroup>
```

### Step 2: Implement Required Interfaces

The library requires two interfaces. Create implementations in your MAUI app:

#### IOutputWriter Implementation

```csharp
using BiblCalCore;
using System.Text;

namespace YourApp.Services
{
    public class MauiOutputWriter : IOutputWriter
    {
        private readonly StringBuilder _output = new StringBuilder();

        public void Write(string text)
        {
            _output.Append(text);
        }

        public void WriteLine(string text)
        {
            _output.AppendLine(text);
        }

        public void Clear()
        {
            _output.Clear();
        }

        public string GetOutput()
        {
            return _output.ToString();
        }
    }
}
```

#### IUserDataProvider Implementation

```csharp
using BiblCalCore;

namespace YourApp.Services
{
    public class MauiUserDataProvider : IUserDataProvider
    {
        private string _currentLocation = "Jerusalem, Israel";

        public string GetCurrentLocation() => _currentLocation;
        public void SetCurrentLocation(string location) => _currentLocation = location;
        public int GetNumberOfLocations() => 1;
        public string GetLocationName(int index) => "Jerusalem";
        public double GetLocationLatitude(int index) => 31.78;
        public double GetLocationLongitude(int index) => -35.24;
        public string GetLocationGMTOffset(int index) => "2";
        public void SaveUserData() { /* Implement persistence if needed */ }
    }
}
```

### Step 3: Initialize and Use the Calculator

In your page or view model:

```csharp
using BiblCalCore;
using YourApp.Services;

public partial class YourPage : ContentPage
{
    private readonly BiblicalCalendarCalculator _calculator;
    private readonly MauiOutputWriter _outputWriter;
    private readonly FloodCalculations _floodCalculations;
    private readonly GolgothaCalculations _golgothaCalculations;

    public YourPage()
    {
        InitializeComponent();
        
        // Initialize services
        _outputWriter = new MauiOutputWriter();
        var userDataProvider = new MauiUserDataProvider();
        _calculator = new BiblicalCalendarCalculator(_outputWriter, userDataProvider);
        
        // Initialize calculation modules
        _floodCalculations = new FloodCalculations(_outputWriter, _calculator);
        _golgothaCalculations = new GolgothaCalculations(_outputWriter, _calculator);
        
        // Initialize Hebrew calendar (required once)
        HebrewCalendarFunctions.LoadHebrewVariables();
    }

    private void CalculateButton_Clicked(object sender, EventArgs e)
    {
        _outputWriter.Clear();
        
        // Set the year
        _calculator.GregorianYear = 2024;
        _calculator.InitializeVariables();

        // Calculate year after creation
        double yearAfterCreation = _calculator.CalculateYearAfterCreation(2024);
        string yearString = _calculator.FormatYearString(2024);

        _outputWriter.WriteLine($"Year: {yearString}");
        _outputWriter.WriteLine($"Year After Creation: {yearAfterCreation:F0}");

        // Calculate Hebrew calendar
        int year = 2024;
        double tishriJD = HebrewCalendarFunctions.JD1stOfTishri(year);
        int yearLength = HebrewCalendarFunctions.LengthOfYear(year);
        bool isLeapYear = HebrewCalendarFunctions.HebrewLeapYear(year + 3760);

        _outputWriter.WriteLine($"1st of Tishri Julian Day: {tishriJD:F2}");
        _outputWriter.WriteLine($"Year Length: {yearLength} days");
        _outputWriter.WriteLine($"Is Leap Year: {isLeapYear}");

        // Convert Julian Day to Gregorian
        var (month, day, gregYear) = _calculator.JulianToGregorian(tishriJD);
        _outputWriter.WriteLine($"1st of Tishri (Gregorian): {month}/{day}/{gregYear}");

        // Get results
        string results = _outputWriter.GetOutput();
        // Display results in your UI
    }
}
```

### Step 4: Available Functions

#### BiblicalCalendarCalculator
- `ConvertToJulian(month, day, year)` - Convert Gregorian to Julian Day Number
- `JulianToGregorian(jd)` - Convert Julian Day Number to Gregorian date
- `CalculateYearAfterCreation(year)` - Calculate year after creation
- `FormatYearString(year, includeSuffix)` - Format year with CE/BCE suffix
- `InitializeVariables()` - Initialize calculation variables

#### HebrewCalendarFunctions
- `LoadHebrewVariables()` - Initialize Hebrew calendar data (call once)
- `JD1stOfTishri(year)` - Get Julian Day for 1st of Tishri
- `LengthOfYear(year)` - Get Hebrew year length in days
- `HebrewLeapYear(hebrewYear)` - Check if Hebrew year is a leap year

#### FloodCalculations
- `CalculateFloodDates(startYear, endYear?)` - Calculate flood dates for year range

#### GolgothaCalculations
- `CalculateGolgothaDates(startYear, endYear?)` - Calculate possible crucifixion dates
- `CalculateJordanCrossingDates(startYear, endYear?)` - Calculate Jordan crossing dates
- `CalculateCreationDates(startYear, endYear?)` - Calculate possible Creation dates

#### SunsetCalculations
- `CalculateSunsets(year, longitude, latitude, gmtOffset)` - Calculate sunset times

#### TimesCalculations
- `CalculateTimes(year, longitude, latitude, gmtOffset, useBiblicalYear)` - Calculate sun/moon times

#### Documentation
- `GetDocumentation(mode)` - Get help text for specific module

### Quick Example

```csharp
// Initialize (do this once)
var outputWriter = new MauiOutputWriter();
var userDataProvider = new MauiUserDataProvider();
var calculator = new BiblicalCalendarCalculator(outputWriter, userDataProvider);
HebrewCalendarFunctions.LoadHebrewVariables();

// Use
calculator.GregorianYear = 2024;
double jd = calculator.ConvertToJulian(1, 1, 2024);
var (month, day, year) = calculator.JulianToGregorian(jd);
double yearAfterCreation = calculator.CalculateYearAfterCreation(2024);
```

## Notes

- **Platform Agnostic**: The Core library works on iOS, Android, macOS, Windows, and any .NET platform
- **No Windows Dependencies**: All Windows-specific code has been removed
- **Thread Safety**: The calculator is not thread-safe; use on UI thread or add synchronization
- **Initialization**: Call `HebrewCalendarFunctions.LoadHebrewVariables()` once before using Hebrew calendar functions
- The original codebase used `Microsoft.VisualBasic` and `UpgradeHelpers` libraries which are Windows-specific. These have been replaced with standard .NET equivalents.
- The Core library is designed to be platform-agnostic and can be used by any .NET platform that supports .NET Standard 2.1.
- The abstraction interfaces allow the Core library to remain independent of UI frameworks.

## Troubleshooting

**Build Error: "Cannot find BiblCalCore"**
- Verify the project reference path is correct
- Ensure BiblCalCore builds successfully: `dotnet build BiblCalCore/BiblCalCore.csproj`

**Runtime Error: "Interface not implemented"**
- Ensure you've implemented both `IOutputWriter` and `IUserDataProvider`
- Check that implementations are in the correct namespace

**Hebrew Calendar Functions Not Working**
- Ensure you've called `HebrewCalendarFunctions.LoadHebrewVariables()` before using Hebrew functions

## Original Codebase

The original BiblCal codebase is located in the `BiblCal-master/` folder and remains unchanged as requested. This MAUI implementation uses a copy/adaptation of the non-UI logic.

