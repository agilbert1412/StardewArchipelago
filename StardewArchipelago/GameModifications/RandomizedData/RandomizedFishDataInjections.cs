using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.EntranceRandomizer;
using StardewArchipelago.Locations.Secrets;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Locations;
using StardewValley.Locations;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;

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
        public static void GetFish_CorrectMinesFish_Postfix(MineShaft __instance, float millisecondsAfterNibble, string bait, int waterDepth, Farmer who, double baitPotency, Vector2 bobberTile, string locationName, ref Item __result)
        {
            try
            {
                var fishName = __result.Name;

                if (!_dataRandomization.FishData.ContainsKey(fishName) || _dataRandomization.FishData[fishName].Location == null)
                {
                    return;
                }

                var currentRegion = "";

                switch (__instance.getMineArea())
                {
                    case 0:
                    case 10:
                        currentRegion = "The Mines - Floor 20";
                        break;
                    case 40:
                        currentRegion = "The Mines - Floor 60";
                        break;
                    case 80:
                        currentRegion = "The Mines - Floor 100";
                        break;
                    default:
                        return;
                }

                var validFish = _dataRandomization.FishData.Values.Where(x => x.Location.Contains(currentRegion)).ToArray();

                if (!validFish.Any())
                {
                    __result = Game1.random.NextDouble() < 0.05 + who.LuckLevel * 0.05 ? ItemRegistry.Create("(O)CaveJelly") : CreateRandomTrash();
                    return;
                }

                var chosenFish = validFish[Game1.random.Next(0, validFish.Length)];
                var fishObject = _itemManager.GetObjectByName(chosenFish.Name);
                var fishId = fishObject.GetQualifiedId();
                var quality = 0;
                if (Game1.random.NextDouble() < who.FishingLevel / 10.0)
                {
                    quality = 1;
                }
                if (Game1.random.NextDouble() < who.FishingLevel / 50.0 + who.LuckLevel / 100.0)
                {
                    quality = 2;
                }

                __result = ItemRegistry.Create(fishId, quality: quality);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetFish_CorrectMinesFish_Postfix)}:\n{ex}");
                return;
            }
        }

        private static Item CreateRandomTrash()
        {
            return ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
        }
    }
}
