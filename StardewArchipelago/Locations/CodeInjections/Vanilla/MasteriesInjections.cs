using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class MasteriesInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Dictionary<Skill, double> _archipelagoExperience = new();

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago,
            LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_MasteryCaveInteractions_Prefix(Farmer farmer, string[] action, Farmer who, Location tileLocation)
        {
            try
            {
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(PerformAction_MasteryCaveInteractions_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        // MasteryTrackerMenu
        // public static int getCurrentMasteryLevel()
    }
}
