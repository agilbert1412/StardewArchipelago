using System;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley;
using StardewValley.GameData.GarbageCans;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    public static class GarbageInjections
    {
        public const string FROM_TRASH_KEY = "from_trash";
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public virtual bool TryGetGarbageItem(string id, double dailyLuck, out Item item, out GarbageCanItemData selected, out Random garbageRandom, Action<string> logError = null)
        public static void TryGetGarbageItem_TagItemWithTrash_Postfix(GameLocation __instance, string id, double dailyLuck, Item item, GarbageCanItemData selected, Random garbageRandom, Action<string> logError, ref bool __result)
        {
            try
            {
                if (!__result || item == null) 
                {
                    return;
                }

                item.modData.TryAdd(FROM_TRASH_KEY, true.ToString());
                item.modData[FROM_TRASH_KEY] = true.ToString();
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryGetGarbageItem_TagItemWithTrash_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
