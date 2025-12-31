using BiblCalCore;

namespace BiblCalMaui.Services
{
    /// <summary>
    /// MAUI implementation of IUserDataProvider with default location data
    /// </summary>
    public class MauiUserDataProvider : IUserDataProvider
    {
        private string _currentLocation = "Jerusalem, Israel";
        private readonly LocationData[] _locations = new LocationData[]
        {
            new LocationData { Name = "Jerusalem, Israel", Latitude = 31.7833333333333, Longitude = -35.2166666666667, GMTOffset = "2" },
            new LocationData { Name = "Mount Ararat", Latitude = 39.69, Longitude = -44.32, GMTOffset = "3" }
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
            return _locations.Length - 1; // Zero-indexed
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

