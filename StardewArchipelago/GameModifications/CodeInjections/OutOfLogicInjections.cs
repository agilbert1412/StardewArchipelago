using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Stardew;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.Objects;
using Category = StardewArchipelago.Stardew.Category;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public class OutOfLogicInjections
    {
        private const int MAX_DEPTH = 10;

        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static StardewItemManager _stardewItemManager;
        private static int _depth;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager)
        {
            _logger = logger;
            _archipelago = archipelago;
            _stardewItemManager = stardewItemManager;
            _depth = 0;
        }

        // public static Item getPrizeItem(int prizeLevel)
        public static void GetPrizeItem_SkipOutOfLogicPrizeTickets_Postfix(int prizeLevel, ref Item __result)
        {
            try
            {
                if (__result.Category != Category.SEED || _archipelago.SlotData.Cropsanity == Cropsanity.Disabled)
                {
                    return;
                }

                var replacementsList = new Dictionary<string, int>
                {
                    { QualifiedItemIds.MIXED_SEEDS, 15 }, { QualifiedItemIds.MIXED_FLOWER_SEEDS, 15 }, { QualifiedItemIds.ARTIFACT_TROVE, 4 },
                    { QualifiedItemIds.MYSTERY_BOX, 5 }, { QualifiedItemIds.GOLDEN_MYSTERY_BOX, 2 }, { QualifiedItemIds.PRIZE_TICKET, 1 },
                    { QualifiedItemIds.STARDROP_TEA, 1 }, { QualifiedItemIds.IRIDIUM_SPRINKLER, 1 }, { QualifiedItemIds.OMNI_GEODE, 8 },
                    { QualifiedItemIds.MAGIC_ROCK_CANDY, 1 }, { QualifiedItemIds.DIAMOND, 5 }, { QualifiedItemIds.CHERRY_BOMB, 20 },
                    { QualifiedItemIds.BOMB, 12 }, { QualifiedItemIds.MEGA_BOMB, 6 },
                };

                var random = Utility.CreateRandom(Game1.uniqueIDForThisGame, prizeLevel);
                var keys = replacementsList.Keys.ToArray();
                var chosenIndex = random.Next(keys.Length);
                var chosenItem = keys[chosenIndex];
                var amount = replacementsList[chosenItem];
                __result = ItemRegistry.Create(chosenItem, amount);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetPrizeItem_SkipOutOfLogicPrizeTickets_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static Item getTreasureFromGeode(Item geode)
        public static void GetTreasureFromGeode_MysteryBoxesGiveReceivedItems_Postfix(Item geode, ref Item __result)
        {
            try
            {
                if (!Utility.IsGeode(geode) || !geode.QualifiedItemId.Contains("MysteryBox") || __result is null or Furniture or Clothing ||
                    __result.QualifiedItemId.Contains("MysteryBox") || __result.Category is Category.POWER_BOOK or Category.EXPERIENCE_BOOK)
                {
                    _depth = 0;
                    return;
                }

                var itemName = __result.Name;
                if (_archipelago.GetAllReceivedItems().Any(x => x.ItemName.Contains(itemName)))
                {
                    _depth = 0;
                    return;
                }

                if (__result.Category == Category.SEED && _archipelago.SlotData.Cropsanity == Cropsanity.Disabled)
                {
                    _depth = 0;
                    return;
                }

                if (__result.QualifiedItemId == QualifiedItemIds.MIXED_SEEDS || __result.QualifiedItemId == QualifiedItemIds.MIXED_FLOWER_SEEDS || __result.QualifiedItemId == QualifiedItemIds.HARDWOOD)
                {
                    _depth = 0;
                    return;
                }

                if (Game1.player.cookingRecipes.ContainsKey(itemName) && Game1.player.cookingRecipes[itemName] > 0)
                {
                    _depth = 0;
                    return;
                }

                if (_depth >= MAX_DEPTH)
                {
                    _depth = 0;
                    __result = ItemRegistry.Create(QualifiedItemIds.STONE);
                    return;
                }

                _logger.LogDebug($"Mystery Box tried to create [{__result.Name}] but the player is not allowed to have this yet. Trying again... (Depth: {_depth})");
                _depth++;
                Game1.stats.Increment("MysteryBoxesOpened");
                __result = Utility.getTreasureFromGeode(geode);
                return;
            }
            catch (Exception ex)
            {
                _depth = 0;
                _logger.LogError($"Failed in {nameof(GetTreasureFromGeode_MysteryBoxesGiveReceivedItems_Postfix)} (Depth: {_depth}):\n{ex}");
                return;
            }
        }

        // public static Item getRaccoonSeedForCurrentTimeOfYear(Farmer who, Random r, int stackOverride = -1)
        public static void GetRaccoonSeedForCurrentTimeOfYear_MysteryBoxesGiveReceivedItems_Postfix(Farmer who, Random r, int stackOverride, ref Item __result)
        {
            try
            {
                if (__result == null || _archipelago.SlotData.Cropsanity == Cropsanity.Disabled || _archipelago.HasReceivedItem(__result.Name))
                {
                    return;
                }

                __result = ItemRegistry.Create(r.NextBool() ? QualifiedItemIds.MIXED_SEEDS : QualifiedItemIds.MIXED_FLOWER_SEEDS, __result.Stack);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRaccoonSeedForCurrentTimeOfYear_MysteryBoxesGiveReceivedItems_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
