using HarmonyLib;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Logging;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Locations;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewModdingAPI.Events;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseManager
    {
        private readonly ModConfig _config;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly JojaLocationChecker _jojaLocationChecker;
        private readonly JojaDisabler _jojaDisabler;
        private readonly JojapocalypseShopPatcher _jojapocalypseShopPatcher;
        private readonly JojaPriceCalculator _jojaPriceCalculator;
        private readonly JojapocalypseConsequencesPatcher _jojaConsequencesPatcher;

        public JojapocalypseManager(LogHandler logger, IModHelper modHelper, ModConfig config, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker)
        {
            _config = config;
            _archipelago = archipelago;
            _jojaLocationChecker = jojaLocationChecker;
            _jojaDisabler = new JojaDisabler(logger, modHelper, harmony);
            _jojaPriceCalculator = new JojaPriceCalculator(logger, _archipelago, locationChecker);
            _jojapocalypseShopPatcher = new JojapocalypseShopPatcher(logger, modHelper, harmony, _archipelago, locationChecker, jojaLocationChecker, this, _jojaPriceCalculator);
            _jojaConsequencesPatcher = new JojapocalypseConsequencesPatcher(logger, modHelper, harmony, _archipelago, jojaLocationChecker);
        }

        public void PatchAllJojaLogic()
        {
            if (_archipelago.SlotData.Jojapocalypse.Jojapocalypse == JojapocalypseSetting.Disabled)
            {
                _jojaDisabler.DisableJojaRouteShortcuts();
                return;
            }

            // _jojaDisabler.DisablePerfectionWaivers(); // We allow this on Jojapocalypse?
            _jojapocalypseShopPatcher.PatchJojaShops();
            _jojaConsequencesPatcher.PatchAllConsequences();
        }

        public void OnNewPurchase(string locationName)
        {
            UpdateAllShopPrices();
            SignUpForJojaMembership();
            _jojaLocationChecker.RecountTags();
        }

        private void SignUpForJojaMembership()
        {
            if (_jojaLocationChecker.GetAllLocationsCheckedByJoja().Count < _archipelago.SlotData.Jojapocalypse.PurchasesBeforeMembership)
            {
                return;
            }

            if (Game1.player.hasOrWillReceiveMail(JojaConstants.MEMBERSHIP_MAIL))
            {
                return;
            }

            Game1.addMailForTomorrow(JojaConstants.MEMBERSHIP_MAIL, true, true);
            Game1.player.removeQuest("26");
            Game1.activeClickableMenu?.exitThisMenu();
            JojaMart.Morris.setNewDialogue("Data\\ExtraDialogue:Morris_PlayerSignedUp");
            Game1.drawDialogue(JojaMart.Morris);
        }

        private static void UpdateAllShopPrices()
        {
            if (Game1.activeClickableMenu is not ShopMenu shopMenu)
            {
                return;
            }

            foreach (var (item, stockInfo) in shopMenu.itemPriceAndStock)
            {
                if (item is not JojaObtainableArchipelagoLocation jojaLocation)
                {
                    continue;
                }

                stockInfo.Price = jojaLocation.salePrice();
            }
        }

        public void OnUpdateTicked(UpdateTickedEventArgs updateTickedEventArgs)
        {
            _jojaConsequencesPatcher?.OnUpdateTicked(updateTickedEventArgs);
        }
    }

}
