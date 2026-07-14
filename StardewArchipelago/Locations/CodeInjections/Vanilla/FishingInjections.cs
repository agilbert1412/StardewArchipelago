using System;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Goals;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Extensions;
using StardewValley.GameData.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.SpecialOrders;
using StardewValley.Tools;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Constants.Modded;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FishingInjections
    {
        private static readonly string[] _fishedTrash =
        {
            QualifiedItemIds.JOJA_COLA, QualifiedItemIds.TRASH, QualifiedItemIds.DRIFTWOOD,
            QualifiedItemIds.BROKEN_GLASSES, QualifiedItemIds.BROKEN_CD, QualifiedItemIds.SOGGY_NEWSPAPER,
        };

        private static readonly string[] _fishsanityExceptions =
        {
            QualifiedItemIds.GREEN_ALGAE, QualifiedItemIds.WHITE_ALGAE, QualifiedItemIds.SEAWEED, QualifiedItemIds.ORNATE_NECKLACE,
            QualifiedItemIds.GOLDEN_WALNUT, QualifiedItemIds.SECRET_NOTE, QualifiedItemIds.FOSSILIZED_SPINE, QualifiedItemIds.PEARL,
            QualifiedItemIds.SNAKE_SKULL, QualifiedItemIds.JOURNAL_SCRAP, QualifiedItemIds.QI_BEAN, QualifiedItemIds.VOID_MAYONNAISE,
            QualifiedItemIds.SEA_JELLY, QualifiedItemIds.CAVE_JELLY, QualifiedItemIds.RIVER_JELLY,
            QualifiedItemIds.WALL_BASKET, QualifiedItemIds.GOURMAND_STATUE, QualifiedItemIds.PHYSICS_101,
            QualifiedItemIds.VISTA, QualifiedItemIds.DECORATIVE_TRASH_CAN, QualifiedItemIds.LIFESAVER,
            QualifiedItemIds.WOOD, QualifiedItemIds.STONE, 
        };
        public const string FISHSANITY_PREFIX = "Fishsanity: ";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;
        private static ArchipelagoWalletDto _wallet;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager, ArchipelagoWalletDto wallet)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
            _wallet = wallet;
        }

        // public bool caughtFish(string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
        public static void CaughtFish_Fishsanity_Postfix(Farmer __instance, string itemId, int size, bool from_fish_pond, int numberCaught, ref bool __result)
        {
            try
            {
                CheckFishsanityLocation(itemId, from_fish_pond);
                DoExtraBehaviorsFromCrabPots(__instance, itemId, from_fish_pond, numberCaught);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CaughtFish_Fishsanity_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void CheckFishsanityLocation(string itemId, bool fromFishPond)
        {
            if (_archipelago.SlotData.Fishsanity == Fishsanity.None)
            {
                return;
            }

            // itemId is qualified
            if (fromFishPond || (IsFishedTrash(itemId)) || !_itemManager.ItemExistsByQualifiedId(itemId))
            {
                return;
            }

            var fish = _itemManager.GetItemByQualifiedId(itemId);
            var fishName = fish.Name;
            var apLocation = $"{FISHSANITY_PREFIX}{fishName}";
            if (_archipelago.GetLocationId(apLocation) > -1)
            {
                _locationChecker.AddCheckedLocation(apLocation);
            }
            else if (!_fishsanityExceptions.Contains(itemId))
            {
                _logger.LogError($"Unrecognized Fishsanity Location: {fishName} [{itemId}]");
            }
            return;
        }

        private static void DoExtraBehaviorsFromCrabPots(Farmer who, string fishId, bool fromFishPond, int numberCaught)
        {
            if (fromFishPond || who.team.specialOrders == null)
            {
                return;
            }

            if (!IsCrabPotFish(fishId))
            {
                return;
            }

            ProgressSpecialOrdersFromCrabPots(who, fishId, numberCaught);
            ProgressFestivalsFromCrabPots(who, fishId, numberCaught);
        }

        private static void ProgressSpecialOrdersFromCrabPots(Farmer who, string fishId, int numberCaught)
        {
            foreach (var specialOrder in who.team.specialOrders)
            {
                var onFishCaught = specialOrder.onFishCaught;
                onFishCaught?.Invoke(who, ItemRegistry.Create(fishId, numberCaught));
            }
        }

        private static void ProgressFestivalsFromCrabPots(Farmer who, string fishId, int numberCaught)
        {
            if (Game1.IsSummer && QualifiedItemIds.UnqualifyId(fishId) == ObjectIds.RAINBOW_TROUT && Game1.dayOfMonth >= 20 && Game1.dayOfMonth <= 21 && Game1.random.NextDouble() < 0.33 * numberCaught)
            {
                who.addItemByMenuIfNecessaryElseHoldUp(ItemRegistry.Create("(O)TroutDerbyTag"));
            }

            //if (Game1.IsWinter && QualifiedItemIds.UnqualifyId(fishId) == ObjectIds.SQUID && Game1.dayOfMonth >= 12 && Game1.dayOfMonth <= 13)
            //{
            //    Game1.player.
            //}
        }

        // public bool caughtFish(string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
        public static void CaughtFish_CheckGoalCompletion_Postfix(Farmer __instance, string itemId, int size, bool from_fish_pond, int numberCaught, ref bool __result)
        {
            try
            {
                GoalCodeInjection.CheckMasterAnglerGoalCompletion();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CaughtFish_CheckGoalCompletion_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool IsFishedTrash(string itemId)
        {
            return _fishedTrash.Contains(itemId);
        }

        // public override void update(GameTime time)
        public static bool Update_CountMissedFish_Prefix(BobberBar __instance, GameTime time)
        {
            try
            {
                if (!__instance.fadeOut || __instance.scale > 0.05 || __instance.distanceFromCatching > 0.05)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                _wallet.MissedFishById.TryAdd(__instance.whichFish, 0);
                _wallet.MissedFishById[__instance.whichFish]++;

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Update_CountMissedFish_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        // public void pullFishFromWater(string fishId, int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, string setFlagOnCatch, bool isBossFish, int numCaught)
        public static bool pullFishFromWater_CrabPotFishDeserveASize_Prefix(FishingRod __instance, string fishId, ref int fishSize, int fishQuality, int fishDifficulty, bool treasureCaught, bool wasPerfect, bool fromFishPond, string setFlagOnCatch, bool isBossFish, int numCaught)
        {
            try
            {
                if (fishSize > 0 || string.IsNullOrWhiteSpace(fishId))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (IsCrabPotFish(fishId))
                {
                    fishSize = 1;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(pullFishFromWater_CrabPotFishDeserveASize_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool IsCrabPotFish(string fishId)
        {
            var fishDatas = DataLoader.Fish(Game1.content);
            fishId = QualifiedItemIds.UnqualifyId(fishId);
            if (!fishDatas.ContainsKey(fishId))
            {
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }

            var fishData = fishDatas[fishId];
            return fishData.Split("/")[1].Equals("trap", StringComparison.InvariantCultureIgnoreCase);
        }

        // public override void DayUpdate()
        public static bool DayUpdate_FairCrabPotOdds_Prefix(CrabPot __instance)
        {
            try
            {
                var location = __instance.Location;
                var player = Game1.GetPlayer(__instance.owner.Value) ?? Game1.MasterPlayer;
                var hasMariner = player.professions.Contains(10);
                if (__instance.NeedsBait(player) || __instance.heldObject.Value != null)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                __instance.tileIndexToShow = 714;
                __instance.readyForHarvest.Value = true;
                var daySaveRandom = Utility.CreateDaySaveRandom(__instance.tileLocation.X * 1000.0, __instance.tileLocation.Y * (double)byte.MaxValue, __instance.directionOffset.X * 1000.0 + __instance.directionOffset.Y);
                var possibleFish = new List<string>();
                if (!location.TryGetFishAreaForTile(__instance.tileLocation.Value, out var id, out var data))
                {
                    data = null;
                }

                var chanceOfJunk = GetBaitModifiers(__instance, hasMariner, data, daySaveRandom, out var amount, out var quality, out var specificFishId);
                var rolledFishes = new List<Object>();
                if (!daySaveRandom.NextBool(chanceOfJunk))
                {
                    var crabPotFishForTile = location.GetCrabPotFishForTile(__instance.tileLocation.Value);
                    var allFishData = DataLoader.Fish(Game1.content).OrderBy(x => daySaveRandom.NextDouble()).ToArray();
                    foreach (var (fishId, fishData) in allFishData)
                    {
                        if (!fishData.Contains("trap"))
                        {
                            continue;
                        }

                        var fishDataFields = fishData.Split('/');
                        var areas = ArgUtility.SplitBySpace(fishDataFields[4]);
                        var fishCanBeCaughtHere = areas.Any(x => crabPotFishForTile.Any(y => x == y));
                        if (!fishCanBeCaughtHere)
                        {
                            continue;
                        }

                        possibleFish.Add(fishId);
                        var fishChance = Convert.ToDouble(fishDataFields[2]);
                        if (specificFishId != null && specificFishId == fishId)
                        {
                            fishChance *= fishChance < 0.1 ? 4.0 : (fishChance < 0.2 ? 3.0 : 2.0);
                        }
                        if (daySaveRandom.NextDouble() < fishChance)
                        {
                            rolledFishes.Add(ItemRegistry.Create<Object>("(O)" + fishId, amount, quality));
                            break;
                        }
                    }
                }

                Object itemCaught = null;
                if (rolledFishes.Any())
                {
                    if (specificFishId != null)
                    {
                        itemCaught = rolledFishes.FirstOrDefault(x => x.ItemId == specificFishId);
                    }
                    itemCaught ??= rolledFishes[daySaveRandom.Next(rolledFishes.Count)];
                }
                else
                {
                    itemCaught = hasMariner ?
                        ItemRegistry.Create<Object>("(O)" + daySaveRandom.ChooseFrom(possibleFish), amount, quality) :
                        ItemRegistry.Create<Object>("(O)" + daySaveRandom.Next(168, 173));
                }

                __instance.heldObject.Value = itemCaught;
                if (itemCaught != null && _modHelper.ModRegistry.IsLoaded(ModUniqueIds.UniqueIds[ModNames.AUTOMATE]))
                {
                    var apLocation = $"{FISHSANITY_PREFIX}{itemCaught.Name}";
                    if (_archipelago.GetLocationId(apLocation) > -1)
                    {
                        _locationChecker.AddCheckedLocation(apLocation);
                    }
                }
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DayUpdate_FairCrabPotOdds_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static double GetBaitModifiers(CrabPot __instance, bool hasMariner, FishAreaData data, Random daySaveRandom, out int amount, out int quality, out string specificFishId)
        {
            var chanceOfJunk = hasMariner ? 0.0 : data?.CrabPotJunkChance ?? 0.2;
            amount = 1;
            quality = 0;
            specificFishId = null;
            var id = __instance.bait.Value?.QualifiedItemId;
            switch (id)
            {
                case "(O)DeluxeBait":
                    quality = 1;
                    chanceOfJunk /= 2.0;
                    break;
                case "(O)774":
                    chanceOfJunk /= 2.0;
                    if (daySaveRandom.NextBool(0.25))
                    {
                        amount = 2;
                        break;
                    }
                    break;
                case "(O)SpecificBait":
                    if (__instance.bait.Value.preservedParentSheetIndex.Value != null && __instance.bait.Value.preserve.Value.HasValue)
                    {
                        specificFishId = __instance.bait.Value.preservedParentSheetIndex.Value;
                        chanceOfJunk /= 2.0;
                        break;
                    }
                    break;
            }
            return chanceOfJunk;
        }
    }
}
