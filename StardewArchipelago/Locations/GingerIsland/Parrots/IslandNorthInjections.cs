﻿using System;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Locations.GingerIsland.Parrots
{
    public class IslandNorthInjections : IParrotReplacer
    {
        private const string AP_BRIDGE_PARROT = "Dig Site Bridge";
        private const string AP_TRADER_PARROT = "Island Trader";
        public const string AP_PROF_SNAIL_CAVE = "Open Professor Snail Cave";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        private IslandLocation _islandLocation;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        public IslandNorthInjections()
        {
            _islandLocation = (IslandNorth)Game1.getLocationFromName("IslandNorth");
        }

        public void ReplaceParrots()
        {
            _islandLocation.parrotUpgradePerches.Clear();
            AddDigSiteBridgeParrot(_islandLocation);
            AddIslandTraderParrot(_islandLocation);
        }

        private static void AddDigSiteBridgeParrot(IslandLocation islandNorth)
        {
            var digSiteBridgeParrot = new ParrotUpgradePerchArchipelago(AP_BRIDGE_PARROT, _archipelago, islandNorth, new Point(35, 52),
                new Rectangle(31, 52, 4, 4), 10, PurchaseBridgeParrot, IsBridgeParrotPurchased,
                "Bridge", "Island_Turtle");
            islandNorth.parrotUpgradePerches.Add(digSiteBridgeParrot);
        }

        private static void PurchaseBridgeParrot()
        {
            _locationChecker.AddCheckedLocation(AP_BRIDGE_PARROT);
        }

        private static bool IsBridgeParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_BRIDGE_PARROT);
        }

        private static void AddIslandTraderParrot(IslandLocation islandNorth)
        {
            var islandTraderParrot = new ParrotUpgradePerchArchipelago(AP_TRADER_PARROT, _archipelago, islandNorth, new Point(32, 72),
                new Rectangle(33, 68, 5, 5), 10, PurchaseTraderParrot, IsTraderParrotPurchased,
                "Trader", "Island_UpgradeHouse");
            islandNorth.parrotUpgradePerches.Add(islandTraderParrot);
        }

        private static void PurchaseTraderParrot()
        {
            _locationChecker.AddCheckedLocation(AP_TRADER_PARROT);
        }

        private static bool IsTraderParrotPurchased()
        {
            return _locationChecker.IsLocationChecked(AP_TRADER_PARROT);
        }

        // public override void explosionAt(float x, float y)
        public static bool ExplosionAt_CheckProfessorSnailLocation_Prefix(IslandNorth __instance, float x, float y)
        {
            try
            {
                var totalDistance = Math.Abs(y - 47) + Math.Abs(x - 21.5);
                if (totalDistance > 2)
                {
                    return false; // don't run original logic
                }

                _locationChecker.AddCheckedLocation(AP_PROF_SNAIL_CAVE);
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(ExplosionAt_CheckProfessorSnailLocation_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
