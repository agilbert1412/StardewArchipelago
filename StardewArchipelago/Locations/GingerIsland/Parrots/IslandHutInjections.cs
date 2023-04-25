using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.GingerIsland.Parrots
{
    public class IslandHutInjections
    {
        private const string AP_LEO_PARROT = "Leo's Parrot";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public IslandHut(string map, string name)
        public static void Constructor_ReplaceParrots_Postfix(IslandHut __instance, string map, string name)
        {
            try
            {
                __instance.parrotUpgradePerches.Clear();
                AddLeoParrot(__instance);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Constructor_ReplaceParrots_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static void AddLeoParrot(IslandLocation __instance)
        {
            __instance.parrotUpgradePerches.Add(new ParrotUpgradePerch(__instance, new Point(7, 6), new Microsoft.Xna.Framework.Rectangle(-1000, -1000, 1, 1), 1, BefriendLeoParrot, IsLeoParrotBefriended, "Hut"));
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
