using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Serialization;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class InventoryInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoWalletDto _wallet;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago, ArchipelagoWalletDto wallet)
        {
            _logger = logger;
            _archipelago = archipelago;
            _wallet = wallet;
        }

        // public void OnItemReceived(Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification = false)
        public static void OnItemReceived_TouchItems_Postfix(Farmer __instance, Item item, int countAdded, Item mergedIntoStack, bool hideHudNotification)
        {
            try
            {
                _wallet.TouchedItems.Add(item.QualifiedItemId);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(OnItemReceived_TouchItems_Postfix)}:\n{ex}");
                return;
            }
        }

        // public bool HasBeenInInventory
        public static void HasBeenInInventoryGet_TouchItems_Postfix(Item __instance, ref bool __result)
        {
            try
            {
                if (__result)
                {
                    _wallet.TouchedItems.Add(__instance.QualifiedItemId);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HasBeenInInventoryGet_TouchItems_Postfix)}:\n{ex}");
                return;
            }
        }

        // public bool HasBeenInInventory
        public static void HasBeenInInventorySet_TouchItems_Postfix(Item __instance, bool value)
        {
            try
            {
                if (value)
                {
                    _wallet.TouchedItems.Add(__instance.QualifiedItemId);
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(HasBeenInInventorySet_TouchItems_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
