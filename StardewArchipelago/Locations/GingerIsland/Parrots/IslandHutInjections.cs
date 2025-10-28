using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.GingerIsland.Parrots
{
    public class IslandHutInjections : IParrotReplacer
    {
        private const string AP_LEO_PARROT = "Leo's Parrot";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private readonly IslandLocation _islandLocation;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public IslandHutInjections()
        {
            _islandLocation = (IslandHut)Game1.getLocationFromName("IslandHut");
        }

        public void ReplaceParrots()
        {
            _islandLocation.parrotUpgradePerches.Clear();
            AddLeoParrot(_islandLocation);
        }

        private static void AddLeoParrot(IslandLocation islandLocation)
        {
            islandLocation.parrotUpgradePerches.Add(new ParrotUpgradePerchArchipelago(AP_LEO_PARROT, _archipelago, islandLocation, new Point(7, 6), new Rectangle(-1000, -1000, 1, 1), 1, BefriendLeoParrot, IsLeoParrotBefriended, "Hut"));
        }

        private static void BefriendLeoParrot()
        {
            _locationChecker.AddCheckedLocation(AP_LEO_PARROT);
        }

        private static bool IsLeoParrotBefriended()
        {
            return _locationChecker.IsLocationChecked(AP_LEO_PARROT);
        }
    }
}
