using HarmonyLib;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.InGameLocations;
using StardewArchipelago.Locations.ShopStockModifiers;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using xTile.Dimensions;
using Object = StardewValley.Object;
using Rectangle = xTile.Dimensions.Rectangle;

namespace StardewArchipelago.Locations.Jojapocalypse
{
    public class JojapocalypseShopPatcher : ShopStockModifier
    {
        private const string JOJA_SUBSHOP_DIALOG_KEY = "Joja_SubShop";
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

        public JojapocalypseShopPatcher(LogHandler logger, IModHelper modHelper, Harmony harmony, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, JojaLocationChecker jojaLocationChecker, StardewItemManager stardewItemManager, JojapocalypseManager jojapocalypseManager, JojaPriceCalculator jojaPriceCalculator) : base(logger, modHelper, archipelago, stardewItemManager)
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
            _modHelper.Events.Content.AssetRequested += OnShopStockRequested;

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

        public void CleanJojaShopEvents()
        {
            _modHelper.Events.Content.AssetRequested -= OnShopStockRequested;
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
            if (locationTagFilters == null)
            {
                locationTagFilters = Array.Empty<string>();
            }

            var items = CreateJojapocalypseItems(priceMultiplier, locationTagFilters);

            if (items.Count < 12)
            {
                OpenJojapocalypseShop(string.Join("_", locationTagFilters), items.Values.ToList());
                return;
            }

            var splitItems = SplitJojapocalypseItems(items);

            DisplayJojaSubShops(splitItems);
        }

        private static void DisplayJojaSubShops(Dictionary<string, List<ISalable>> splitItems)
        {
            var speaker = Morris;
            speaker = new NPC(speaker.Sprite, Vector2.Zero, "", 0, speaker.Name, speaker.Portrait, false);
            speaker.displayName = speaker.displayName;

            var dialogueSubShops = "Which department would you like to speak with?";
            var dialogueSubShopsWithResponses = $"$y '{dialogueSubShops}";
            var keys = new List<(string, List<ISalable>)>();
            foreach (var (subItemsKey, subItems) in splitItems)
            {
                keys.Add((subItemsKey, subItems));
                dialogueSubShopsWithResponses += $"_{subItemsKey}_One Moment.";
            }

            var dialogue1 = new Dialogue(speaker, nameof(dialogueSubShopsWithResponses), dialogueSubShopsWithResponses);
            dialogue1.answerQuestionBehavior = (which) => OnAnswerSubShop(keys[which]);

            Morris.CurrentDialogue.Clear();
            Morris.CurrentDialogue.Push(dialogue1);
            Game1.drawDialogue(Morris);
        }

        private static bool OnAnswerSubShop((string, List<ISalable>) subItems)
        {
            OpenJojapocalypseShop(subItems.Item1, subItems.Item2.OrderBy(x => x.Name).ToList());
            return true;
        }

        private static void OpenJojapocalypseShop(string idDifferenciator, List<ISalable> items)
        {
            var dialogueBox = (DialogueBox)Game1.activeClickableMenu;
            dialogueBox.closeDialogue();
            Game1.activeClickableMenu = new ShopMenu($"Jojapocalypse_{idDifferenciator}", items, 0, "Morris", on_purchase: OnPurchaseJojapocalypseItem);
        }

        private static Dictionary<StardewArchipelagoLocation, ISalable> CreateJojapocalypseItems(double priceMultiplier, IEnumerable<string> locationTagFilters)
        {
            var locations = _archipelago.DataPackageCache.GetAllLocations().ToArray();
            var locationsMissing = locations.Where(x => _locationChecker.IsLocationMissing(x.Name)).ToArray();
            var locationsFiltered = locationsMissing.Where(x => locationTagFilters.All(x.LocationTags.Contains)).ToArray();
            var locationsCanPurchaseNow = locationsFiltered.Where(_jojaFiltering.CanPurchaseJojapocalypseLocation).ToArray();
            var locationsInOrder = locationsCanPurchaseNow.OrderBy(x => x.Name).ToArray();

            var salableItems = new Dictionary<StardewArchipelagoLocation, ISalable>();
            _jojaPriceCalculator.SetPriceMultiplier(priceMultiplier);
            foreach (var location in locationsInOrder)
            {
                salableItems.Add(location, new JojaObtainableArchipelagoLocation($"Joja {location.Name}", location.Name, _logger, _modHelper, _jojaLocationChecker, _archipelago, _jojaPriceCalculator));
            }

            return salableItems;
        }

