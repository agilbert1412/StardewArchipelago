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
using StardewValley.Menus;
using StardewValley.SpecialOrders;
using StardewValley.Tools;
using System;
using System.Linq;

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
    }
}
