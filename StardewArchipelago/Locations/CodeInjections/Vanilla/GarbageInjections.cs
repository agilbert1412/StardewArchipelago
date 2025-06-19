using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.Secrets;
using StardewValley;
using StardewValley.GameData.GarbageCans;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class GarbageInjections
    {
        public const string FROM_TRASH_KEY = "from_trash";
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual bool TryGetGarbageItem(string id, double dailyLuck, out Item item, out GarbageCanItemData selected, out Random garbageRandom, Action<string> logError = null)
        public static void TryGetGarbageItem_TagItemWithTrash_Postfix(GameLocation __instance, string id, double dailyLuck, ref Item item, GarbageCanItemData selected, Random garbageRandom, Action<string> logError, ref bool __result)
        {
            try
            {
                if (!__result || item == null)
                {
                    return;
                }

                SecretNotesInjections.CheckForSecretNoteGarbageCanItem(id, item);
                item = ReplaceTrashCatalogue(id, item);

                if (item == null)
                {
                    return;
                }

                TagItemWithTrashSource(item);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryGetGarbageItem_TagItemWithTrash_Postfix)}:\n{ex}");
                return;
            }
        }
        private static void TagItemWithTrashSource(Item item)
        {
            item.modData.TryAdd(FROM_TRASH_KEY, true.ToString());
            item.modData[FROM_TRASH_KEY] = true.ToString();
        }
        private static Item ReplaceTrashCatalogue(string id, Item item)
        {
            if (!_archipelago.SlotData.IncludeEndgameLocations)
            {
                return item;
            }

            if (item.QualifiedItemId != QualifiedItemIds.TRASH_CATALOGUE)
            {
                return item;
            }

            var locationName = $"Find Trash Catalogue";
            if (_locationChecker.IsLocationMissing(locationName))
            {
                var itemId = IDProvider.CreateApLocationItemId(locationName);
                var locationItem = ItemRegistry.Create(itemId);
                return locationItem;
            }

            if (_archipelago.HasReceivedItem("Trash Catalogue"))
            {
                return item;
            }

            return null;
        }
    }
}
