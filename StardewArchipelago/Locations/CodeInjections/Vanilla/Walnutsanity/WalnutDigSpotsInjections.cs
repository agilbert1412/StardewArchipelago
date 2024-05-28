using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Walnutsanity
{
    public static class WalnutDigSpotsInjections
    {
        private static IMonitor _monitor;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static Texture2D _bushtexture;

        public static void Initialize(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _helper = helper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _bushtexture = ArchipelagoTextures.GetArchipelagoBush(monitor, helper);
        }

        // public override string checkForBuriedItem(int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who)
        public static bool CheckForBuriedItem_ReplaceWalnutWithCheck_Prefix(IslandLocation __instance, int xLocation, int yLocation, bool explosion, bool detectOnly, Farmer who, ref string __result)
        {
            try
            {
                if (!__instance.IsBuriedNutLocation(new Point(xLocation, yLocation)))
                {
                    return true; // run original logic
                }

                var digSpotId = $"Buried_{__instance.Name}_{xLocation}_{yLocation}";
                if (!_digSpotNameMap.ContainsKey(digSpotId))
                {
                    throw new Exception($"Dig Spot '{digSpotId}' Could not be mapped to an Archipelago location!");
                }

                Game1.player.team.MarkCollectedNut(digSpotId);
                var itemId = IDProvider.CreateApLocationItemId(_digSpotNameMap[digSpotId]);
                var item = ItemRegistry.Create(itemId);
                Game1.createItemDebris(item, new Vector2(xLocation, yLocation) * 64f, -1, __instance);

                __result = "";
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CheckForBuriedItem_ReplaceWalnutWithCheck_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }

        private static readonly Dictionary<string, string> _digSpotNameMap = new()
        {
            { "01", "Journal Scrap #6" },
            { "02", "Starfish Triangle" },
            { "03", "Starfish Diamond" },
            { "04", "X in the sand" },
            { "05", "Diamond Of Indents" },
            { "06", "Journal Scrap #4" },
            { "07", "Diamond Of Pebbles" },
            { "08", "Big Circle Of Stones" },
            { "09", "Circle Of Flowers" },
            { "10", "Small Circle Of Stones" },
            { "11", "Patch Of Sand" },
            { "12", "Crooked Circle Of Stones" },
            { "13", "Arc Of Stones" },
            { "14", "Journal Scrap #10" },
            { "15", "Northmost Point Circle Of Stones" },
            { "16", "Diamond Of Yellow Starfish" },
            { "17", "Pirate Cove Patch Of Sand" },
        };
    }
}
