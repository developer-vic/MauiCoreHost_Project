using BiblCalCore;

namespace BiblCalMaui.Services
{
    /// <summary>
    /// MAUI implementation of IUserDataProvider with default location data
    /// </summary>
    public class MauiUserDataProvider : IUserDataProvider
    {
        private string _currentLocation = "Jerusalem, Israel";
        // CRITICAL: Windows XML attributes are swapped!
        // XML: long="31.78" (this is latitude), lat="-35.21" (this is longitude)
        // Windows ReadUserDataXML stores: DegLat = lat attribute value (longitude), DegLon = long attribute value (latitude)
        // So to match Windows internal storage exactly:
        // Latitude field should store what Windows calls DegLat (from XML "lat" attribute = longitude value)
        // Longitude field should store what Windows calls DegLon (from XML "long" attribute = latitude value)
        // For Jerusalem: XML long="31.78" lat="-35.21"
        // Windows stores: DegLat = -35.21 (longitude), DegLon = 31.78 (latitude)
        // We store to match: Latitude = -35.21 (DegLat), Longitude = 31.78 (DegLon)
        private readonly LocationData[] _locations = new LocationData[]
        {
            // All coordinates stored to match Windows: Latitude = XML "lat" attribute, Longitude = XML "long" attribute
            new LocationData { Name = "Jerusalem, Israel", Latitude = -35.2166666666667, Longitude = 31.7833333333333, GMTOffset = "2" },
            new LocationData { Name = "Lennon, Michigan, USA", Latitude = 42.95, Longitude = 83.95, GMTOffset = "4" },
            new LocationData { Name = "New York, New York, USA", Latitude = 40.7333333333333, Longitude = 73.9166666666667, GMTOffset = "5" },
            new LocationData { Name = "Chicago, Illinois, USA", Latitude = 41.85, Longitude = 87.65, GMTOffset = "6" },
            new LocationData { Name = "Houston, Texas, USA", Latitude = 29.75, Longitude = 95.3833333333333, GMTOffset = "6" },
            new LocationData { Name = "Los Angeles, California, USA", Latitude = 34.0833333333333, Longitude = 118.366666666667, GMTOffset = "8" },
            new LocationData { Name = "Honolulu, Hawaii, USA", Latitude = 21.3166666666667, Longitude = 157.833333333333, GMTOffset = "10" },
            new LocationData { Name = "Perth, Australia", Latitude = -31.9666666666667, Longitude = -115.816666666667, GMTOffset = "8" },
            new LocationData { Name = "Melbourne, Australia", Latitude = -37.82, Longitude = -144.97, GMTOffset = "10" },
            new LocationData { Name = "Brisbane, Australia", Latitude = 27.5, Longitude = -153, GMTOffset = "10" },
            new LocationData { Name = "Sydney, Australia", Latitude = -33.9166666666667, Longitude = -151.283333333333, GMTOffset = "10" },
            new LocationData { Name = "Ottawa, Ontario, Canada", Latitude = 45.4166666666667, Longitude = 75.7166666666667, GMTOffset = "5" },
            new LocationData { Name = "Vancouver, Canada", Latitude = 49.2166666666667, Longitude = 123.1, GMTOffset = "8" },
            new LocationData { Name = "Greenwich Observatory, England", Latitude = 51.4666666666667, Longitude = 0, GMTOffset = "0" },
            new LocationData { Name = "Berlin, Germany", Latitude = 52.5, Longitude = -13.0166666666667, GMTOffset = "1" },
            new LocationData { Name = "Kinshasa, Congo Dem.Rep.", Latitude = -4.3, Longitude = -15.3, GMTOffset = "1" },
            new LocationData { Name = "Paris, France", Latitude = 48.85, Longitude = -2.33333333333333, GMTOffset = "1" },
            new LocationData { Name = "Rome, Italy", Latitude = 41.8666666666667, Longitude = -12.6166666666667, GMTOffset = "1" },
            new LocationData { Name = "StockHolm, Sweden", Latitude = 59.03, Longitude = -18.05, GMTOffset = "1" },
            new LocationData { Name = "Cairo, Egypt", Latitude = 30, Longitude = -31.2833333333333, GMTOffset = "2" },
            new LocationData { Name = "Johannesburg, South Africa", Latitude = -26.1999972222222, Longitude = -28.0799972222222, GMTOffset = "2" },
            new LocationData { Name = "Moscow, Russia", Latitude = 55.75, Longitude = -37.6166666666667, GMTOffset = "3" },
            new LocationData { Name = "Rio de Janeiro, Brazil", Latitude = -22.45, Longitude = 42.7166666666667, GMTOffset = "3" },
            new LocationData { Name = "Lima, Lima, Peru", Latitude = -12.1, Longitude = 76.9166666666667, GMTOffset = "5" },
            new LocationData { Name = "Bombay, India", Latitude = 18.9333333333333, Longitude = -72.85, GMTOffset = "5.5" },
            new LocationData { Name = "Calcutta, India", Latitude = 22.5166666666667, Longitude = -88.3666666666667, GMTOffset = "5.5" },
            new LocationData { Name = "Mexico City, Mexico", Latitude = 19.4666666666667, Longitude = 99.15, GMTOffset = "6" },
            new LocationData { Name = "Jakarta, Java, Indonesia", Latitude = -6.13333333333333, Longitude = -106.75, GMTOffset = "7" },
            new LocationData { Name = "Beijing, China", Latitude = 39.9166666666667, Longitude = -116.383333333333, GMTOffset = "8" },
            new LocationData { Name = "Manila, Philippines", Latitude = 14.6166666666667, Longitude = -121, GMTOffset = "8" },
            new LocationData { Name = "Seoul, South Korea", Latitude = 37.5833333333333, Longitude = -127.05, GMTOffset = "9" },
            new LocationData { Name = "Tokyo, Japan", Latitude = 35.6833333333333, Longitude = -139.733333333333, GMTOffset = "9" }
        };

        public string GetCurrentLocation()
        {
            return _currentLocation;
        }

        public void SetCurrentLocation(string location)
        {
            _currentLocation = location;
        }

        public int GetNumberOfLocations()
        {
            return _locations.Length; // Return total count
        }

        public string GetLocationName(int index)
        {
            if (index >= 0 && index < _locations.Length)
            {
                return _locations[index].Name;
            }
            return string.Empty;
        }

        public double GetLocationLatitude(int index)
        {
            if (index >= 0 && index < _locations.Length)
            {
                return _locations[index].Latitude;
            }
            return 0;
        }

        public double GetLocationLongitude(int index)
        {
            if (index >= 0 && index < _locations.Length)
            {
                return _locations[index].Longitude;
            }
            return 0;
        }

        public string GetLocationGMTOffset(int index)
        {
            if (index >= 0 && index < _locations.Length)
            {
                return _locations[index].GMTOffset;
            }
            return "0";
        }

        public void SaveUserData()
        {
            // In a full implementation, this would save to platform-specific storage
            // For now, it's a no-op
        }

        private class LocationData
        {
            public string Name { get; set; } = string.Empty;
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public string GMTOffset { get; set; } = "0";
        }
    }
}

