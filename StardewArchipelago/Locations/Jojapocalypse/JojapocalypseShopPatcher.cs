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

        public JojapocalypseShopPatcher(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker, JojapocalypseManager jojapocalypseManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _harmony = harmony;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _jojaLocationChecker = jojaLocationChecker;
            _jojapocalypseManager = jojapocalypseManager;
        }

        public void PatchJojaShops()
        {

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

                const string archipelagoPartnershipText = "Greetings. As part of our partnership with Archipelago";
                DrawMorrisDialogue(nameof(archipelagoPartnershipText), archipelagoPartnershipText,
                    x => OpenJojapocalypseShop());

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckAction_JojapocalypseShops_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void DrawMorrisDialogue(string dialogueKey, string dialogueText, Func<int, bool> answerQuestionBehavior = null)
        {
            JojaMart.Morris.CurrentDialogue.Clear();

            var newDialogue = new Dialogue(JojaMart.Morris, dialogueKey, dialogueText); 
            JojaMart.Morris.CurrentDialogue.Push(newDialogue);

            if (answerQuestionBehavior != null)
            {
                JojaMart.Morris.CurrentDialogue.Peek().answerQuestionBehavior = new Dialogue.onAnswerQuestion(answerQuestionBehavior);
            }
            Game1.drawDialogue(JojaMart.Morris);
        }

        private static bool OpenJojapocalypseShop(IEnumerable<string> locationTagFilters = null)
        {
            if (locationTagFilters == null)
            {
                locationTagFilters = Array.Empty<string>();
            }

            var items = CreateJojapocalypseItems(locationTagFilters);

            Game1.activeClickableMenu = new ShopMenu($"Jojapocalypse_{string.Join("_", locationTagFilters)}", items, 0, "Morris", on_purchase: OnPurchaseJojapocalypseItem);
            return true;
        }

        private static List<ISalable> CreateJojapocalypseItems(IEnumerable<string> locationTagFilters)
        {
            var locations = _archipelago.DataPackageCache.GetAllLocations();
            var locationsMissing = locations.Where(x => _locationChecker.IsLocationMissing(x.Name));
            var locationsFiltered = locationsMissing.Where(x => x.LocationTags.Any(locationTagFilters.Contains));
            var locationsCanPurchaseNow = locationsFiltered.Where(CanPurchaseJojapocalypseLocation);

            var salableItems = new List<ISalable>();
            foreach (var location in locationsCanPurchaseNow)
            {
                salableItems.Add(new JojaObtainableArchipelagoLocation($"Joja {location.Name}", location.Name, _logger, _modHelper, _jojaLocationChecker, _archipelago));
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
            return true;
        }
    }
}
