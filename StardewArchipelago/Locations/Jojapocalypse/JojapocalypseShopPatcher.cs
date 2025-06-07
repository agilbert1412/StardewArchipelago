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
using StardewValley.Objects;

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
            _harmony.Patch(
                original: AccessTools.Method(typeof(Game1), nameof(Game1.ShowTelephoneMenu)),
                prefix: new HarmonyMethod(typeof(JojapocalypseShopPatcher), nameof(ShowTelephoneMenu_AddJojaPhoneNumber_Prefix))
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

        // public void ShowTelephoneMenu()
        public static bool ShowTelephoneMenu_AddJojaPhoneNumber_Prefix(Game1 __instance)
        {
            try
            {
                Game1.playSound("openBox");
                if (Game1.IsGreenRainingHere())
                {
                    Game1.drawObjectDialogue("...................");
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }
                var responses = new List<KeyValuePair<string, string>>();
                responses.Add(new KeyValuePair<string, string>(nameof(JojaMart), "Morris"));
                foreach (var phoneHandler in Phone.PhoneHandlers)
                {
                    responses.AddRange(phoneHandler.GetOutgoingNumbers());
                }
                responses.Add(new KeyValuePair<string, string>("HangUp", Game1.content.LoadString("Strings\\Locations:MineCart_Destination_Cancel")));
                Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\Characters:Phone_SelectNumber"), responses, (Action<string>)(callId =>
                {
                    if (callId == "HangUp")
                    {
                        Phone.HangUp();
                    }
                    else
                    {
                        if (TryCallJoja(callId))
                        {
                            return;
                        }
                        foreach (var phoneHandler in Phone.PhoneHandlers)
                        {
                            if (phoneHandler.TryHandleOutgoingCall(callId))
                            {
                                return;
                            }
                        }
                        Phone.HangUp();
                    }
                }), addCancel: false, itemsPerPage: 6);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ShowTelephoneMenu_AddJojaPhoneNumber_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool TryCallJoja(string callId)
        {
            if (callId == nameof(JojaMart))
            {
                CallJojamart();
                return true;
            }

            return false;
        }

        /// <summary>Handle an outgoing call to Jojamart</summary>
        public static void CallJojamart()
        {
            Game1.currentLocation.playShopPhoneNumberSounds("Jojamart");
            Game1.player.freezePause = 4950;
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.playSound("bigSelect");

                var stayOnlineText = "Thank you for calling Jojamart! Please stay on the line, a representative will be with you shortly.";
                DrawMorrisDialogue(nameof(stayOnlineText), stayOnlineText, () =>
                {
                    DelayedAction.functionAfterDelay(() =>
                    {
                        var purchasingText = "Thank you for calling Jojamart. What will you be purchasing today? Note that there is a delivery fee for phone purchases.";
                        DrawMorrisDialogue(nameof(purchasingText), purchasingText, () => OpenJojapocalypseShop(1.5));
                    }, Utility.CreateDaySaveRandom().Next(1000, 30000));
                });
            }, 4950);
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

        private static void OpenJojapocalypseShop(double priceMultiplier = 1.0, IEnumerable<string> locationTagFilters = null)
        {
            var dialogueBox = ((DialogueBox)(Game1.activeClickableMenu));
            dialogueBox.closeDialogue();
            if (locationTagFilters == null)
            {
                locationTagFilters = Array.Empty<string>();
            }

            var items = CreateJojapocalypseItems(priceMultiplier, locationTagFilters);

            Game1.activeClickableMenu = new ShopMenu($"Jojapocalypse_{string.Join("_", locationTagFilters)}", items, 0, "Morris", on_purchase: OnPurchaseJojapocalypseItem);
        }

        private static List<ISalable> CreateJojapocalypseItems(double priceMultiplier, IEnumerable<string> locationTagFilters)
        {
            var locations = _archipelago.DataPackageCache.GetAllLocations().ToArray();
            var locationsMissing = locations.Where(x => _locationChecker.IsLocationMissing(x.Name)).ToArray();
            var locationsFiltered = locationsMissing.Where(x => locationTagFilters.All(x.LocationTags.Contains)).ToArray();
            var locationsCanPurchaseNow = locationsFiltered.Where(_jojaFiltering.CanPurchaseJojapocalypseLocation).ToArray();
            var locationsInOrder = locationsCanPurchaseNow.OrderBy(x => x.Name).ToArray();

            var salableItems = new List<ISalable>();
            _jojaPriceCalculator.SetPriceMultiplier(priceMultiplier);
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
