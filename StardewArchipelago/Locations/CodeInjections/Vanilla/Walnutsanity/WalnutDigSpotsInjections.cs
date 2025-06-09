using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutDigSpotsInjections
    {
        private static ILogger _logger;
        private static IModHelper _helper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _archipelago.ScoutStardewLocations(_digSpotNameMap.Values);
        }

        // public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        public static bool CheckForBuriedItem_ReplaceWalnutWithCheck_Prefix(IslandLocation __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who, ref string __result)
        {
            try
            {
                if (!__instance.IsBuriedNutLocation(new Point(xLocation, yLocation)))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var digSpotId = $"Buried_{__instance.Name}_{xLocation}_{yLocation}";
                if (!_digSpotNameMap.ContainsKey(digSpotId))
                {
                    throw new Exception($"Dig Spot '{digSpotId}' Could not be mapped to an Archipelago location!");
                }

                if (!Game1.netWorldState.Value.FoundBuriedNuts.Add(digSpotId) && !_locationChecker.IsLocationMissing(_digSpotNameMap[digSpotId]))
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                Game1.player.team.MarkCollectedNut(digSpotId);
                var itemId = IDProvider.CreateApLocationItemId(_digSpotNameMap[digSpotId]);
                var item = ItemRegistry.Create(itemId);
                Game1.createItemDebris(item, new Vector2(xLocation, yLocation) * 64f, -1, __instance);

                __result = "";
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForBuriedItem_ReplaceWalnutWithCheck_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static readonly Dictionary<string, string> _digSpotNameMap = new()
        {
            { "Buried_IslandWest_62_76", "Starfish Triangle" },
            { "Buried_IslandWest_43_74", "Starfish Diamond" },
            { "Buried_IslandWest_30_75", "X in the sand" },
            { "Buried_IslandWest_21_81", "Diamond Of Indents" },
            { "Buried_IslandWest_39_24", "Circle Of Grass" },
            { "Buried_IslandWest_88_14", "Diamond Of Pebbles" },
            { "Buried_IslandNorth_26_81", "Big Circle Of Stones" },
            { "Buried_IslandNorth_42_77", "Diamond Of Grass" },
            { "Buried_IslandNorth_57_79", "Small Circle Of Stones" },
            { "Buried_IslandNorth_62_54", "Patch Of Sand" },
            { "Buried_IslandNorth_19_39", "Crooked Circle Of Stones" },
            { "Buried_IslandNorth_54_21", "Arc Of Stones" },
            { "Buried_IslandNorth_19_13", "Northmost Point Circle Of Stones" },
            { "Buried_IslandSouthEast_25_17", "Diamond Of Yellow Starfish" },
            { "Buried_IslandSouthEastCave_36_26", "Pirate Cove Patch Of Sand" },
        };
    }
}
