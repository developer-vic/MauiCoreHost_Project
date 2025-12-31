namespace BiblCalCore
{
    /// <summary>
    /// Interface for user data operations (replaces file I/O operations)
    /// </summary>
    public interface IUserDataProvider
    {
        string GetCurrentLocation();
        void SetCurrentLocation(string location);
        int GetNumberOfLocations();
        string GetLocationName(int index);
        double GetLocationLatitude(int index);
        double GetLocationLongitude(int index);
        string GetLocationGMTOffset(int index);
        void SaveUserData();
    }
}

