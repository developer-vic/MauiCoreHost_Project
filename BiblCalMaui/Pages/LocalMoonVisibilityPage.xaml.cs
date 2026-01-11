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
        private bool _changeFlag = false; // Track if coordinates have been changed
        private string _lastLocationName = ""; // Track last selected location name
        private bool _isSelectingFromDropdown = false; // Prevent text change events during dropdown selection

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
            
            // Set up CollectionView ItemsSource to show all locations
            if (LocationDropdownList != null)
            {
                LocationDropdownList.ItemsSource = _locations;
            }

            LatDirPicker.SelectedIndex = 0; // Default to N
            LongDirPicker.SelectedIndex = 0; // Default to E

            // Set default year to current year
            YearEntry.Text = DateTime.Now.Year.ToString();

            // Set default location if available
            if (_locations.Count > 0)
            {
                LocationEntry.Text = _locations[0].Name;
                _lastLocationName = _locations[0].Name;
                SetupLocation(_locations[0]);
            }

            // Track coordinate changes
            LatDegEntry.TextChanged += OnCoordinateChanged;
            LatMinEntry.TextChanged += OnCoordinateChanged;
            LatDirPicker.SelectedIndexChanged += OnCoordinateChanged;
            LongDegEntry.TextChanged += OnCoordinateChanged;
            LongMinEntry.TextChanged += OnCoordinateChanged;
            LongDirPicker.SelectedIndexChanged += OnCoordinateChanged;
            GMTOffsetEntry.TextChanged += OnCoordinateChanged;
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
            
            // Refresh CollectionView if dropdown is visible
            if (LocationDropdownList != null && LocationDropdownList.ItemsSource != _locations)
            {
                LocationDropdownList.ItemsSource = _locations;
            }
        }

        private void SetupLocation(LocationItem selectedLocation)
        {
            // Temporarily disable change tracking
            bool wasTracking = _changeFlag;
            _changeFlag = false;

            // Windows app UI: txtLatDeg shows DegLat (latitude), txtLongDeg shows DegLon (longitude)
            var (latDeg, latMin) = ConvertToDegreesMinutes(Math.Abs(selectedLocation.Latitude));
            var (longDeg, longMin) = ConvertToDegreesMinutes(Math.Abs(selectedLocation.Longitude));

            LatDegEntry.Text = latDeg.ToString();
            LatMinEntry.Text = latMin.ToString();
            LatDirPicker.SelectedIndex = selectedLocation.Latitude >= 0 ? 0 : 1; // 0 = N, 1 = S

            LongDegEntry.Text = longDeg.ToString();
            LongMinEntry.Text = longMin.ToString();
            LongDirPicker.SelectedIndex = selectedLocation.Longitude < 0 ? 0 : 1; // 0 = E, 1 = W

            GMTOffsetEntry.Text = selectedLocation.GMTOffset;

            // Restore change tracking
            _changeFlag = wasTracking;
        }

        private void OnCoordinateChanged(object? sender, EventArgs? e)
        {
            _changeFlag = true;
        }

        private void OnLocationEntryTextChanged(object? sender, TextChangedEventArgs e)
        {
            if (_isSelectingFromDropdown)
            {
                return; // Don't process while selecting from dropdown
            }

            // Limit to 40 characters like Windows app
            if (e.NewTextValue != null && e.NewTextValue.Length > 40)
            {
                LocationEntry.Text = e.NewTextValue.Substring(0, 40);
            }
        }

        private void OnLocationEntryFocused(object? sender, FocusEventArgs e)
        {
            try
            {
                // Show dropdown with all locations when entry is focused
                if (LocationEntry != null && LocationDropdown != null && LocationDropdownList != null)
                {
                    // Ensure ItemsSource is set to show all locations
                    if (LocationDropdownList.ItemsSource == null)
                    {
                        LocationDropdownList.ItemsSource = _locations;
                    }
                    else if (LocationDropdownList.ItemsSource != _locations)
                    {
                        LocationDropdownList.ItemsSource = _locations;
                    }
                    
                    // Position dropdown to match entry width and align with it
                    if (LocationEntry != null && LocationDropdown != null)
                    {
                        // Use a small delay to ensure layout is complete
                        Task.Delay(50).ContinueWith(_ =>
                        {
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                try
                                {
                                    if (LocationEntry != null && LocationDropdown != null)
                                    {
                                        // Match the entry's width
                                        if (LocationEntry.Width > 0)
                                        {
                                            LocationDropdown.WidthRequest = LocationEntry.Width;
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    System.Diagnostics.Debug.WriteLine($"Error positioning dropdown: {ex.Message}");
                                }
                            });
                        });
                    }
                    
                    // Show the dropdown
                    LocationDropdown.IsVisible = _locations.Count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLocationEntryFocused: {ex.Message}");
            }
        }

        private void OnLocationEntryUnfocused(object? sender, FocusEventArgs e)
        {
            // Hide dropdown when entry loses focus (with small delay to allow item selection)
            if (LocationEntry != null && LocationDropdown != null)
            {
                Task.Delay(200).ContinueWith(_ =>
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (LocationEntry != null && LocationDropdown != null && !LocationEntry.IsFocused)
                        {
                            LocationDropdown.IsVisible = false;
                        }
                    });
                });
            }
        }

        private void OnLocationDropdownItemSelected(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (e.CurrentSelection != null && e.CurrentSelection.Count > 0 && e.CurrentSelection[0] is LocationItem selectedLocation)
                {
                    _isSelectingFromDropdown = true;
                    
                    // Set the location name in the entry
                    if (LocationEntry != null)
                    {
                        LocationEntry.Text = selectedLocation.Name;
                    }
                    _lastLocationName = selectedLocation.Name;
                    
                    // Load the location coordinates
                    SetupLocation(selectedLocation);
                    
                    // Hide dropdown
                    if (LocationDropdown != null)
                    {
                        LocationDropdown.IsVisible = false;
                    }
                    if (LocationEntry != null)
                    {
                        LocationEntry.Unfocus();
                    }
                    
                    _isSelectingFromDropdown = false;
                    
                    // Clear selection to allow re-selecting the same item
                    if (LocationDropdownList != null)
                    {
                        LocationDropdownList.SelectedItem = null;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in OnLocationDropdownItemSelected: {ex.Message}");
                _isSelectingFromDropdown = false;
            }
        }

        private async void OnLocationEntryCompleted(object? sender, EventArgs e)
        {
            // Hide dropdown when ENTER is pressed
            if (LocationDropdown != null)
            {
                LocationDropdown.IsVisible = false;
            }
            await AddEditDeleteLocation();
        }

        private async Task AddEditDeleteLocation()
        {
            string locationName = LocationEntry.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(locationName))
            {
                return;
            }

            // Limit to 40 characters
            if (locationName.Length > 40)
            {
                locationName = locationName.Substring(0, 40);
                LocationEntry.Text = locationName;
            }

            // Find location in list
            int index = _userDataProvider.FindLocationIndex(locationName);
            bool found = index >= 0;

            if (!found)
            {
                // Location not found - ask to add
                bool add = await DisplayAlert("Add Location", 
                    "Do you wish to add this location?\nSelecting [Yes] will add the location to the list.", 
                    "Yes", "No");
                
                if (add)
                {
                    try
                    {
                        // Get coordinates from UI
                        if (!int.TryParse(LatDegEntry.Text, out int latDeg) ||
                            !int.TryParse(LatMinEntry.Text, out int latMin) ||
                            !int.TryParse(LongDegEntry.Text, out int longDeg) ||
                            !int.TryParse(LongMinEntry.Text, out int longMin))
                        {
                            await DisplayAlert("Error", "Please enter valid coordinates before adding a location.", "OK");
                            return;
                        }

                        string latDir = LatDirPicker.SelectedItem?.ToString() ?? "N";
                        string longDir = LongDirPicker.SelectedItem?.ToString() ?? "E";
                        string gmt = GMTOffsetEntry.Text ?? "0";

                        // Convert to decimal degrees (matching Windows Degrees function)
                        double latitude = ConvertFromDegreesMinutes(latDeg, latMin, latDir);
                        if (latDir == "S") latitude = -latitude;

                        double longitude = ConvertFromDegreesMinutes(longDeg, longMin, longDir);
                        if (longDir == "E") longitude = -longitude;

                        _userDataProvider.AddLocation(locationName, latitude, longitude, gmt);
                        LoadLocations();
                        _userDataProvider.SetCurrentLocation(locationName);
                        _lastLocationName = locationName;
                        _changeFlag = false;
                        await DisplayAlert("Success", "Location added successfully.", "OK");
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                }
            }
            else
            {
                if (_changeFlag)
                {
                    // Location found and coordinates changed - ask to edit
                    bool edit = await DisplayAlert("Edit Location", 
                        "Do you wish to edit this location?\nSelecting [Yes] will change the location information.", 
                        "Yes", "No");
                    
                    if (edit)
                    {
                        // Get coordinates from UI
                        if (!int.TryParse(LatDegEntry.Text, out int latDeg) ||
                            !int.TryParse(LatMinEntry.Text, out int latMin) ||
                            !int.TryParse(LongDegEntry.Text, out int longDeg) ||
                            !int.TryParse(LongMinEntry.Text, out int longMin))
                        {
                            await DisplayAlert("Error", "Please enter valid coordinates.", "OK");
                            return;
                        }

                        string latDir = LatDirPicker.SelectedItem?.ToString() ?? "N";
                        string longDir = LongDirPicker.SelectedItem?.ToString() ?? "E";
                        string gmt = GMTOffsetEntry.Text ?? "0";

                        // Convert to decimal degrees
                        double latitude = ConvertFromDegreesMinutes(latDeg, latMin, latDir);
                        if (latDir == "S") latitude = -latitude;

                        double longitude = ConvertFromDegreesMinutes(longDeg, longMin, longDir);
                        if (longDir == "E") longitude = -longitude;

                        _userDataProvider.UpdateLocation(index, locationName, latitude, longitude, gmt);
                        LoadLocations();
                        _userDataProvider.SetCurrentLocation(locationName);
                        _lastLocationName = locationName;
                        _changeFlag = false;
                        await DisplayAlert("Success", "Location updated successfully.", "OK");
                    }
                    else
                    {
                        _changeFlag = false;
                    }
                }
                else
                {
                    // Location found and no changes - ask to delete
                    bool delete = await DisplayAlert("Delete Location", 
                        "Do you wish to delete this location?", 
                        "Yes", "No");
                    
                    if (delete)
                    {
                        _userDataProvider.DeleteLocation(index);
                        LoadLocations();
                        if (_locations.Count > 0)
                        {
                            LocationEntry.Text = _locations[0].Name;
                            _lastLocationName = _locations[0].Name;
                            SetupLocation(_locations[0]);
                        }
                        else
                        {
                            LocationEntry.Text = "";
                            _lastLocationName = "";
                        }
                        await DisplayAlert("Success", "Location deleted successfully.", "OK");
                    }
                }
            }
        }

        private (int degrees, int minutes) ConvertToDegreesMinutes(double decimalDegrees)
        {
            int degrees = (int)Math.Floor(Math.Abs(decimalDegrees));
            double minutesDecimal = (Math.Abs(decimalDegrees) - degrees) * 60;
            // Round to nearest minute for display accuracy
            int minutes = (int)Math.Round(minutesDecimal, MidpointRounding.AwayFromZero);
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
            ImageButton? calculateButton = sender as ImageButton;
            
            try
            {
                // Disable button during calculation
                if (calculateButton != null)
                {
                    calculateButton.IsEnabled = false;
                }

                _outputWriter.Clear();
                ResultsLabel.Text = "Calculating... This may take a few seconds.";

                // Allow UI to update
                await Task.Delay(50);

                // Get year
                if (string.IsNullOrWhiteSpace(YearEntry.Text))
                {
                    ResultsLabel.Text = "Please enter a year.";
                    if (calculateButton != null)
                    {
                        calculateButton.IsEnabled = true;
                    }
                    return;
                }

                if (!double.TryParse(YearEntry.Text, out double year))
                {
                    ResultsLabel.Text = "Invalid year format. Please enter a number.";
                    if (calculateButton != null)
                    {
                        calculateButton.IsEnabled = true;
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
                    if (calculateButton != null)
                    {
                        calculateButton.IsEnabled = true;
                    }
                    return;
                }

                string latDir = LatDirPicker.SelectedItem?.ToString() ?? "N";
                string longDir = LongDirPicker.SelectedItem?.ToString() ?? "E";

                // Windows GetLocation: LG = Degrees(txtLongDeg, txtLongMin), LT = Degrees(txtLatDeg, txtLatMin)
                // LG = Longitude (from txtLongDeg field), LT = Latitude (from txtLatDeg field)
                // Convert to decimal (Degrees function, always positive)
                double lg = ConvertFromDegreesMinutes(longDeg, longMin, longDir); // LG from txtLongDeg (Longitude)
                double lt = ConvertFromDegreesMinutes(latDeg, latMin, latDir);   // LT from txtLatDeg (Latitude)

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

                // Windows GetLocation produces: LG = longitude value, LT = latitude value
                double longitude = lg;  // LG is longitude value
                double latitude = lt;   // LT is latitude value

                // Get GMT offset
                if (!double.TryParse(GMTOffsetEntry.Text, out double gmtOffset))
                {
                    gmtOffset = 0;
                }

                // Calculate hour location (HR) - matching Windows GetLocation exactly
                // Windows GetLocation logic:
                //   HR = 12 + GMT (where GMT is always treated as positive value)
                //   If txtLonDir == "W" (West): HR = 12 - GMT
                //   If txtLonDir == "E" (East): HR = 12 + GMT (stays the same)
                // The Windows app stores GMT as positive, direction determines if we add or subtract
                // For negative GMT offsets (like -5), we need to use absolute value
                double gmtValue = Math.Abs(gmtOffset);
                double hr = 12 + gmtValue; // First step: add GMT
                if (longDir == "W") // txtLonDir == "W" (West)
                {
                    hr = 12 - gmtValue; // For West: subtract GMT from 12
                }
                // For East: hr stays as 12 + gmtValue

                // Calculate local moons using the calculation class (run on background thread)
                string locationName = LocationEntry.Text?.Trim() ?? "Custom Location";

                // Run calculation on background thread to prevent UI blocking
                await Task.Run(() =>
                {
                    try
                    {
                        // Windows GetLocation produces: LG = longitude value, LT = latitude value
                        // Function signature: CalculateLocalMoons(year, longitude, latitude, hr, locationName, originalGmtOffset)
                        // Function sets: LG = longitude parameter, LT = latitude parameter
                        // Pass coordinates in correct order: longitude, latitude
                        // Pass original GMT offset for correct display in header
                        _localMoonCalc.CalculateLocalMoons(year, longitude, latitude, hr, locationName, gmtOffset);
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
                if (calculateButton != null)
                {
                    calculateButton.IsEnabled = true;
                }
            }
        }

    }
}

