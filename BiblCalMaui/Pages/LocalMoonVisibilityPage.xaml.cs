using BiblCalCore;
using BiblCalMaui.Services;
using System.Collections.ObjectModel;

namespace BiblCalMaui.Pages
{
    public partial class LocalMoonVisibilityPage : ContentPage
    {
        private readonly BiblicalCalendarCalculator _calculator;
        private readonly LocalMoonCalculations _localMoonCalc;
        private readonly MauiOutputWriter _outputWriter;
        private readonly MauiUserDataProvider _userDataProvider;
        private readonly ObservableCollection<LocationItem> _locations;

        public class LocationItem
        {
            public string Name { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string GMTOffset { get; set; } = "0";
        }

        public LocalMoonVisibilityPage()
        {
            InitializeComponent();

            // Initialize services
            _outputWriter = new MauiOutputWriter();
            _userDataProvider = new MauiUserDataProvider();
            _calculator = new BiblicalCalendarCalculator(_outputWriter, _userDataProvider);
            _localMoonCalc = new LocalMoonCalculations(_outputWriter, _calculator);

            // Initialize locations
            _locations = new ObservableCollection<LocationItem>();
            LoadLocations();

            LocationPicker.ItemsSource = _locations;
            LatDirPicker.SelectedIndex = 0; // Default to N
            LongDirPicker.SelectedIndex = 0; // Default to E

            // Set default location if available
            if (_locations.Count > 0)
            {
                LocationPicker.SelectedItem = _locations[0];
                OnLocationSelected(null, null);
            }
        }

        private void LoadLocations()
        {
            _locations.Clear();

            int numLocations = _userDataProvider.GetNumberOfLocations();
            for (int i = 0; i < numLocations; i++)
            {
                string name = _userDataProvider.GetLocationName(i);
                if (!string.IsNullOrEmpty(name))
                {
                    _locations.Add(new LocationItem
                    {
                        Name = name,
                        Latitude = _userDataProvider.GetLocationLatitude(i),
                        Longitude = _userDataProvider.GetLocationLongitude(i),
                        GMTOffset = _userDataProvider.GetLocationGMTOffset(i)
                    });
                }
            }
        }

        private void OnLocationSelected(object? sender, EventArgs? e)
        {
            if (LocationPicker.SelectedItem is LocationItem selectedLocation)
            {
                // Windows app UI: txtLatDeg shows DegLat (latitude), txtLongDeg shows DegLon (longitude)
                // But the UI labels are confusing - we match Windows exactly:
                // LatDegEntry shows latitude, LongDegEntry shows longitude
                var (latDeg, latMin) = ConvertToDegreesMinutes(Math.Abs(selectedLocation.Latitude)); // Show latitude in Lat field
                var (longDeg, longMin) = ConvertToDegreesMinutes(Math.Abs(selectedLocation.Longitude)); // Show longitude in Long field

                LatDegEntry.Text = latDeg.ToString();
                LatMinEntry.Text = latMin.ToString();
                // Windows: txtLatDir = "N" if DegLat >= 0, "S" if DegLat < 0
                LatDirPicker.SelectedIndex = selectedLocation.Latitude >= 0 ? 0 : 1; // 0 = N, 1 = S

                LongDegEntry.Text = longDeg.ToString();
                LongMinEntry.Text = longMin.ToString();
                // Windows: txtLonDir = "E" if DegLon < 0, "W" if DegLon >= 0
                LongDirPicker.SelectedIndex = selectedLocation.Longitude < 0 ? 0 : 1; // 0 = E, 1 = W

                GMTOffsetEntry.Text = selectedLocation.GMTOffset;
            }
        }

        private (int degrees, int minutes) ConvertToDegreesMinutes(double decimalDegrees)
        {
            int degrees = (int)Math.Floor(Math.Abs(decimalDegrees));
            double minutesDecimal = (Math.Abs(decimalDegrees) - degrees) * 60;
            int minutes = (int)Math.Floor(minutesDecimal);
            return (degrees, minutes);
        }

        private double ConvertFromDegreesMinutes(int degrees, int minutes, string direction)
        {
            // Windows app's Degrees function just converts degrees+minutes to decimal (always positive)
            // Direction is applied separately in GetLocation
            return Math.Abs(degrees) + (Math.Abs(minutes) / 60.0);
        }

