using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;

namespace StardewArchipelago
{
    public class BugFixer
    {
        private readonly ILogger _logger;
        private readonly LocationChecker _locationChecker;

        public BugFixer(ILogger logger, LocationChecker locationChecker)
        {
            _logger = logger;
            _locationChecker = locationChecker;
        }


        public void FixKnownBugs()
        {
            FixFreeBuildingsWithFreeInTheId();
        }

        private static void FixFreeBuildingsWithFreeInTheId()
        {

            foreach (var gameLocation in Game1.locations)
            {
                foreach (var building in gameLocation.buildings)
                {
                    building.buildingType.Set(CarpenterInjections.RemoveFreePrefix(building.buildingType.Get()));
                }
            }
        }
    }
}
