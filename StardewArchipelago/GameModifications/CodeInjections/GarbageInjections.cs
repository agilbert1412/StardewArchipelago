using System;
using KaitoKid.ArchipelagoUtilities.Net;
using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewArchipelago.Locations.Secrets;
using StardewValley;
using StardewValley.GameData.GarbageCans;
using StardewValley.Locations;

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
