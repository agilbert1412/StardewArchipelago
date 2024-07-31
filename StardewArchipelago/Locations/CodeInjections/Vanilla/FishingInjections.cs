using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Goals;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FishingInjections
    {
        private static readonly string[] _fishedTrash = {
            QualifiedItemIds.JOJA_COLA, QualifiedItemIds.TRASH, QualifiedItemIds.DRIFTWOOD,
            QualifiedItemIds.BROKEN_GLASSES, QualifiedItemIds.BROKEN_CD, QualifiedItemIds.SOGGY_NEWSPAPER,
        };

        private static readonly string[] _fishsanityExceptions = {
            QualifiedItemIds.GREEN_ALGAE, QualifiedItemIds.WHITE_ALGAE, QualifiedItemIds.SEAWEED, QualifiedItemIds.ORNATE_NECKLACE,
            QualifiedItemIds.GOLDEN_WALNUT, QualifiedItemIds.SECRET_NOTE, QualifiedItemIds.FOSSILIZED_SPINE, QualifiedItemIds.PEARL,
            QualifiedItemIds.SNAKE_SKULL, QualifiedItemIds.JOURNAL_SCRAP, QualifiedItemIds.QI_BEAN,
            QualifiedItemIds.SEA_JELLY, QualifiedItemIds.CAVE_JELLY, QualifiedItemIds.RIVER_JELLY, QualifiedItemIds.WALL_BASKET,
        };
        public const string FISHSANITY_PREFIX = "Fishsanity: ";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _itemManager = itemManager;
        }

        // public bool caughtFish(string itemId, int size, bool from_fish_pond = false, int numberCaught = 1)
        public static void CaughtFish_Fishsanity_Postfix(Farmer __instance, string itemId, int size, bool from_fish_pond, int numberCaught, ref bool __result)
        {
            try
            {
                // itemId is qualified
                if (from_fish_pond || (IsFishedTrash(itemId)) || !_itemManager.ItemExistsByQualifiedId(itemId))
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
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CaughtFish_Fishsanity_Postfix)}:\n{ex}");
                return;
            }
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
    }
}