        private async void OnCalculateClicked(object? sender, EventArgs e)
        {
            try
            {
                // Disable button during calculation
                if (sender is Button btn)
                {
                    btn.IsEnabled = false;
                }

                _outputWriter.Clear();
                ResultsLabel.Text = "Calculating... This may take a few seconds.";

                // Allow UI to update
                await Task.Delay(50);

                // Get year
                if (string.IsNullOrWhiteSpace(YearEntry.Text))
                {
                    ResultsLabel.Text = "Please enter a year.";
                    if (sender is Button button)
                    {
                        button.IsEnabled = true;
                    }
                    return;
                }

                if (!double.TryParse(YearEntry.Text, out double year))
                {
                    ResultsLabel.Text = "Invalid year format. Please enter a number.";
                    if (sender is Button button)
                    {
                        button.IsEnabled = true;
                    }
                    return;
                }

                // Get location coordinates
                if (!int.TryParse(LatDegEntry.Text, out int latDeg) ||
                    !int.TryParse(LatMinEntry.Text, out int latMin) ||
                    !int.TryParse(LongDegEntry.Text, out int longDeg) ||
                    !int.TryParse(LongMinEntry.Text, out int longMin))
                {
                    ResultsLabel.Text = "Please enter valid latitude and longitude coordinates.";
                    if (sender is Button button)
                    {
                        button.IsEnabled = true;
                    }
                    return;
                }

                string latDir = LatDirPicker.SelectedItem?.ToString() ?? "N";
                string longDir = LongDirPicker.SelectedItem?.ToString() ?? "E";

                // Windows GetLocation: LG = Degrees(txtLongDeg, txtLongMin), LT = Degrees(txtLatDeg, txtLatMin)
                // txtLatDeg shows DegLat (latitude field, but actually longitude value)
                // txtLongDeg shows DegLon (longitude field, but actually latitude value)
                // Convert to decimal (Degrees function, always positive)
                double lg = ConvertFromDegreesMinutes(longDeg, longMin, longDir); // LG from txtLongDeg (DegLon = latitude)
                double lt = ConvertFromDegreesMinutes(latDeg, latMin, latDir);   // LT from txtLatDeg (DegLat = longitude)

                // Apply directions (matching Windows GetLocation exactly):
                // Windows GetLocation: if txtLonDir == "E": LG = -LG
                //                     if txtLatDir == "S": LT = -LT
                if (longDir == "E") // txtLonDir == "E" (East)
                {
                    lg = -lg;
                }
                // else longDir == "W", stays positive
                if (latDir == "S") // txtLatDir == "S" (South)
                {
                    lt = -lt;
                }
                // else latDir == "N", stays positive

                // Now lg = latitude value, lt = longitude value (matching Windows GetLocation output)
                // But Windows uses LG as latitude and LT as longitude in calculations
                // So we pass: latitude = lg, longitude = lt
                double latitude = lg;  // LG is latitude value
                double longitude = lt; // LT is longitude value

                // Get GMT offset
                if (!double.TryParse(GMTOffsetEntry.Text, out double gmtOffset))
                {
                    gmtOffset = 0;
                }

                // Calculate hour location (HR) - matching Windows GetLocation exactly
                // Windows GetLocation: HR = 12 + GMT, then if txtLonDir == "W": HR = 12 - GMT
                // txtLonDir is from "Longitude" UI field (which shows longitude)
                double hr = 12 + gmtOffset;
                if (longDir == "W") // txtLonDir == "W" (West)
                {
                    hr = 12 - gmtOffset;
                }
                else // txtLonDir == "E" (East)
                {
                    hr = 12 + gmtOffset;
                }

                // Calculate local moons using the calculation class (run on background thread)
                string locationName = LocationPicker.SelectedItem is LocationItem loc ? loc.Name : "Custom Location";

                // Run calculation on background thread to prevent UI blocking
                await Task.Run(() =>
                {
                    try
                    {
                        // Windows GetLocation produces: LG = latitude value (from txtLongDeg), LT = longitude value (from txtLatDeg)
                        // Function signature: CalculateLocalMoons(year, longitude, latitude, hr, locationName)
                        // Function sets: LG = longitude parameter, LT = latitude parameter
                        // To match Windows: We need LG = latitude, LT = longitude
                        // So: longitude parameter (goes to LG) = latitude value
                        //     latitude parameter (goes to LT) = longitude value
                        // Pass: (latitude_value, longitude_value) to match Windows GetLocation output
                        _localMoonCalc.CalculateLocalMoons(year, latitude, longitude, hr, locationName);
                    }
                    catch (Exception calcEx)
                    {
                        // Store error for display on UI thread
                        _outputWriter.Clear();
                        _outputWriter.WriteLine($"Calculation Error: {calcEx.Message}");
                        _outputWriter.WriteLine($"\nStack Trace:\n{calcEx.StackTrace}");
                        _outputWriter.WriteLine($"\nInner Exception: {calcEx.InnerException?.Message ?? "None"}");
                    }
                });

                // Display results on UI thread
                var output = _outputWriter.GetOutput();
                if (string.IsNullOrEmpty(output))
                {
                    ResultsLabel.Text = "No results generated.";
                }
                else
                {
                    ResultsLabel.Text = output;
                    ResultsLabel.InvalidateMeasure();
                }
            }
            catch (Exception ex)
            {
                ResultsLabel.Text = $"Error: {ex.Message}\n\nStack Trace:\n{ex.StackTrace}";
            }
            finally
            {
                // Re-enable button
                if (sender is Button btn)
                {
                    btn.IsEnabled = true;
                }
            }
        }

    }
}

