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
    public class IslandNorthInjections
    {
        private const string AP_BRIDGE_PARROT = "Dig Site Bridge";
        private const string AP_TRADER_PARROT = "Island Trader";

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

        // public IslandNorth(string map, string name)
        public static void Constructor_ReplaceParrots_Postfix(IslandNorth __instance, string map, string name)
        {
            try
            {
                __instance.parrotUpgradePerches.Clear();
                AddBridgeParrot(__instance);
                AddTraderParrot(__instance);
                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(Constructor_ReplaceParrots_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static void AddBridgeParrot(IslandLocation __instance)
        {
            __instance.parrotUpgradePerches.Add(new ParrotUpgradePerch(__instance, new Point(35, 52), new Microsoft.Xna.Framework.Rectangle(31, 52, 4, 4), 10, PurchaseBridgeParrot, IsBridgeParrotPurchased, "Bridge", "Island_Turtle"));
        }

        private static void PurchaseBridgeParrot()
        {
            _locationChecker.AddCheckedLocation(AP_BRIDGE_PARROT);
        }

        private static bool IsBridgeParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_BRIDGE_PARROT);
        }

        private static void AddTraderParrot(IslandLocation __instance)
        {
            __instance.parrotUpgradePerches.Add(new ParrotUpgradePerch(__instance, new Point(32, 72), new Microsoft.Xna.Framework.Rectangle(33, 68, 5, 5), 10, PurchaseTraderParrot, IsTraderParrotPurchased, "Trader", "Island_UpgradeHouse"));
        }

        private static void PurchaseTraderParrot()
        {
            _locationChecker.AddCheckedLocation(AP_TRADER_PARROT);
        }

        private static bool IsTraderParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_TRADER_PARROT);
        }
    }
}
