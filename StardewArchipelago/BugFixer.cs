using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewValley;

namespace StardewArchipelago
{
    public class BugFixer
    {
        public BugFixer() { }


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
