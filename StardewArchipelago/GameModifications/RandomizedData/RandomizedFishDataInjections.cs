using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class RandomizedFishDataInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static StardewItemManager _itemManager;
        private static DataRandomization _dataRandomization;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        // public override Item getFish(float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName = null)
        public static bool GetFish_DeHardCodeMinesFish_Prefix(MineShaft __instance, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName, ref Item __result)
        {
            try
            {
                if (locationName != null && locationName != __instance.Name)
                {
                    var locationFromName = Game1.getLocationFromName(locationName);
                    if (locationFromName != null && locationFromName != __instance)
                    {
                        __result = locationFromName.getFish(millisecondsAfterNibble, bait, waterDepth, who, baitPotency, bobberTile);
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                }

                var isTutorialCatch = who.fishCaught.Length == 0;
                __result = GameLocation.GetFishFromLocationData(__instance.Name, bobberTile, waterDepth, who, isTutorialCatch, false, __instance) ?? ItemRegistry.Create("(O)168");
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetFish_DeHardCodeMinesFish_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static Item CreateRandomTrash()
        {
            return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
        }
    }
}
