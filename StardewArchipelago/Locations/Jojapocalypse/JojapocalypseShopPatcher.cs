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
using Microsoft.Xna.Framework.Graphics;
using StardewValley.GameData;
using StardewValley.Logging;
using StardewValley.TokenizableStrings;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Rectangle = xTile.Dimensions.Rectangle;
using Microsoft.Xna.Framework.Audio;
using System.IO;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseShopPatcher
    {
        private const string HOLD_MUSIC_CUE = "hold-music";

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
            RegisterHoldMusic();
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
            _harmony.Patch(
                original: AccessTools.Method(typeof(DefaultPhoneHandler), nameof(DefaultPhoneHandler.CheckForIncomingCall)),
                postfix: new HarmonyMethod(typeof(JojapocalypseShopPatcher), nameof(CheckForIncomingCall_AddJojaAdCalls_Postfix))
            );
            _harmony.Patch(
                original: AccessTools.Method(typeof(Phone), nameof(Phone.GetIncomingCallAction)),
                prefix: new HarmonyMethod(typeof(JojapocalypseShopPatcher), nameof(GetIncomingCallAction_JojaIncomingCall_Prefix))
            );
        }

        private static void RegisterHoldMusic()
        {
            var currentModFolder = _modHelper.DirectoryPath;
            var soundsFolder = "Sounds";
            var fileName = "hold-music.wav";
            var relativePathToSound = Path.Combine(currentModFolder, soundsFolder, fileName);
            var holdMusicCueDefinition = new CueDefinition(HOLD_MUSIC_CUE, SoundEffect.FromFile(relativePathToSound), 0);
            Game1.soundBank.AddCue(holdMusicCueDefinition);
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
                Game1.currentLocation.ShowPagedResponses(Game1.content.LoadString("Strings\\Characters:Phone_SelectNumber"), responses, callId =>
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
                }, addCancel: false, itemsPerPage: 6);
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
            var timeInputNumber = 4950;
            var timeOnHold = Utility.CreateDaySaveRandom().Next(1000, 30000);
            Game1.player.freezePause = timeInputNumber + timeOnHold;
            DelayedAction.functionAfterDelay(() =>
            {
                Game1.playSound("bigSelect");

                var stayOnlineText = "Thank you for calling Jojamart! Please stay on the line, a representative will be with you shortly.";
                DrawMorrisDialogue(nameof(stayOnlineText), stayOnlineText, () =>
                {
                    Game1.playSound(HOLD_MUSIC_CUE, out var cue);

                    DelayedAction.functionAfterDelay(() =>
                    {
                        cue.Stop(AudioStopOptions.Immediate);
                        var purchasingText = "Thank you for calling Jojamart. What will you be purchasing today? Note that there is a delivery fee for phone purchases.";
                        DrawMorrisDialogue(nameof(purchasingText), purchasingText, () => OpenJojapocalypseShop(1.5));
                    }, timeOnHold);
                });
            }, timeInputNumber);
        }

        private static void DrawMorrisDialogue(string dialogueKey, string dialogueText, Action actionAfterFinish = null)
        {
            Morris.CurrentDialogue.Clear();

            var newDialogue = new Dialogue(Morris, dialogueKey, dialogueText);

            if (actionAfterFinish != null)
            {
                newDialogue.onFinish += actionAfterFinish;
            }

            Morris.setNewDialogue(newDialogue);
            Game1.drawDialogue(Morris);
        }

        private static void OpenJojapocalypseShop(double priceMultiplier = 1.0, IEnumerable<string> locationTagFilters = null)
        {
            var dialogueBox = (DialogueBox)Game1.activeClickableMenu;
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

        //public string CheckForIncomingCall(Random random)
        public static void CheckForIncomingCall_AddJojaAdCalls_Postfix(DefaultPhoneHandler __instance, Random random, ref string __result)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(__result))
                {
                    return;
                }

                var chanceOfAd = (_jojaLocationChecker.GetPercentCheckedLocationsByJoja() * 0.25) + 0.02;
                if (random.NextDouble() < chanceOfAd)
                {
                    __result = JojaConstants.JOJA_INCOMING_CALL;
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForIncomingCall_AddJojaAdCalls_Postfix)}:\n{ex}");
                return;
            }
        }

        // public static Action GetIncomingCallAction(string callId)
        public static bool GetIncomingCallAction_JojaIncomingCall_Prefix(string callId, ref Action __result)
        {
            try
            {
                if (callId != JojaConstants.JOJA_INCOMING_CALL)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var dialogueAction = () =>
                {
                    var speaker = Morris;
                    speaker = new NPC(speaker.Sprite, Vector2.Zero, "", 0, speaker.Name, speaker.Portrait, false);
                    speaker.displayName = speaker.displayName;

                    var dialoguePart1 = "Hello! This is your Jojamart customer service representative." +
                                             " We are calling you with a one-time, incredible offer!";
                    var dialoguePart2 = " If you purchase it now, you can be the proud owner of {0} for the modest sum of {1}g!";
                    var offeredLocation = _jojaLocationChecker.GetTodayRandomOfferLocation();
                    _jojaPriceCalculator.SetPriceMultiplier(0.4);
                    var offeredPrice = _jojaPriceCalculator.GetNextItemPrice();
                    var dialoguePart2Resolved = string.Format(dialoguePart2, offeredLocation, offeredPrice);

                    var responsePositive = "What a good deal!";
                    var responseNegative = Game1.content.LoadString("Strings\\Characters:Phone_HangUp");
                    var dialoguePart2WithResponses = $"$y '{dialoguePart2Resolved}_{responsePositive}_Thank you._{responseNegative}_Have a good day.'";

                    var dialogue1 = new Dialogue(speaker, nameof(dialoguePart1), dialoguePart1);
                    var dialogue2 = new Dialogue(speaker, nameof(dialoguePart2WithResponses), dialoguePart2WithResponses);
                    dialogue2.answerQuestionBehavior = OnAnswerPhoneAd;
                    dialogue1.onFinish = () =>
                    {
                        Morris.CurrentDialogue.Clear();
                        Morris.CurrentDialogue.Push(dialogue2);
                        Game1.drawDialogue(Morris);
                    };

                    Morris.CurrentDialogue.Clear();
                    Morris.CurrentDialogue.Push(dialogue1);
                    Game1.drawDialogue(Morris);
                };

                __result = dialogueAction;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetIncomingCallAction_JojaIncomingCall_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool OnAnswerPhoneAd(int whichresponse)
        {
            if (whichresponse == 0)
            {
                var offeredLocation = _jojaLocationChecker.GetTodayRandomOfferLocation();
                _jojaPriceCalculator.SetPriceMultiplier(0.4);
                var offeredPrice = _jojaPriceCalculator.GetNextItemPrice();
                if (Game1.player.Money >= offeredPrice)
                {
                    Game1.player.Money -= offeredPrice;
                    _jojaLocationChecker.AddCheckedLocation(offeredLocation);
                    var responsePurchaseSuccess = "Thank you for doing business with Joja.";
                    DrawMorrisDialogue(responsePurchaseSuccess, responsePurchaseSuccess);
                    return true;
                }

                var responseCantAfford = $"If you cannot afford this item, we can set up a payment plan of {offeredPrice/4} per month for 24 months";
                DrawMorrisDialogue(responseCantAfford, responseCantAfford);
                return true;
            }

            return false;
        }

        private static NPC Morris
        {
            get
            {
                if (JojaMart.Morris == null)
                {
                    var jojaMart = Game1.getLocationFromName(nameof(JojaMart));
                    // protected virtual void resetLocalState()
                    var resetLocalStateMethod = _modHelper.Reflection.GetMethod(jojaMart, "resetLocalState");
                    resetLocalStateMethod.Invoke();
                }
                return JojaMart.Morris;
            }
        }
    }
}
