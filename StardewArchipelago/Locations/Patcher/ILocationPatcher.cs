namespace StardewArchipelago.Locations.Patcher
{
    public interface ILocationPatcher
    {
        void ReplaceAllLocationsRewardsWithChecks();
        void CleanEvents();
    }
}
