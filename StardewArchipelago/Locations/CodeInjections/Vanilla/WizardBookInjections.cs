﻿using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class WizardBookInjections
    {
        public const string EARTH_OBELISK = "Earth Obelisk";
        public const string WATER_OBELISK = "Water Obelisk";
        public const string DESERT_OBELISK = "Desert Obelisk";
        public const string ISLAND_OBELISK = "Island Obelisk";
        public const string JUNIMO_HUT = "Junimo Hut";
        public const string GOLD_CLOCK = "Gold Clock";
        public const string DEEP_WOODS_OBELISK = "Deep Woods Obelisk";

        private static readonly string[] _wizardBuildings = { EARTH_OBELISK, WATER_OBELISK, DESERT_OBELISK, ISLAND_OBELISK, JUNIMO_HUT, GOLD_CLOCK, DEEP_WOODS_OBELISK };

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_WizardBook_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return true; // run original logic
                }
                var firstWord = action[0];

                if (firstWord != "WizardBook")
                {
                    return true; // run original logic
                }

                if (_wizardBuildings.Any(x => _archipelago.HasReceivedItem(x)))
                {
                    __instance.ShowConstructOptions("Wizard");
                }

                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_WizardBook_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
