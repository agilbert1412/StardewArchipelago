namespace StardewArchipelago.Locations
{
    public interface ILocationChecker
    {
        void AddWalnutCheckedLocation(string locationName);
        void AddCheckedLocations(string[] locationNames);
        void AddCheckedLocation(string locationName);
    }
}
