using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley.TerrainFeatures;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutBushInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public string GetShakeOffItem()
        public static bool GetShakeOffItem_ReplaceWalnutWithCheck_Prefix(Bush __instance, ref string __result)
        {
            try
            {
                if (__instance.size.Value != 4)
                {
                    return true; // run original logic
                }

                var bushId = $"Bush_{__instance.Location.Name}_{__instance.Tile.X}_{__instance.Tile.Y}";

                if (!_bushNameMap.ContainsKey(bushId))
                {
                    throw new Exception($"Bush '{bushId}' Could not be mapped to an Archipelago location!");
                }

                __result = $"(AP){IDProvider.AP_LOCATION} {_bushNameMap[bushId]}";
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetShakeOffItem_ReplaceWalnutWithCheck_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static readonly Dictionary<string, string> _bushNameMap = new Dictionary<string, string>()
        {
            {"Bush_IslandEast_17_37", "Jungle Bush"},
            {"Bush_IslandShrine_23_34", "Gem Birds Bush"},
            {"Bush_CaptainRoom_2_4", "Shipwreck Bush"},
            {"Bush_IslandWest_38_56", "Bush Behind Coconut Tree"},
            {"Bush_IslandWest_25_30", "Walnut Room Bush"},
            {"Bush_IslandWest_15_3", "Coast Bush"},
            {"Bush_IslandWest_31_24", "Bush Behind Mahogany Tree"},
            {"Bush_IslandWest_54_18", "Below Colored Crystals Cave Bush"},
            {"Bush_IslandWest_64_30", "Cliff Edge Bush"},
            {"Bush_IslandWest_104_3", "Farm Parrot Express Bush"},
            {"Bush_IslandWest_75_29", "Farmhouse Cliff Bush"},
            {"Bush_IslandNorth_9_84", "Grove Bush"},
            {"Bush_IslandNorth_4_42", "Above Dig Site Bush"},
            {"Bush_IslandNorth_45_38", "Above Field Office Bush 1"},
            {"Bush_IslandNorth_47_40", "Above Field Office Bush 2"},
            {"Bush_IslandNorth_56_27", "Bush Behind Volcano Tree"},
            {"Bush_IslandNorth_20_26", "Hidden Passage Bush"},
            {"Bush_IslandNorth_13_33", "Secret Beach Bush 1"},
            {"Bush_IslandNorth_5_30", "Secret Beach Bush 2"},
            {"20", "Forge Entrance Bush"},
            {"21", "Forge Exit Bush"},
            {"Bush_IslandSouth_31_5", "Cliff Over Island South Bush"},
        };
    }
}
