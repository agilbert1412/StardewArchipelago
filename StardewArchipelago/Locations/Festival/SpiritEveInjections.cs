using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.Festival
{
    internal class SpiritEveInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        public static bool CheckForAction_SpiritEveChest_Prefix(Chest __instance, Farmer who, bool justCheckingForActivity, ref bool __result)
        {
            try
            {
                if (justCheckingForActivity || __instance.giftbox.Value || __instance.playerChest.Value || Game1.CurrentEvent == null || !Game1.CurrentEvent.isSpecificFestival("fall27"))
                {
                    return true; // run original logic
                }

                if (__instance.Items.Count <= 0 || __instance.Items.Count > 1)
                {
                    return true; // run original logic
                }

                who.currentLocation.playSound("openChest");
                if (__instance.synchronized.Value)
                    __instance.GetMutex().RequestLock(() => __instance.openChestEvent.Fire());
                else
                    __instance.performOpenChest();

                if (_archipelago.HasReceivedItem("Golden Pumpkin"))
                {
                    __instance.Items[0] = ItemRegistry.Create(QualifiedItemIds.GOLDEN_PUMPKIN);
                }
                else
                {
                    __instance.Items[0] = ItemRegistry.Create(QualifiedItemIds.PRIZE_TICKET);
                }

                _locationChecker.AddCheckedLocation(FestivalLocationNames.GOLDEN_PUMPKIN);

                __result = true;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForAction_SpiritEveChest_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
