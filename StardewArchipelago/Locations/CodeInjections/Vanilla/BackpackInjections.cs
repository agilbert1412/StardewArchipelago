﻿using System;
using System.IO;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants.Modded;
using StardewValley;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.PriceMath;
using StardewValley.Objects;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using xTile.Dimensions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using HarmonyLib;
using StardewArchipelago.Textures;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public static class BackpackInjections
    {
        private const int UNINITIALIZED = -1;
        private const int STARTING_BACKPACK_SLOTS = 12;
        private const int SLOTS_PER_ROW = 12;
        private const string SMALL_PACK = "Small Pack";
        private const string LARGE_PACK = "Large Pack";
        private const string DELUXE_PACK = "Deluxe Pack";
        private const string PREMIUM_PACK = "Premium Pack";
        private static readonly string[] BACKPACK_LOCATION_NAMES = new[] { SMALL_PACK, LARGE_PACK, DELUXE_PACK, PREMIUM_PACK };
        private const string PROGRESSIVE_BACKPACK = "Progressive Backpack";

        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static LocationChecker _locationChecker;
        private static BackpackPriceCalculator _priceCalculator;

        private static string[] _backpackLocationsInOrder;

        private const string SLOTS_IDENTIFIER = "[SLOTS]";
        private static readonly string QUESTION_SMALL = "Backpack Upgrade -- 12 slots".Replace("12", SLOTS_IDENTIFIER);
        private static readonly string QUESTION_LARGE = Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question24").Replace("24", SLOTS_IDENTIFIER);
        private static readonly string QUESTION_DELUXE = Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Question36").Replace("36", SLOTS_IDENTIFIER);
        private static readonly string QUESTION_PREMIUM = "Backpack Upgrade -- 48 slots".Replace("48", SLOTS_IDENTIFIER);
        private static readonly string[] QUESTIONS_PER_TIER = new[] { QUESTION_SMALL, QUESTION_LARGE, QUESTION_DELUXE, QUESTION_PREMIUM };

        private const string PRICE_IDENTIFIER = "[PRICE]";
        private static readonly string RESPONSE_PURCHASE_SMALL = "Purchase (400g)".Replace("400", PRICE_IDENTIFIER);
        private static readonly string RESPONSE_PURCHASE_LARGE = Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response2000").Replace("2000", PRICE_IDENTIFIER).Replace("2,000", PRICE_IDENTIFIER).Replace("2.000", PRICE_IDENTIFIER);
        private static readonly string RESPONSE_PURCHASE_DELUXE = Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_Response10000").Replace("10000", PRICE_IDENTIFIER).Replace("10,000", PRICE_IDENTIFIER).Replace("10.000", PRICE_IDENTIFIER);
        private static readonly string RESPONSE_PURCHASE_PREMIUM = "Purchase (50,000g)".Replace("50000", PRICE_IDENTIFIER).Replace("50,000", PRICE_IDENTIFIER).Replace("50.000", PRICE_IDENTIFIER);
        private static readonly string RESPONSE_NO = Game1.content.LoadString("Strings\\Locations:SeedShop_BuyBackpack_ResponseNo");
        private static readonly string[] RESPONSES_PER_TIER = new[] { RESPONSE_PURCHASE_SMALL, RESPONSE_PURCHASE_LARGE, RESPONSE_PURCHASE_DELUXE, RESPONSE_PURCHASE_PREMIUM };

        private static MethodInfo _drawBiggerBackpackMethod;

        public static void Initialize(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _priceCalculator = new BackpackPriceCalculator();
            _backpackLocationsInOrder = GetAllBackpackLocationsInOrder();
            var biggerBackpackModEntryType = AccessTools.TypeByName("BiggerBackpack.ModEntry");
            if (biggerBackpackModEntryType != null)
            {
                _drawBiggerBackpackMethod = AccessTools.Method(biggerBackpackModEntryType, "drawBiggerBackpack");
            }
        }

        private static string[] GetAllBackpackLocationsInOrder()
        {
            var backpackLocations = _locationChecker.GetAllLocations().Where(location => BACKPACK_LOCATION_NAMES.Any(packName => location.Contains(packName)));
            var sortedLocations = backpackLocations
                .OrderBy(GetTier)
                .ThenBy(GetNumberWithinTier);
            return sortedLocations.ToArray();
        }

        private static string GetNextBackpackLocation()
        {
            return _backpackLocationsInOrder.FirstOrDefault(x => _locationChecker.IsLocationMissing(x));
        }

        private static bool CanBuyNextBackpack()
        {
            var nextBackpack = GetNextBackpackLocation();
            if (string.IsNullOrEmpty(nextBackpack))
            {
                return false;
            }

            var indexNextBackpack = Array.IndexOf(_backpackLocationsInOrder, nextBackpack);

            // TODO: Delete this after Beta7. This is to work around a logic bug
            if (_archipelago.SlotData.ToolProgression.HasFlag(ToolProgression.NoStartingTools))
            {
                indexNextBackpack -= 12 / _archipelago.SlotData.BackpackSize;
            }
            return _archipelago.GetReceivedItemCount(PROGRESSIVE_BACKPACK) >= indexNextBackpack;
        }

        private static int GetTier(string location)
        {
            return Array.FindIndex(BACKPACK_LOCATION_NAMES, packName => location.Contains(packName));
        }

        private static int GetNumberWithinTier(string location)
        {
            var lastWord = location.Split(' ').Last();
            if (int.TryParse(lastWord, out var index))
            {
                return index;
            }

            return 1;
        }

        private static int GetNumberPerTier(int tier)
        {
            // The parameter tier might be used when we start to shuffle the first row, as some slots will come for free
            return SLOTS_PER_ROW / _archipelago.SlotData.BackpackSize;
        }

        public static bool AnswerDialogueAction_BackPackPurchase_Prefix(GameLocation __instance, string questionAndAnswer, string[] questionParams, ref bool __result)
        {
            try
            {
                if (questionAndAnswer != "Backpack_Purchase")
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                ConfirmBuyBackpack();

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AnswerDialogueAction_BackPackPurchase_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void ConfirmBuyBackpack()
        {
            if (_archipelago.SlotData.BackpackProgression == BackpackProgression.Vanilla)
            {
                ConfirmBuyBackpackVanilla();
            }
            else
            {
                ConfirmBuyBackpackArchipelago();
            }
        }

        private static void ConfirmBuyBackpackVanilla()
        {
            var numInventorySlots = Game1.player.MaxItems;
            var maxItems = _archipelago.SlotData.Mods.HasMod(ModNames.BIGGER_BACKPACK) ? 48 : 36;
            if (numInventorySlots >= maxItems)
            {
                return;
            }

            var nextUpgradeTier = numInventorySlots / SLOTS_PER_ROW;
            var currentPlaceInRow = numInventorySlots % SLOTS_PER_ROW;
            var placeInTier = (currentPlaceInRow / _archipelago.SlotData.BackpackSize) + 1;
            var slots = numInventorySlots + _archipelago.SlotData.BackpackSize;
            var numberPerTiers = GetNumberPerTier(nextUpgradeTier);
            var price = _priceCalculator.GetPrice(nextUpgradeTier, placeInTier, numberPerTiers);

            if (Game1.player.Money >= price)
            {
                Game1.player.Money -= price;
                Game1.player.increaseBackpackSize(_archipelago.SlotData.BackpackSize);
                Game1.player.holdUpItemThenMessage(new SpecialItem(99, Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708")));
                // Game1.multiplayer.globalChatInfoMessage("BackpackLarge", Game1.player.Name);
            }
        }

        private static void ConfirmBuyBackpackArchipelago()
        {
            if (!CanBuyNextBackpack())
            {
                return;
            }

            var nextLocation = GetNextBackpackLocation();
            var tier = GetTier(nextLocation);
            var backpackNumber = GetNumberWithinTier(nextLocation);
            var numberPerTier = GetNumberPerTier(tier);

            var price = _priceCalculator.GetPrice(tier, backpackNumber, numberPerTier);

            if (Game1.player.Money >= price)
            {
                Game1.player.Money -= price;
                _locationChecker.AddCheckedLocation(nextLocation);
            }
        }

        // public virtual bool performAction(string[] action, Farmer who, Location tileLocation)
        public static bool PerformAction_BuyBackpack_Prefix(GameLocation __instance, string[] action, Farmer who, Location tileLocation, ref bool __result)
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

                if (actionName == "BuyBackpack")
                {
                    PerformActionBuySizedBackPack(__instance);
                    __result = true;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(PerformAction_BuyBackpack_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void PerformActionBuySizedBackPack(GameLocation gameLocation)
        {
            if (_archipelago.SlotData.BackpackProgression == BackpackProgression.Vanilla)
            {
                PerformActionBuySizedBackPackVanilla(gameLocation);
            }
            else
            {
                PerformActionBuySizedBackPackArchipelago(gameLocation);
            }
        }

        private static void PerformActionBuySizedBackPackVanilla(GameLocation gameLocation)
        {
            var numInventorySlots = Game1.player.MaxItems;
            var maxItems = _archipelago.SlotData.Mods.HasMod(ModNames.BIGGER_BACKPACK) ? 48 : 36;
            if (numInventorySlots >= maxItems)
            {
                return;
            }

            var nextUpgradeTier = numInventorySlots / SLOTS_PER_ROW;
            var currentPlaceInRow = numInventorySlots % SLOTS_PER_ROW;
            var placeInTier = (currentPlaceInRow / _archipelago.SlotData.BackpackSize) + 1;
            var slots = numInventorySlots + _archipelago.SlotData.BackpackSize;
            var numberPerTiers = GetNumberPerTier(nextUpgradeTier);

            CreateBuyBackpackDialogue(gameLocation, nextUpgradeTier, placeInTier, numberPerTiers, slots);
        }

        private static void PerformActionBuySizedBackPackArchipelago(GameLocation gameLocation)
        {
            if (!CanBuyNextBackpack())
            {
                return;
            }

            var nextLocation = GetNextBackpackLocation();
            var nextUpgradeTier = GetTier(nextLocation);
            var backpackNumber = GetNumberWithinTier(nextLocation);
            var numberPerTier = GetNumberPerTier(nextUpgradeTier);
            var slots = (SLOTS_PER_ROW * nextUpgradeTier) + (_archipelago.SlotData.BackpackSize * backpackNumber);

            CreateBuyBackpackDialogue(gameLocation, nextUpgradeTier, backpackNumber, numberPerTier, slots);
        }

        private static void CreateBuyBackpackDialogue(GameLocation gameLocation, int nextUpgradeTier, int placeInTier, int numberPerTiers, int slots)
        {
            var price = _priceCalculator.GetPrice(nextUpgradeTier, placeInTier, numberPerTiers);
            var purchaseText = RESPONSES_PER_TIER[nextUpgradeTier].Replace(PRICE_IDENTIFIER, price.ToString());
            var responsePurchase = new Response("Purchase", purchaseText);
            var responseDontPurchase = new Response("Not", RESPONSE_NO);

            var question = QUESTIONS_PER_TIER[nextUpgradeTier].Replace(SLOTS_IDENTIFIER, slots.ToString());
            gameLocation.createQuestionDialogue(
                question,
                new[]
                {
                    responsePurchase,
                    responseDontPurchase,
                }, "Backpack");
        }

        // public override void draw(SpriteBatch b)
        public static bool Draw_SeedShopBackpack_Prefix(SeedShop __instance, SpriteBatch b)
        {
            try
            {
                CallBaseDraw(__instance, b);

                var backpackToDisplay = GetBackpackToDisplay();
                switch (backpackToDisplay)
                {
                    case 0:
                        var smallBackpackTexture = TexturesLoader.GetTexture(Path.Combine("Backpack", "small_pack.png"));
                        DrawBackpackOnCounter(b, smallBackpackTexture, new Rectangle(0, 0, 12, 14));
                        break;
                    case 1:
                        DrawBackpackOnCounter(b, Game1.mouseCursors, new Rectangle(byte.MaxValue, 1436, 12, 14));
                        break;
                    case 2:
                        DrawBackpackOnCounter(b, Game1.mouseCursors, new Rectangle(267, 1436, 12, 14));
                        break;
                    case 3:
                        if (_drawBiggerBackpackMethod != null)
                        {
                            _drawBiggerBackpackMethod.Invoke(null, new object[] { b });
                        }
                        // Display Bigger Backpack
                        break;
                    default:
                        b.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Rectangle(452, 1184, 112, 20)), new Rectangle(258, 1449, 1, 1), Color.White, 0.0f, Vector2.Zero, SpriteEffects.None, 0.1232f);
                        break;
                }

                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_SeedShopBackpack_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void CallBaseDraw(SeedShop seedShop, SpriteBatch b)
        {
            // base.draw(b);
            var gameLocationDrawMethod = typeof(GameLocation).GetMethod("draw", BindingFlags.Instance | BindingFlags.Public, new[] { typeof(SpriteBatch) });
            var functionPointer = gameLocationDrawMethod.MethodHandle.GetFunctionPointer();
            var baseDraw = (Action<SpriteBatch>)Activator.CreateInstance(typeof(Action<SpriteBatch>), seedShop, functionPointer);
            baseDraw(b);
        }

        private static int GetBackpackToDisplay()
        {
            if (!CanBuyNextBackpack())
            {
                return -1;
            }

            var nextBackpack = GetNextBackpackLocation();
            var tier = GetTier(nextBackpack);
            if (tier < 0)
            {
                return -1;
            }

            return tier;
        }

        private static void DrawBackpackOnCounter(SpriteBatch b, Texture2D texture, Rectangle sourceRect)
        {
            var drawPosition = Game1.GlobalToLocal(new Vector2(456f, 1088f));
            b.Draw(texture, drawPosition, sourceRect, Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1232f);
        }

        // public GameMenu(bool playOpeningSound = true)
        /*public static void GameMenuConstructor_UseFakeInventoryPages_Postfix(GameMenu __instance, bool playOpeningSound)
        {
            try
            {
                for (var i = 0; i < __instance.pages.Count; i++)
                {
                    var page = __instance.pages[i];
                    if (page is InventoryPage)
                    {
                        __instance.pages[i] = new FakeInventoryPage(__instance.xPositionOnScreen, __instance.yPositionOnScreen, __instance.width, __instance.height);
                        return;
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GameMenuConstructor_UseFakeInventoryPages_Postfix)}:\n{ex}");
                return;
            }
        }*/

        /*
            [XmlIgnore]
            public int MaxItems
            {
              get => this.maxItems.Value;
              set => this.maxItems.Value = value;
            }
        */

        /*private static int _lastMaxItems = 0;
        public static void MaxItemsGetter_CheckAmount_Postfix(Farmer __instance, ref int __result)
        {
            try
            {

                if (_lastMaxItems != __result)
                {
                    int a = 0;
                }
                _lastMaxItems = __result;
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MaxItemsGetter_CheckAmount_Postfix)}:\n{ex}");
                return;
            }
        }
        public static void MaxItemsSetter_CheckAmount_Postfix(Farmer __instance, int value)
        {
            try
            {

                if (_lastMaxItems != value)
                {
                    int a = 0;
                }
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(MaxItemsSetter_CheckAmount_Postfix)}:\n{ex}");
                return;
            }
        }*/
    }
}
