using HarmonyLib;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Logging;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseManager
    {
        private readonly ModConfig _config;
        private readonly JojaDisabler _jojaDisabler;
        private readonly JojapocalypseShopPatcher _jojapocalypseShopPatcher;
        private readonly JojaPriceCalculator _jojaPriceCalculator;

        public JojapocalypseManager(LogHandler logger, IModHelper modHelper, ModConfig config, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker)
        {
            _config = config;
            _jojaDisabler = new JojaDisabler(logger, modHelper, harmony);
            _jojaPriceCalculator = new JojaPriceCalculator(logger, locationChecker);
            _jojapocalypseShopPatcher = new JojapocalypseShopPatcher(logger, modHelper, harmony, archipelago, locationChecker, jojaLocationChecker, this, _jojaPriceCalculator);
        }

        public void PatchAllJojaLogic()
        {
            if (_config.Jojapocalypse == JojapocalypseSetting.Disabled)
            {
                _jojaDisabler.DisableJojaRouteShortcuts();
                return;
            }

            _jojaDisabler.DisablePerfectionWaivers();
            _jojapocalypseShopPatcher.PatchJojaShops();
        }

        public void OnNewPurchase(string locationName)
        {
            UpdateAllShopPrices();
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
    }

}
