using BiblCalCore;
using BiblCalMaui.Services;

namespace BiblCalMaui;

public partial class MainPage : ContentPage
{
	private readonly BiblicalCalendarCalculator _calculator;
	private readonly MauiOutputWriter _outputWriter;
	private readonly MauiUserDataProvider _userDataProvider;
	private readonly FloodCalculations _floodCalculations;
	private readonly GolgothaCalculations _golgothaCalculations;
	private readonly SunsetCalculations _sunsetCalculations;
	private readonly TimesCalculations _timesCalculations;

	public MainPage()
	{
		InitializeComponent();
		
		// Initialize services
		_outputWriter = new MauiOutputWriter();
		_userDataProvider = new MauiUserDataProvider();
		_calculator = new BiblicalCalendarCalculator(_outputWriter, _userDataProvider);
		
		// Initialize calculation modules
		_floodCalculations = new FloodCalculations(_outputWriter, _calculator);
		_golgothaCalculations = new GolgothaCalculations(_outputWriter, _calculator);
		_sunsetCalculations = new SunsetCalculations(_outputWriter, _calculator);
		_timesCalculations = new TimesCalculations(_outputWriter, _calculator);
		
		// Initialize Hebrew calendar variables
		HebrewCalendarFunctions.LoadHebrewVariables();
		
		// Set initial text to ensure Label is properly initialized
		ResultsLabel.Text = "Enter a year and click Calculate to see results.\n\nTip: Enter 'test' to run library tests.\nEnter 'help' to see available commands.";
	}

	private async void OnCalculateClicked(object? sender, EventArgs e)
	{
		try
		{
			// Clear previous results
			_outputWriter.Clear();
			ResultsLabel.Text = "Calculating...";
			
			// Force UI update
			await Task.Delay(10);
			
			string input = YearEntry.Text?.Trim() ?? "";
			
			// Check for special commands
			if (input.ToLower() == "test")
			{
				var testResults = BiblCalCore.BiblCalCoreTests.RunBasicTests();
				ResultsLabel.Text = testResults;
				return;
			}
			
			if (input.ToLower() == "help")
			{
				ShowHelp();
				return;
			}
			
			if (string.IsNullOrWhiteSpace(input))
			{
				ResultsLabel.Text = "Please enter a year.\n\nTip: Enter 'test' to run library tests.\nEnter 'help' to see available commands.";
				return;
			}

			if (!double.TryParse(input, out double year))
			{
				ResultsLabel.Text = "Invalid year format. Please enter a number.\n\nTip: Enter 'test' to run library tests.\nEnter 'help' to see available commands.";
				return;
			}

			_calculator.GregorianYear = year;
			_calculator.InitializeVariables();

			// Calculate year after creation
			double yearAfterCreation = _calculator.CalculateYearAfterCreation(year);
			string yearString = _calculator.FormatYearString(year);

			_outputWriter.WriteLine($"Year: {yearString}");
			_outputWriter.WriteLine($"Year After Creation: {yearAfterCreation:F0}");
			_outputWriter.WriteLine("");

			// Calculate Hebrew calendar for this year
			int intYear = (int)year;
			if (intYear != 0) // Avoid year zero
			{
				double tishriJD = HebrewCalendarFunctions.JD1stOfTishri(intYear);
				int yearLength = HebrewCalendarFunctions.LengthOfYear(intYear);
				bool isLeapYear = HebrewCalendarFunctions.HebrewLeapYear(intYear + 3760);

				_outputWriter.WriteLine("Hebrew Calendar Information:");
				_outputWriter.WriteLine($"1st of Tishri Julian Day: {tishriJD:F2}");
				_outputWriter.WriteLine($"Year Length: {yearLength} days");
				_outputWriter.WriteLine($"Is Leap Year: {isLeapYear}");
				_outputWriter.WriteLine("");

				// Convert Julian Day to Gregorian
				var (month, day, gregYear) = _calculator.JulianToGregorian(tishriJD);
				_outputWriter.WriteLine($"1st of Tishri (Gregorian): {month}/{day}/{gregYear}");
			}

			// Display results - force property change for macOS
			var output = _outputWriter.GetOutput();
			if (string.IsNullOrEmpty(output))
			{
				ResultsLabel.Text = "No results generated.";
			}
			else
			{
				// Explicitly set the text property to ensure UI updates on macOS
				ResultsLabel.Text = output;
				// Force layout update
				ResultsLabel.InvalidateMeasure();
			}
		}
		catch (Exception ex)
		{
			ResultsLabel.Text = $"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
		}
	}

	private void ShowHelp()
	{
		_outputWriter.Clear();
		_outputWriter.WriteLine("=== BiblCal Core Library - Available Modules ===");
		_outputWriter.WriteLine("");
		_outputWriter.WriteLine("All calculation modules are available:");
		_outputWriter.WriteLine("");
		_outputWriter.WriteLine("1. BiblicalCalendarCalculator - Core calendar functions");
		_outputWriter.WriteLine("2. HebrewCalendarFunctions - Hebrew calendar calculations");
		_outputWriter.WriteLine("3. FloodCalculations - Flood date calculations");
		_outputWriter.WriteLine("4. GolgothaCalculations - Golgotha/Jordan/Creation dates");
		_outputWriter.WriteLine("5. SunsetCalculations - Sunset time calculations");
		_outputWriter.WriteLine("6. TimesCalculations - Sun/Moon rise/set calculations");
		_outputWriter.WriteLine("7. Documentation - Help text and documentation");
		_outputWriter.WriteLine("");
		_outputWriter.WriteLine("For detailed usage, see the README.md file.");
		_outputWriter.WriteLine("");
		_outputWriter.WriteLine("Enter a year to calculate basic calendar information.");
		_outputWriter.WriteLine("Enter 'test' to run library tests.");
		
		ResultsLabel.Text = _outputWriter.GetOutput();
	}
}
