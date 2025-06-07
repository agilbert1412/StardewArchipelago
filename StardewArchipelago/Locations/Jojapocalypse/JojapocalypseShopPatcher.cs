using HarmonyLib;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using xTile.Dimensions;
using StardewArchipelago.Locations.InGameLocations;
using StardewValley.Menus;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseShopPatcher
    {
        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static Harmony _harmony;
        private static StardewArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;
        private static JojaLocationChecker _jojaLocationChecker;
        private static JojapocalypseManager _jojapocalypseManager;
        private static JojaPriceCalculator _jojaPriceCalculator;
        private static JojapocalypseFiltering _jojaFiltering;

        public JojapocalypseShopPatcher(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker, JojapocalypseManager jojapocalypseManager, JojaPriceCalculator jojaPriceCalculator)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _jojaLocationChecker = jojaLocationChecker;
            _jojapocalypseManager = jojapocalypseManager;
            _jojaPriceCalculator = jojaPriceCalculator;
            _jojaFiltering = new JojapocalypseFiltering(_logger, _archipelago, _locationChecker);
        }

        public void PatchJojaShops()
        {
            _harmony.Patch(
                original: AccessTools.Method(typeof(JojaMart), nameof(JojaMart.checkAction)),
                prefix: new HarmonyMethod(typeof(JojapocalypseShopPatcher), nameof(CheckAction_JojapocalypseShops_Prefix))
            );
        }

        // public override bool checkAction(Location tileLocation, xTile.Dimensions.Rectangle viewport, Farmer who)
        public static bool CheckAction_JojapocalypseShops_Prefix(JojaMart __instance, Location tileLocation, Rectangle viewport, Farmer who, ref bool __result)
        {
            try
            {
                if (__instance.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Action", "Buildings") != "JoinJoja")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var archipelagoPartnershipText = "Greetings. As part of our new partnership with Archipelago, we can offer you our services to accomplish any task you or your organisation might need!"+
                                                 "#$b#" +
                                                 "Nothing is off limits, as long as you're ready to pay the price.";
                DrawMorrisDialogue(nameof(archipelagoPartnershipText), archipelagoPartnershipText, () => OpenJojapocalypseShop());

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_JojapocalypseShops_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void DrawMorrisDialogue(string dialogueKey, string dialogueText, Action actionAfterFinish = null)
        {
            JojaMart.Morris.CurrentDialogue.Clear();

            var newDialogue = new Dialogue(JojaMart.Morris, dialogueKey, dialogueText);

            if (actionAfterFinish != null)
            {
                newDialogue.onFinish += actionAfterFinish;
            }

            JojaMart.Morris.setNewDialogue(newDialogue);
            Game1.drawDialogue(JojaMart.Morris);
        }

        private static void OpenJojapocalypseShop(IEnumerable<string> locationTagFilters = null)
        {
            var dialogueBox = ((DialogueBox)(Game1.activeClickableMenu));
            dialogueBox.closeDialogue();
            if (locationTagFilters == null)
            {
                locationTagFilters = Array.Empty<string>();
            }

            var items = CreateJojapocalypseItems(locationTagFilters);

            Game1.activeClickableMenu = new ShopMenu($"Jojapocalypse_{string.Join("_", locationTagFilters)}", items, 0, "Morris", on_purchase: OnPurchaseJojapocalypseItem);
        }

        private static List<ISalable> CreateJojapocalypseItems(IEnumerable<string> locationTagFilters)
        {
            var locations = _archipelago.DataPackageCache.GetAllLocations().ToArray();
            var locationsMissing = locations.Where(x => _locationChecker.IsLocationMissing(x.Name)).ToArray();
            var locationsFiltered = locationsMissing.Where(x => locationTagFilters.All(x.LocationTags.Contains)).ToArray();
            var locationsCanPurchaseNow = locationsFiltered.Where(_jojaFiltering.CanPurchaseJojapocalypseLocation).ToArray();
            var locationsInOrder = locationsCanPurchaseNow.OrderBy(x => x.Name).ToArray();

            var salableItems = new List<ISalable>();
            foreach (var location in locationsInOrder)
            {
                salableItems.Add(new JojaObtainableArchipelagoLocation($"Joja {location.Name}", location.Name, _logger, _modHelper, _jojaLocationChecker, _archipelago, _jojaPriceCalculator));
            }

            return salableItems;
        }

        private static bool OnPurchaseJojapocalypseItem(ISalable salable, Farmer who, int counttaken, ItemStockInformation stock)
        {
            _jojapocalypseManager.OnNewPurchase(((JojaObtainableArchipelagoLocation)salable).LocationName);
            return false;
        }
    }
}
