using HarmonyLib;
using StardewModdingAPI;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseManager
    {
        private readonly ModConfig _config;
        private readonly JojaDisabler _jojaDisabler;
        private readonly JojapocalypseShopPatcher _jojapocalypseShopPatcher;

        public JojapocalypseManager(LogHandler logger, IModHelper modHelper, ModConfig config, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker)
        {
            _config = config;
            _jojaDisabler = new JojaDisabler(logger, modHelper, harmony);
            _jojapocalypseShopPatcher = new JojapocalypseShopPatcher(logger, modHelper, harmony, archipelago, locationChecker, jojaLocationChecker, this);
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

        }
    }

}
