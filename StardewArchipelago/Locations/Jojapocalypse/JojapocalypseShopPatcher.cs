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
using Netcode;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Locations.InGameLocations;
using StardewValley.Menus;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles;

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

                const string archipelagoPartnershipText = "Greetings. As part of our new partnership with Archipelago, we can offer you our services to accomplish any task you or your organisation might need! Nothing is off limits, as long as you're ready to pay the price.";
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
            JojaMart.Morris.CurrentDialogue.Push(newDialogue);

            if (actionAfterFinish != null)
            {
                JojaMart.Morris.CurrentDialogue.Peek().onFinish = actionAfterFinish;
            }
            Game1.drawDialogue(JojaMart.Morris);
        }

        private static void OpenJojapocalypseShop(IEnumerable<string> locationTagFilters = null)
        {
            if (locationTagFilters == null)
            {
                locationTagFilters = Array.Empty<string>();
            }

            var items = CreateJojapocalypseItems(locationTagFilters);

            Game1.activeClickableMenu = new ShopMenu($"Jojapocalypse_{string.Join("_", locationTagFilters)}", items, 0, "Morris", on_purchase: OnPurchaseJojapocalypseItem);
        }

        private static List<ISalable> CreateJojapocalypseItems(IEnumerable<string> locationTagFilters)
        {
            var locations = _archipelago.DataPackageCache.GetAllLocations();
            var locationsMissing = locations.Where(x => _locationChecker.IsLocationMissing(x.Name));
            var locationsFiltered = locationsMissing.Where(x => locationTagFilters.All(x.LocationTags.Contains));
            var locationsCanPurchaseNow = locationsFiltered.Where(CanPurchaseJojapocalypseLocation);

            var salableItems = new List<ISalable>();
            foreach (var location in locationsCanPurchaseNow)
            {
                salableItems.Add(new JojaObtainableArchipelagoLocation($"Joja {location.Name}", location.Name, _logger, _modHelper, _jojaLocationChecker, _archipelago, _jojaPriceCalculator));
            }

            return salableItems;
        }

        private static bool CanPurchaseJojapocalypseLocation(StardewArchipelagoLocation location)
        {
            var nameWords = location.Name.Split(" ");
            for (var i = 0; i < nameWords.Length; i++)
            {
                if (!int.TryParse(nameWords[i], out var number))
                {
                    continue;
                }

                if (number <= 1)
                {
                    continue;
                }

                for (var j = number - 1; j >= 0; j--)
                {
                    var previousNumberLocationNameWords = new List<string>();
                    previousNumberLocationNameWords.AddRange(nameWords.Take(i));
                    previousNumberLocationNameWords.Add(j.ToString());
                    previousNumberLocationNameWords.AddRange(nameWords.Skip(i + 1));
                    var previousNumberLocation = string.Join(" ", previousNumberLocationNameWords);
                    if (_locationChecker.IsLocationMissing(previousNumberLocation))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool OnPurchaseJojapocalypseItem(ISalable salable, Farmer who, int counttaken, ItemStockInformation stock)
        {
            _jojapocalypseManager.OnNewPurchase(((JojaObtainableArchipelagoLocation)salable).LocationName);
            return false;
        }
    }
}
