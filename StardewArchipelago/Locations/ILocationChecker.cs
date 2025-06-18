namespace StardewArchipelago.Locations
{
    public interface ILocationChecker
    {
        void AddCheckedLocations(string[] locationNames);
        void AddCheckedLocation(string locationName);
    }
}
