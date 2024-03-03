using System;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Goals;
using StardewArchipelago.Stardew;
using StardewArchipelago.Stardew.Ids.Vanilla;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class FishingInjections
    {
        private static readonly string[] _fishedTrash = new[]
        {
            ItemIds.JOJA_COLA, ItemIds.TRASH, ItemIds.DRIFTWOOD,
            ItemIds.BROKEN_GLASSES, ItemIds.BROKEN_CD, ItemIds.SOGGY_NEWSPAPER,
        };

        private static readonly string[] _fishsanityExceptions = new[]
        {
            ItemIds.GREEN_ALGAE, ItemIds.WHITE_ALGAE, ItemIds.SEAWEED, ItemIds.ORNATE_NECKLACE, ItemIds.GOLDEN_WALNUT, 
            ItemIds.SECRET_NOTE, ItemIds.FOSSILIZED_SPINE, ItemIds.PEARL, ItemIds.SNAKE_SKULL, ItemIds.JOURNAL_SCRAP,
            ItemIds.QI_BEAN,
        };
        public const string FISHSANITY_PREFIX = "Fishsanity: ";

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static StardewItemManager _itemManager;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker, StardewItemManager itemManager)
        {
            _monitor = monitor;
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
                if (from_fish_pond || (IsFishedTrash(itemId)) || !_itemManager.ObjectExists(itemId))
                {
                    return;
                }

                var fish = _itemManager.GetObjectById(itemId);
                var fishName = fish.Name;
                var apLocation = $"{FISHSANITY_PREFIX}{fishName}";
                if (_archipelago.GetLocationId(apLocation) > -1)
                {
                    _locationChecker.AddCheckedLocation(apLocation);
                }
                else if (!_fishsanityExceptions.Contains(itemId))
                {
                    _monitor.Log($"Unrecognized Fishsanity Location: {fishName} [{itemId}]", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CaughtFish_Fishsanity_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        public static void CaughtFish_CheckGoalCompletion_Postfix(Farmer __instance, int index, int size, bool from_fish_pond, int numberCaught, ref bool __result)
        {
            try
            {
                GoalCodeInjection.CheckMasterAnglerGoalCompletion();
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(CaughtFish_CheckGoalCompletion_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        private static bool IsFishedTrash(string itemId)
        {
            return _fishedTrash.Contains(itemId);
        }
    }
}
