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
    public class IslandSouthInjections
    {
        private const string AP_WESTERN_TURTLE = "Island West Turtle";
        private const string AP_RESORT = "Island Resort";

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

        // public IslandSouth(string map, string name)
        public static void Constructor_ReplaceParrots_Postfix(IslandSouth __instance, string map, string name)
        {
            try
            {
                __instance.parrotUpgradePerches.Clear();
                AddResortParrot(__instance);
                AddWesternTurtleParrot(__instance);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Constructor_ReplaceParrots_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static void AddResortParrot(IslandLocation __instance)
        {
            __instance.parrotUpgradePerches.Add(new ParrotUpgradePerch(__instance, new Point(17, 22),
                new Microsoft.Xna.Framework.Rectangle(12, 18, 14, 7), 20, PurchaseResortParrot, IsResortParrotPurchased, "Resort", "Island_UpgradeHouse"));
        }

        private static void PurchaseResortParrot()
        {
            _locationChecker.AddCheckedLocation(AP_RESORT);
        }

        private static bool IsResortParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_RESORT);
        }

        private static void AddWesternTurtleParrot(IslandSouth __instance)
        {
            __instance.parrotUpgradePerches.Add(new ParrotUpgradePerch(__instance, new Point(5, 9),
                new Microsoft.Xna.Framework.Rectangle(1, 10, 3, 4), 10, PurchaseWesternTurtleParrot, IsWesternTurtleParrotPurchased, "Turtle", "Island_FirstParrot"));
        }

        private static void PurchaseWesternTurtleParrot()
        {
            _locationChecker.AddCheckedLocation(AP_WESTERN_TURTLE);
        }

        private static bool IsWesternTurtleParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_WESTERN_TURTLE);
        }
    }
}
