using System;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewModdingAPI;
using StardewValley;
using xTile.Dimensions;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Constants;
using StardewValley.Menus;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Locations.Jojapocalypse;
using System.Collections.Generic;
using StardewValley.GameData.Shops;
using Archipelago.MultiClient.Net.Models;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Constants.Vanilla;
using Object = StardewValley.Object;
using StardewValley.Locations;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class WizardBookInjections
    {
        public const string BUILDING_BLUEPRINT_LOCATION_NAME = "{0} Blueprint";

        public const string EARTH_OBELISK = "Earth Obelisk";
        public const string WATER_OBELISK = "Water Obelisk";
        public const string DESERT_OBELISK = "Desert Obelisk";
        public const string ISLAND_OBELISK = "Island Obelisk";
        public const string JUNIMO_HUT = "Junimo Hut";
        public const string GOLD_CLOCK = "Gold Clock";
        public const string DEEP_WOODS_OBELISK = "Deep Woods Obelisk";

        private static readonly string[] _wizardBuildings = { EARTH_OBELISK, WATER_OBELISK, DESERT_OBELISK, ISLAND_OBELISK, JUNIMO_HUT, GOLD_CLOCK, DEEP_WOODS_OBELISK };

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static StardewLocationChecker _locationChecker;

        public static void Initialize(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_WizardBook_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
        {
            try
            {
                if (action == null || !who.IsLocalPlayer)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (!ArgUtility.TryGet(action, 0, out var actionName, out _, name: "string actionType"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (actionName != "WizardBook")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                __result = true;

                var hasReceivedAnyBuildings = _wizardBuildings.Any(x => _archipelago.HasReceivedItem(x));
                var hasMagicInk = who.hasMagicInk;
                var canPurchaseBlueprints = _wizardBuildings.Any(x => _locationChecker.IsLocationMissing($"{x} Blueprint"));

                if (!hasReceivedAnyBuildings && !canPurchaseBlueprints)
                {
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (hasReceivedAnyBuildings && !canPurchaseBlueprints)
                {
                    __instance.ShowConstructOptions("Wizard");
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (!hasReceivedAnyBuildings && canPurchaseBlueprints)
                {
                    ShowWizardBlueprintsShop();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                ShowChoiceBetweenBuildingsAndShop(__instance);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_WizardBook_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ShowChoiceBetweenBuildingsAndShop(GameLocation gameLocation)
        {
            var responseList = new List<Response>();
            responseList.Add(new Response("Shop", "Shop for Blueprints"));
            responseList.Add(new Response("Construct", "Construct Magical Buildings"));
            responseList.Add(new Response("Leave", "Leave"));
            gameLocation.createQuestionDialogue("Wizard's Magic Book", responseList.ToArray(), "magicbook");
        }

        // public virtual bool answerDialogueAction(string questionAndAnswer, string[] questionParams)
        public static bool AnswerDialogueAction_WizardBook_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (__instance is not WizardHouse || !questionAndAnswer.StartsWith("magicbook"))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                if (questionAndAnswer == "magicbook_Shop")
                {
                    ShowWizardBlueprintsShop();
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                if (questionAndAnswer == "magicbook_Construct")
                {
                    __instance.ShowConstructOptions("Wizard");
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_WizardBook_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ShowWizardBlueprintsShop()
        {
            var items = CreateWizardBlueprintItems();
            Game1.activeClickableMenu = new ShopMenu($"Wizard_Blueprints", items, 0, "Wizard");
        }

        private static Dictionary<ISalable, ItemStockInformation> CreateWizardBlueprintItems()
        {
            var salableItems = new Dictionary<ISalable, ItemStockInformation>();
            var hints = _archipelago.GetMyActiveDesiredHints();
            AddCheckItemToStock(salableItems, EARTH_OBELISK, 500_000, new []{ IridiumBar(10), new Object(ObjectIds.EARTH_CRYSTAL, 10)  }, hints);
            AddCheckItemToStock(salableItems, WATER_OBELISK, 500_000, new[] { IridiumBar(5), new Object(ObjectIds.CLAM, 10), new Object(ObjectIds.CORAL, 10) }, hints);
            AddCheckItemToStock(salableItems, DESERT_OBELISK, 1_000_000, new[] { IridiumBar(20), new Object(ObjectIds.COCONUT, 10), new Object(ObjectIds.CACTUS_FRUIT, 10) }, hints);
            AddCheckItemToStock(salableItems, ISLAND_OBELISK, 1_000_000, new[] { IridiumBar(10), new Object(ObjectIds.DRAGON_TOOTH, 10), new Object(ObjectIds.BANANA, 10) }, hints);
            AddCheckItemToStock(salableItems, JUNIMO_HUT, 20_000, new []{ new Object(ObjectIds.STONE, 200), new Object(ObjectIds.STARFRUIT, 9), new Object(ObjectIds.FIBER, 100) }, hints);
            AddCheckItemToStock(salableItems, GOLD_CLOCK, 10_000_000, Array.Empty<Item>(), hints);

            return salableItems;
        }

        private static void AddCheckItemToStock(Dictionary<ISalable, ItemStockInformation> shopItems, string buildingName, int price, Item[] materials, Hint[] hints)
        {
            var locationName = string.Format(BUILDING_BLUEPRINT_LOCATION_NAME, buildingName);
            if (!_locationChecker.IsLocationMissing(locationName))
            {
                return;
            }

            var id = $"{IDProvider.AP_LOCATION} {locationName}";

            // var priceMultiplier = _archipelago.SlotData.BuildingPriceMultiplier;
            var finalPrice = (int)(price * 1.0);
            var materialsString = string.Join(',', materials.Select(x => GetMaterialString(x, 1.0)));


            var blueprintCheck = new ObtainableArchipelagoLocation(locationName, _logger, _modHelper, _locationChecker, _archipelago, hints, true);
            blueprintCheck.modData.Add(ObtainableArchipelagoLocation.EXTRA_MATERIALS_KEY, materialsString);


            shopItems.Add(blueprintCheck, new ItemStockInformation(finalPrice, 1));
        }

        private static string GetMaterialString(ISalable material, double priceMultiplier)
        {
            var amount = Math.Max(1, (int)Math.Round(material.Stack * priceMultiplier));
            return $"{QualifiedItemIds.UnqualifyId(material.QualifiedItemId)}:{amount}";
        }

        private static Object IridiumBar(int stack)
        {
            return new Object(ObjectIds.IRIDIUM_BAR, stack);
        }
    }
}