        private static Dictionary<string, List<ISalable>> SplitJojapocalypseItems(Dictionary<StardewArchipelagoLocation, ISalable> items)
        {
            var salablesByKeyword = new Dictionary<string, List<ISalable>>();
            foreach (var (location, item) in items)
            {
                var words = location.Name.Split(" ");
                foreach (var word in words)
                {
                    var keyword = word.Replace(":", "").Replace("?", "").Replace("(", "").Replace(")", "");
                    if (keyword.Length > 2)
                    {
                        if (!salablesByKeyword.ContainsKey(keyword))
                        {
                            salablesByKeyword.Add(keyword, new List<ISalable>());
                        }
                        if (!salablesByKeyword[keyword].Contains(item))
                        {
                            salablesByKeyword[keyword].Add(item);
                        }
                    }
                }
            }

            var salablesByKeywordOrdered = salablesByKeyword.OrderByDescending(x => x.Value.Count).ToArray();
            const int NUMBER_CATEGORIES_SPLIT = 8;
            const string OTHER_CATEGORY = "Other";
            const string ALL_CATEGORY = "Everything";
            var splitItems = new Dictionary<string, List<ISalable>>();
            var otherItems = new Dictionary<string, ISalable>();
            var alreadyCategorizedItems = new HashSet<string>();
            for (var i = 0; i < salablesByKeywordOrdered.Length; i++)
            {
                var (keyword, keywordItems) = salablesByKeywordOrdered[i];
                if (i < NUMBER_CATEGORIES_SPLIT - 1 && keywordItems.Count > 3)
                {
                    splitItems.Add(keyword, keywordItems);
                    alreadyCategorizedItems.UnionWith(keywordItems.Select(x => x.Name));
                }
                else
                {
                    foreach (var keywordItem in keywordItems)
                    {
                        if (alreadyCategorizedItems.Contains(keywordItem.Name))
                        {
                            continue;
                        }
                        otherItems.TryAdd(keywordItem.Name, keywordItem);
                        alreadyCategorizedItems.Add(keywordItem.Name);
                    }
                }
            }

            splitItems.Add(OTHER_CATEGORY, otherItems.Values.ToList());
            splitItems.Add(ALL_CATEGORY, items.Values.ToList());

            return splitItems;
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
                    var dialogue1 = new Dialogue(speaker, nameof(dialoguePart1), dialoguePart1);

                    if (string.IsNullOrWhiteSpace(offeredLocation))
                    {
                        dialoguePart1 = "Hello! This is your Jojamart customer service representative." +
                                            " Is everything going Jojamazing? Don't hesitate to call us if you need anything!";
                        dialogue1 = new Dialogue(speaker, nameof(dialoguePart1), dialoguePart1);
                    }
                    else
                    {
                        var responsePositive = "What a good deal!";
                        var responseNegative = Game1.content.LoadString("Strings\\Characters:Phone_HangUp");
                        var offeredPrice = _jojaPriceCalculator.GetNextItemPrice();
                        var dialoguePart2Resolved = string.Format(dialoguePart2, offeredLocation.Replace("#", ""), offeredPrice);
                        var dialoguePart2WithResponses = $"$y '{dialoguePart2Resolved}_{responsePositive}_Thank you._{responseNegative}_Have a good day.'";
                        var dialogue2 = new Dialogue(speaker, nameof(dialoguePart2WithResponses), dialoguePart2WithResponses);
                        dialogue2.answerQuestionBehavior = OnAnswerPhoneAd;
                        dialogue1.onFinish = () =>
                        {
                            Morris.CurrentDialogue.Clear();
                            Morris.CurrentDialogue.Push(dialogue2);
                            Game1.drawDialogue(Morris);
                        };
                    }

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
                if (string.IsNullOrWhiteSpace(offeredLocation))
                {
                    return false;
                }

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

        public override void OnShopStockRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!AssetIsShops(e))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var shopsData = asset.AsDictionary<string, ShopData>().Data;
                    var jojaShop = shopsData["Joja"];
                    AddObjectsToJojaShop(jojaShop);
                },
                AssetEditPriority.Late
            );
        }

        private void AddObjectsToJojaShop(ShopData shopData)
        {
            var allApItems = _archipelago.DataPackageCache.GetAllItems().ToDictionary(x => x.Name, x => x);
            var allQualifiedIds = new HashSet<string>();
            for (var i = shopData.Items.Count - 1; i >= 0; i--)
            {
                var item = shopData.Items[i];
                var itemId = string.IsNullOrWhiteSpace(item.ItemId) ? item.ItemId : item.Id;
                if (!ItemRegistry.IsQualifiedItemId(itemId))
                {
                    itemId = ItemRegistry.QualifyItemId(itemId);
                }

                allQualifiedIds.Add(itemId);

                if (!_stardewItemManager.ObjectExistsById(itemId))
                {
                    continue;
                }

                var stardewItem = _stardewItemManager.GetObjectById(itemId);

                if (allApItems.ContainsKey(stardewItem.Name) && (item.Condition == null || !item.Condition.Contains(GameStateCondition.HAS_RECEIVED_ITEM)))
                {
                    item.Condition = item.Condition.AddCondition(GameStateConditionProvider.CreateHasReceivedItemCondition(stardewItem.Name));
                }
            }

            foreach (var stardewItem in _stardewItemManager.GetAllItems())
            {
                var qualifiedId = stardewItem.GetQualifiedId();
                var realItem = ItemRegistry.Create(qualifiedId);
                if (allQualifiedIds.Contains(qualifiedId) || !JojaCanSellItem(qualifiedId, realItem))
                {
                    continue;
                }

                allQualifiedIds.Add(qualifiedId);

                if (allApItems.ContainsKey(stardewItem.Name) && TryGetItemValue(realItem, stardewItem, out var itemValue))
                {
                    var maxStack = realItem.maximumStackSize();
                    var receivedApCondition = GameStateConditionProvider.CreateHasReceivedItemCondition(stardewItem.Name);
                    var receivedApRecipeCondition = GameStateConditionProvider.CreateHasReceivedItemCondition($"{stardewItem.Name} Recipe");
                    var knowsRecipeCondition = GameStateConditionProvider.CreateKnowsAnyRecipeCondition(stardewItem.Name);
                    var condition = GameStateConditionProvider.CreateOrCondition(new[] { receivedApCondition, receivedApRecipeCondition, knowsRecipeCondition });
                    var shopObject = new ShopItemData()
                    {
                        Id = qualifiedId,
                        ItemId = qualifiedId,
                        Price = itemValue * Math.Min(maxStack, 8),
                        IsRecipe = false,
                        MaxItems = 1,
                        MinStack = Math.Min(maxStack, 10),
                        MaxStack = -1,
                        Condition = condition,
                    };

                    shopData.Items.Add(shopObject);
                }
            }
        }

        private bool TryGetItemValue(Item realItem, StardewItem stardewItem, out int itemValue)
        {
            var specialSellPrices = new Dictionary<string, int>()
            {
                { QualifiedItemIds.PRIZE_TICKET, 500 },
                { QualifiedItemIds.GOLDEN_EGG, 50000 },
                { QualifiedItemIds.CACTUS_SEEDS, 75 },
                { QualifiedItemIds.STRAWBERRY_SEEDS, 50 },
                { QualifiedItemIds.ANCIENT_SEEDS, 200 },
            };

            if (specialSellPrices.TryGetValue(stardewItem.GetQualifiedId(), out itemValue))
            {
                return true;
            }

            itemValue = realItem.salePrice();
            var recipe = _stardewItemManager.GetRecipeByName(stardewItem.Name, false);
            if (itemValue > 0 && recipe == null)
            {
                return true;
            }

            if (recipe == null)
            {
                return false;
            }

            var ingredientsPrice = 0;
            foreach (var (ingredientId, ingredientAmount) in recipe.Ingredients)
            {
                var ingredient = ItemRegistry.Create(ingredientId);
                if (!TryGetItemValue(ingredient, _stardewItemManager.GetItemByQualifiedId(ingredient.QualifiedItemId), out var ingredientPrice))
                {
                    return false;
                }
                ingredientsPrice += ingredientPrice * ingredientAmount;
            }

            ingredientsPrice = (int)Math.Round(ingredientsPrice * 1.25); // Service Charge

            if (ingredientsPrice >= 0)
            {
                itemValue = ingredientsPrice;
                return true;
            }

            return false;
        }

        private bool JojaCanSellItem(string qualifiedId, Item realItem)
        {
            var illegalIds = new[] { QualifiedItemIds.STARDROP, QualifiedItemIds.DWARVISH_TRANSLATION_GUIDE, QualifiedItemIds.GOLDEN_WALNUT };
            if (illegalIds.Contains(qualifiedId))
            {
                return false;
            }

            return !realItem.isLostItem &&
                   !realItem.specialItem &&
                   realItem.CanBeLostOnDeath() &&
                   realItem.canBeTrashed() &&
                   realItem.canBeDropped() &&
                   (realItem is not Object realObject || !realObject.questItem.Value);
        }
    }
}
