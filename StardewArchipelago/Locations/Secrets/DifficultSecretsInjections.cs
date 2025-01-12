using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Constants.Vanilla;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Objects;

namespace StardewArchipelago.Locations.Secrets
{
    public class DifficultSecretsInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public void doneEating()
        public static void DoneEating_StardropFavoriteThing_Postfix(Farmer __instance)
        {
            try
            {
                var itemToEat = __instance.itemToEat as Object;
                if (itemToEat.QualifiedItemId != QualifiedItemIds.STARDROP)
                {
                    return;
                }

                var triggerWords = new string[] { "Kaito", "ConcernedApe", "CA" };
                if (triggerWords.Any(x => __instance.favoriteThing.Value.Contains(x, StringComparison.InvariantCultureIgnoreCase)))
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.THANK_THE_DEVS);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(DoneEating_StardropFavoriteThing_Postfix)}:\n{ex}");
                return;
            }
        }

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static void ReceiveLeftClick_AnnoyMoonMan_Postfix(ShippingMenu __instance, int x, int y, bool playSound)
        {
            try
            {
                // private int timesPokedMoon;
                var timesPokedMoonField = _modHelper.Reflection.GetField<int>(__instance, "timesPokedMoon");
                if (timesPokedMoonField.GetValue() > 10)
                {
                    _locationChecker.AddCheckedLocation(SecretsLocationNames.ANNOY_THE_MOON_MAN);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_AnnoyMoonMan_Postfix)}:\n{ex}");
                return;
            }
        }
    }
}
