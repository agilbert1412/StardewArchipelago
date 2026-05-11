using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using KaitoKid.Utilities.Interfaces;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Logging;
using StardewValley.Menus;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.GameModifications.Shops
{
    public static class ShopMenuInjections
    {
        public const string CURRENCY_KEY = "Currency";
        public const string MATERIALS_KEY = "Materials";

        private static ILogger _logger;
        private static IModHelper _helper;
        private static ArchipelagoClient _archipelago;
        private static StardewItemManager _itemManager;
        private static ArchipelagoStateDto _state;

        public static void Initialize(LogHandler logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager stardewItemManager, ArchipelagoStateDto state)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _itemManager = stardewItemManager;
            _state = state;
        }

        // private bool tryToPurchaseItem(ISalable item, ISalable held_item, int stockToBuy, int x, int y)
        public static bool TryToPurchaseItem_ConsiderCurrencyAndMaterials_Prefix(ShopMenu __instance, ISalable item, ISalable held_item, int stockToBuy, int x, int y, ref bool __result)
        {
            try
            {
                if (__instance.readOnly)
                {
                    __result = false;
                    return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                }

                var stock = __instance.itemPriceAndStock[item];
                // protected bool _isStorageShop;
                var isStorageShopField = _helper.Reflection.GetField<bool>(__instance, "_isStorageShop");
                var isStorageShop = isStorageShopField.GetValue();

                if (held_item == null)
                {
                    if (stock.Stock == 0)
                    {
                        __instance.hoveredItem = null;
                        __result = true;
                        return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                    }
                    if (stockToBuy > item.GetSalableInstance().maximumStackSize())
                    {
                        stockToBuy = Math.Max(1, item.GetSalableInstance().maximumStackSize());
                    }
                    var totalPrice = stock.Price * stockToBuy;
                    var tradeItemId = (string)null;
                    var totalTradeItemCount = 5;
                    var stack = stockToBuy * item.Stack;
                    if (stock.TradeItem != null)
                    {
                        tradeItemId = stock.TradeItem;
                        if (stock.TradeItemCount.HasValue)
                        {
                            totalTradeItemCount = stock.TradeItemCount.Value;
                        }
                        totalTradeItemCount *= stockToBuy;
                    }
                    if (CanAfford(__instance, item, Game1.player, stockToBuy, totalPrice) && (tradeItemId == null || __instance.HasTradeItem(tradeItemId, totalTradeItemCount)))
                    {
                        __instance.heldItem = item.GetSalableInstance();
                        __instance.heldItem.Stack = stack;
                        if (!__instance.heldItem.CanBuyItem(Game1.player) && !item.IsInfiniteStock() && !item.IsRecipe)
                        {
                            Game1.playSound("smallSelect");
                            __instance.heldItem = null;
                            __result = false;
                            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                        }
                        if (__instance.CanBuyback() && __instance.buyBackItems.Contains(item))
                        {
                            __instance.BuyBuybackItem(item, totalPrice, stack);
                        }

                        ChargeForPurchase(__instance, item, Game1.player, stockToBuy, totalPrice);

                        if (!string.IsNullOrEmpty(tradeItemId))
                        {
                            __instance.ConsumeTradeItem(tradeItemId, totalTradeItemCount);
                        }
                        if (!isStorageShop && item.actionWhenPurchased(__instance.ShopId))
                        {
                            if (item.IsRecipe)
                            {
                                if (item is Item obj)
                                {
                                    obj.LearnRecipe();
                                }
                                Game1.playSound("newRecipe");
                            }
                            held_item = null;
                            __instance.heldItem = null;
                        }
                        else
                        {
                            // ISSUE: explicit non-virtual call
                            if ((__instance.heldItem is Item heldItem ? heldItem.QualifiedItemId : null) == "(O)858")
                            {
                                Game1.player.team.addQiGemsToTeam.Fire(__instance.heldItem.Stack);
                                __instance.heldItem = null;
                            }
                            if (Game1.mouseClickPolling > 300)
                            {
                                if (__instance.purchaseRepeatSound != null)
                                {
                                    Game1.playSound(__instance.purchaseRepeatSound);
                                }
                            }
                            else if (__instance.purchaseSound != null)
                            {
                                Game1.playSound(__instance.purchaseSound);
                            }
                        }
                        if (stock.Stock != int.MaxValue && !item.IsInfiniteStock())
                        {
                            __instance.HandleSynchedItemPurchase(item, Game1.player, stockToBuy);
                            if (stock.ItemToSyncStack != null)
                            {
                                stock.ItemToSyncStack.Stack = stock.Stock;
                            }
                        }
                        var actionsOnPurchase = stock.ActionsOnPurchase;
                        // ISSUE: explicit non-virtual call
                        if ((actionsOnPurchase != null ? (actionsOnPurchase.Count > 0 ? 1 : 0) : 0) != 0)
                        {
                            foreach (var action in stock.ActionsOnPurchase)
                            {
                                if (!TriggerActionManager.TryRunAction(action, out var error, out var exception1))
                                {
                                    LogTriggerActionError(__instance, item, action, error, exception1);
                                }
                            }
                        }
                        if (__instance.onPurchase != null && __instance.onPurchase(item, Game1.player, stockToBuy, stock))
                        {
                            __instance.exitThisMenu();
                        }
                    }
                    else
                    {
                        CancelShakeMoney(totalPrice);
                    }
                }
                else if (held_item.canStackWith(item))
                {
                    stockToBuy = Math.Min(stockToBuy, (held_item.maximumStackSize() - held_item.Stack) / item.Stack);
                    var stack = stockToBuy * item.Stack;
                    if (stockToBuy > 0)
                    {
                        var totalPrice = stock.Price * stockToBuy;
                        var tradeItemId = (string)null;
                        var totalTradeItemCount = 5;
                        if (stock.TradeItem != null)
                        {
                            tradeItemId = stock.TradeItem;
                            if (stock.TradeItemCount.HasValue)
                            {
                                totalTradeItemCount = stock.TradeItemCount.Value;
                            }
                            totalTradeItemCount *= stockToBuy;
                        }
                        var salableInstance = item.GetSalableInstance();
                        salableInstance.Stack = stack;
                        if (!salableInstance.CanBuyItem(Game1.player))
                        {
                            Game1.playSound("cancel");
                            __result = false;
                            return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
                        }
                        if (CanAfford(__instance, item, Game1.player, totalPrice, stockToBuy) && (tradeItemId == null || __instance.HasTradeItem(tradeItemId, totalTradeItemCount)))
                        {
                            PurchaseItem(__instance, item, stockToBuy, stack, totalPrice, tradeItemId, totalTradeItemCount, stock, isStorageShop);
                        }
                        else
                        {
                            CancelShakeMoney(totalPrice);
                        }
                    }
                }
                if (stock.Stock > 0)
                {
                    __result = false;
                }
                __instance.buyBackItems.Remove(item);
                __instance.hoveredItem = null;

                __result = true;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(TryToPurchaseItem_ConsiderCurrencyAndMaterials_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void PurchaseItem(ShopMenu shop, ISalable item, int amountToPurchase, int stack, int totalPrice, string tradeItemId, int totalTradeItemCount, ItemStockInformation stock, bool isStorageShop)
        {
            shop.heldItem.Stack += stack;
            if (shop.CanBuyback() && shop.buyBackItems.Contains(item))
            {
                shop.BuyBuybackItem(item, totalPrice, stack);
            }

            ChargeForPurchase(shop, item, Game1.player, amountToPurchase, totalPrice);

            if (Game1.mouseClickPolling > 300)
            {
                if (shop.purchaseRepeatSound != null)
                {
                    Game1.playSound(shop.purchaseRepeatSound);
                }
            }
            else if (shop.purchaseSound != null)
            {
                Game1.playSound(shop.purchaseSound);
            }
            if (tradeItemId != null)
            {
                shop.ConsumeTradeItem(tradeItemId, totalTradeItemCount);
            }
            if (!isStorageShop && item.actionWhenPurchased(shop.ShopId))
            {
                shop.heldItem = null;
            }
            if (stock.Stock != int.MaxValue && !item.IsInfiniteStock())
            {
                shop.HandleSynchedItemPurchase(item, Game1.player, amountToPurchase);
                if (stock.ItemToSyncStack != null)
                {
                    stock.ItemToSyncStack.Stack = stock.Stock;
                }
            }
            if (shop.onPurchase != null && shop.onPurchase(item, Game1.player, amountToPurchase, stock))
            {
                shop.exitThisMenu();
            }
        }

        private static void CancelShakeMoney(int price)
        {

            if (price > 0)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 1000;
            }
            Game1.playSound("cancel");
        }

        private static bool CanAfford(ShopMenu shop, ISalable salable, Farmer who, int price, int amountToPurchase)
        {
            return CanAffordCurrency(shop, salable, who, price) && CanAffordMaterials(salable, who, amountToPurchase);
        }

        private static bool CanAffordCurrency(ShopMenu shop, ISalable salable, Farmer who, int price)
        {
            var currencyName = GetCurrencyName(shop, salable);

            var currencyOwned = GetOwnedOfCurrency(currencyName, who);
            return currencyOwned >= price;
        }

        private static int GetOwnedOfCurrency(string currencyName, Farmer who)
        {
            switch (currencyName)
            {
                case "Money":
                    return who.Money;
                case "Star Token":
                    if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival && Game1.CurrentEvent.isSpecificFestival("fall16"))
                    {
                        return who.festivalScore + _state.Wallet.StarTokens;
                    }
                    return _state.Wallet.StarTokens;
                case "Qi Coin":
                    return who.clubCoins;
                case "Qi Gem":
                    return who.QiGems;
                default:
                    throw new ArgumentException($"Invalid Currency: {currencyName}");
            }
        }

        private static string GetCurrencyName(ShopMenu shop, ISalable salable)
        {
            if (salable is Item salableItem && salableItem.modData.ContainsKey(CURRENCY_KEY))
            {
                return salableItem.modData[CURRENCY_KEY];
            }
            return shop.currency switch
            {
                0 => "Money",
                1 => "Star Token",
                2 => "Qi Coin",
                4 => "Qi Gem",
            };
        }

        private static int GetCurrencyIndex(ShopMenu shop, ISalable salable)
        {
            if (salable is Item salableItem && salableItem.modData.ContainsKey(CURRENCY_KEY))
            {
                return salableItem.modData[CURRENCY_KEY] switch
                {
                    "Money" => 0,
                    "Star Token" => 1,
                    "Qi Coin" => 2,
                    "Qi Gem" => 4,
                };
            }

            return shop.currency;
        }

        private static bool CanAffordMaterials(ISalable salable, Farmer who, int amountToPurchase)
        {
            var materials = GetMaterialsPrice(salable);
            return CanAffordMaterials(who, materials, amountToPurchase);
        }

        private static bool CanAffordMaterials(Farmer who, Dictionary<string, int> materials, int amountToPurchase)
        {
            foreach (var (identifier, amount) in materials)
            {
                if (!CanAffordMaterial(who, identifier, amount, amountToPurchase))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool CanAffordMaterial(Farmer who, string materialId, int materialAmount, int amountToPurchase)
        {
            return who.Items.CountId(materialId) >= materialAmount * amountToPurchase;

        }

        private static Dictionary<string, int> GetMaterialsPrice(ISalable salable)
        {
            if (salable is not Item salableItem || !salableItem.modData.ContainsKey(MATERIALS_KEY))
            {
                return new Dictionary<string, int>();
            }

            var materials = JsonConvert.DeserializeObject<Dictionary<string, int>>(salableItem.modData[MATERIALS_KEY]);
            return materials;
        }

        private static void ChargeForPurchase(ShopMenu shop, ISalable salable, Farmer who, int amountToPurchase, int totalPrice)
        {
            ChargeCurrency(shop, salable, who, totalPrice);
            ChargeMaterials(salable, who, amountToPurchase);
        }

        private static void ChargeCurrency(ShopMenu shop, ISalable salable, Farmer who, int totalPrice)
        {
            var currency = "";
            if (salable is Item salableItem && salableItem.modData.ContainsKey(CURRENCY_KEY))
            {
                currency = salableItem.modData[CURRENCY_KEY];
            }
            else
            {
                currency = shop.currency switch
                {
                    0 => "Money",
                    1 => "Star Token",
                    2 => "Qi Coin",
                    4 => "Qi Gem",
                };
            }

            switch (currency)
            {
                case "Money":
                    who.Money -= totalPrice;
                    return;
                case "Star Token":
                    if (Game1.CurrentEvent != null && Game1.CurrentEvent.isFestival && Game1.CurrentEvent.isSpecificFestival("fall16"))
                    {
                        who.festivalScore -= totalPrice;
                        if (who.festivalScore < 0)
                        {
                            var valueToGetFromWallet = -who.festivalScore;
                            _state.Wallet.StarTokens -= valueToGetFromWallet;
                            who.festivalScore += valueToGetFromWallet;
                        }
                        return;
                    }
                    _state.Wallet.StarTokens -= totalPrice;
                    return;
                case "Qi Coin":
                    who.clubCoins -= totalPrice;
                    return;
                case "Qi Gem":
                    who.QiGems -= totalPrice;
                    return;
                default:
                    throw new ArgumentException($"Invalid Currency: {currency}");
            }
        }

        private static void ChargeMaterials(ISalable salable, Farmer who, int amountToPurchase)
        {
            var materials = GetMaterialsPrice(salable);
            foreach (var (qualifiedId, amount) in materials)
            {
                Game1.player.Items.ReduceId(qualifiedId, amount * amountToPurchase);
            }

            return;
        }

        private static void LogTriggerActionError(ShopMenu __instance, ISalable item, string action, string error, Exception exception1)
        {
            var interpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 4);
            interpolatedStringHandler.AppendLiteral("Shop ");
            interpolatedStringHandler.AppendFormatted(__instance.ShopId);
            interpolatedStringHandler.AppendLiteral(" ignored invalid action '");
            interpolatedStringHandler.AppendFormatted(action);
            interpolatedStringHandler.AppendLiteral("' on purchase of item '");
            interpolatedStringHandler.AppendFormatted(item.QualifiedItemId);
            interpolatedStringHandler.AppendLiteral("': ");
            interpolatedStringHandler.AppendFormatted(error);
            var stringAndClear = interpolatedStringHandler.ToStringAndClear();
            var exception2 = exception1;
            _logger.LogError(stringAndClear, exception2);
        }

        // public override void draw(SpriteBatch b)
        public static bool Draw_ConsiderCurrencyAndMaterials_Prefix(ShopMenu __instance, SpriteBatch b)
        {
            try
            {
                DrawMenuBoxes(__instance, b);
                DrawCurrency(__instance, b);
                DrawForSaleItems(__instance, b);
                DrawOutOfStock(__instance, b);
                DrawInventory(__instance, b);
                DrawAnimations(__instance, b);
                DrawArrows(__instance, b);
                DrawTabButtons(__instance, b);
                DrawScrollBar(__instance, b);
                DrawHoverText(__instance, b);
                DrawHeldItem(__instance, b);
                BaseDraw(__instance, b);
                DrawMerchantPortraits(__instance, b);

                __instance.drawMouse(b);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_ConsiderCurrencyAndMaterials_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void DrawCurrency(ShopMenu menu, SpriteBatch b)
        {
            menu.drawCurrency(b);
        }

        private static void DrawMenuBoxes(ShopMenu menu, SpriteBatch b)
        {
            if (!Game1.options.showMenuBackground && !Game1.options.showClearBackgrounds)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            }
            var visualTheme = menu.VisualTheme;
            IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(384, 373, 18, 18), menu.xPositionOnScreen + menu.width - menu.inventory.width - 32 - 24, menu.yPositionOnScreen + menu.height - 256 + 40, menu.inventory.width + 56, menu.height - 448 + 20, Color.White, 4f);
            IClickableMenu.drawTextureBox(b, visualTheme.WindowBorderTexture, visualTheme.WindowBorderSourceRect, menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height - 256 + 32 + 4, Color.White, 4f);
        }

        private static void DrawForSaleItems(ShopMenu menu, SpriteBatch b)
        {
            for (var index = 0; index < menu.forSaleButtons.Count; ++index)
            {
                DrawForSaleItem(menu, b, index);
            }
        }

        private static void DrawForSaleItem(ShopMenu menu, SpriteBatch b, int index)
        {
            var forSaleButton = menu.forSaleButtons[index];
            if (menu.currentItemIndex + index >= menu.forSale.Count)
            {
                return;
            }

            var flag1 = menu.canPurchaseCheck != null && !menu.canPurchaseCheck(menu.currentItemIndex + index);
            var salableItem = menu.forSale[menu.currentItemIndex + index];
            var stockInfo = menu.itemPriceAndStock[salableItem];
            var stackDrawType = menu.GetStackDrawType(stockInfo, salableItem);
            var s1 = salableItem.DisplayName;
            var visualTheme = menu.VisualTheme;
            // private bool scrolling;
            var scrollingField = _helper.Reflection.GetField<bool>(menu, "scrolling");
            var scrolling = scrollingField.GetValue();
            IClickableMenu.drawTextureBox(b, visualTheme.ItemRowBackgroundTexture, visualTheme.ItemRowBackgroundSourceRect, forSaleButton.bounds.X, forSaleButton.bounds.Y, forSaleButton.bounds.Width, forSaleButton.bounds.Height, !forSaleButton.containsPoint(Game1.getOldMouseX(), Game1.getOldMouseY()) || scrolling ? Color.White : visualTheme.ItemRowBackgroundHoverColor, 4f, false);
            if (salableItem.Stack > 1)
            {
                s1 = s1 + " x" + salableItem.Stack.ToString();
            }
            if (salableItem.ShouldDrawIcon())
            {
                DrawSaleItemIcon(menu, b, forSaleButton, flag1, stockInfo, salableItem, stackDrawType, s1);
            }
            else
            {
                SpriteText.drawString(b, s1, forSaleButton.bounds.X + 32 + 8, forSaleButton.bounds.Y + 28, alpha: flag1 ? 0.5f : 1f, color: visualTheme.ItemRowTextColor);
            }
            var right = forSaleButton.bounds.Right;
            var y1 = forSaleButton.bounds.Y + 28 - 4;
            var y2 = forSaleButton.bounds.Y + 44;
            DrawSaleItemPrice(menu, b, salableItem, stockInfo, forSaleButton, flag1, ref right, ref y1, ref y2);
            DrawSaleItemTradeItems(menu, b, salableItem, index, stockInfo, ref right, y1, y2);
        }

        private static void DrawSaleItemIcon(ShopMenu menu, SpriteBatch b, ClickableComponent forSaleButton, bool flag1, ItemStockInformation stockInfo, ISalable key, StackDrawType stackDrawType, string s1)
        {
            var visualTheme = menu.VisualTheme;
            b.Draw(visualTheme.ItemIconBackgroundTexture, new Vector2(forSaleButton.bounds.X + 32 - 12, forSaleButton.bounds.Y + 24 - 4), new Rectangle?(visualTheme.ItemIconBackgroundSourceRect), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            var location = new Vector2(forSaleButton.bounds.X + 32 - 8, forSaleButton.bounds.Y + 24);
            var color = Color.White * (!flag1 ? 1f : 0.25f);
            var stock = stockInfo.Stock;
            key.drawInMenu(b, location, 1f, 1f, 0.9f, StackDrawType.HideButShowQuality, color, true);
            if (stock != int.MaxValue && menu.ShopId != "ClintUpgrade" && (stackDrawType == StackDrawType.Draw && stock > 1 || stackDrawType == StackDrawType.Draw_OneInclusive))
            {
                Utility.drawTinyDigits(stock, b, location + new Vector2(64 - Utility.getWidthOfTinyDigitString(stock, 3f) + 3, 47f), 3f, 1f, color);
            }
            if (menu.buyBackItems.Contains(key))
            {
                b.Draw(Game1.mouseCursors2, new Vector2(forSaleButton.bounds.X + 32 - 8, forSaleButton.bounds.Y + 24), new Rectangle?(new Rectangle(64, 240, 16, 16)), Color.White * (!flag1 ? 1f : 0.25f), 0.0f, new Vector2(8f, 8f), 4f, SpriteEffects.None, 1f);
            }
            var s2 = s1;
            var flag2 = stockInfo.Price > 0;
            if (SpriteText.getWidthOfString(s2) > menu.width - (flag2 ? 150 + SpriteText.getWidthOfString(stockInfo.Price.ToString() + " ") : 100) && s2.Length > (flag2 ? 27 : 37))
            {
                s2 = s2.Substring(0, flag2 ? 27 : 37) + "...";
            }
            SpriteText.drawString(b, s2, forSaleButton.bounds.X + 96 + 8, forSaleButton.bounds.Y + 28, alpha: flag1 ? 0.5f : 1f, color: visualTheme.ItemRowTextColor);
        }

        private static void DrawSaleItemPrice(ShopMenu menu, SpriteBatch b, ISalable salableItem, ItemStockInformation stockInfo, ClickableComponent forSaleButton, bool flag1, ref int right, ref int y1, ref int y2)
        {
            if (stockInfo.Price <= 0)
            {
                return;
            }

            var canAffordItem = CanAffordCurrency(menu, salableItem, Game1.player, stockInfo.Price);
            var currencyIndex = GetCurrencyIndex(menu, salableItem);
            SpriteText.drawString(b, stockInfo.Price.ToString() + " ", right - SpriteText.getWidthOfString(stockInfo.Price.ToString() + " ") - 60, forSaleButton.bounds.Y + 28, alpha: !canAffordItem || flag1 ? 0.5f : 1f, color: menu.VisualTheme.ItemRowTextColor);
            Utility.drawWithShadow(b, Game1.mouseCursors, new Vector2(forSaleButton.bounds.Right - 52, forSaleButton.bounds.Y + 40 - 4), new Rectangle(193 + currencyIndex * 9, 373, 9, 10), Color.White * (!flag1 ? 1f : 0.25f), 0.0f, Vector2.Zero, 4f, shadowIntensity: !flag1 ? 0.35f : 0.0f);
            right -= SpriteText.getWidthOfString(stockInfo.Price.ToString() + " ") + 68;
            y1 = forSaleButton.bounds.Y + 20;
            y2 = forSaleButton.bounds.Y + 28;
        }

        private static void DrawSaleItemTradeItems(ShopMenu menu, SpriteBatch b, ISalable salableItem, int index, ItemStockInformation stockInfo, ref int right, int y1, int y2)
        {
            if (stockInfo.TradeItem != null)
            {
                DrawSaleItemTradeItem(menu, b, index, stockInfo.TradeItem, stockInfo.TradeItemCount, right, y1, y2);
                return;
            }

            var materials = GetMaterialsPrice(salableItem);

            if (materials.Count == 1)
            {
                DrawSaleItemTradeItem(menu, b, index, materials.First().Key, materials.First().Value, right, y1, y2);
            }

            if (materials.Count > 1)
            {
                DrawSaleItemMaterials(menu, b, index, materials, ref right, y1, y2);
            }
        }

        private static void DrawSaleItemTradeItem(ShopMenu menu, SpriteBatch b, int index, string tradeItemId, int? tradeItemCount, int right, int y1, int y2)
        {
            var count = 5;
            if (tradeItemId != null && tradeItemCount.HasValue)
            {
                count = tradeItemCount.Value;
            }
            var flag3 = menu.HasTradeItem(tradeItemId, count);
            if (menu.canPurchaseCheck != null && !menu.canPurchaseCheck(menu.currentItemIndex + index))
            {
                flag3 = false;
            }
            var widthOfString = (float)SpriteText.getWidthOfString("x" + count.ToString());
            var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(tradeItemId);
            var texture = dataOrErrorItem.GetTexture();
            var sourceRect = dataOrErrorItem.GetSourceRect();
            Utility.drawWithShadow(b, texture, new Vector2(right - 88 - widthOfString, y1), sourceRect, Color.White * (flag3 ? 1f : 0.25f), 0.0f, Vector2.Zero, shadowIntensity: flag3 ? 0.35f : 0.0f);
            SpriteText.drawString(b, "x" + count.ToString(), right - (int)widthOfString - 22, y2, alpha: flag3 ? 1f : 0.5f, color: menu.VisualTheme.ItemRowTextColor);
        }

        private static void DrawSaleItemMaterials(ShopMenu menu, SpriteBatch b, int index, Dictionary<string, int> materials, ref int right, int y1, int y2)
        {
            var canPurchaseAnything = !(menu.canPurchaseCheck != null && !menu.canPurchaseCheck(menu.currentItemIndex + index));

            foreach (var (materialId, materialCount) in materials.Reverse())
            {
                var canAfford = canPurchaseAnything && CanAffordMaterial(Game1.player, materialId, materialCount, 1);
                var dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(materialId);
                var texture = dataOrErrorItem.GetTexture();
                var sourceRect = dataOrErrorItem.GetSourceRect();
                Utility.drawWithShadow(b, texture, new Vector2(right - 88, y1), sourceRect, Color.White * (canAfford ? 1f : 0.25f), 0.0f, Vector2.Zero, shadowIntensity: canAfford ? 0.35f : 0.0f);
                right -= 60;
            }
        }

        private static void DrawOutOfStock(ShopMenu menu, SpriteBatch b)
        {
            if (menu.IsOutOfStock())
            {
                var s = Game1.content.LoadString("Strings\\StringsFromCSFiles:ShopMenu.cs.11583");
                SpriteText.drawString(b, s, menu.xPositionOnScreen + menu.width / 2 - SpriteText.getWidthOfString(s) / 2, menu.yPositionOnScreen + menu.height / 2 - 128, color: menu.VisualTheme?.ItemRowTextColor);
            }
        }

        private static void DrawInventory(ShopMenu menu, SpriteBatch b)
        {

            menu.inventory.draw(b);
        }

        private static void DrawAnimations(ShopMenu menu, SpriteBatch b)
        {
            // private TemporaryAnimatedSpriteList animations;
            var animationsField = _helper.Reflection.GetField<TemporaryAnimatedSpriteList>(menu, "animations");
            var animations = animationsField.GetValue();

            for (var index = animations.Count - 1; index >= 0; --index)
            {
                if (animations[index].update(Game1.currentGameTime))
                {
                    animations.RemoveAt(index);
                }
                else
                {
                    animations[index].draw(b, true);
                }
            }
        }

        private static void DrawArrows(ShopMenu menu, SpriteBatch b)
        {
            // private TemporaryAnimatedSprite poof;
            var poofField = _helper.Reflection.GetField<TemporaryAnimatedSprite>(menu, "poof");
            var poof = poofField.GetValue();

            poof?.draw(b);
            menu.upArrow.draw(b);
            menu.downArrow.draw(b);
        }

        private static void DrawTabButtons(ShopMenu menu, SpriteBatch b)
        {
            foreach (ClickableTextureComponent tabButton in menu.tabButtons)
            {
                tabButton.draw(b);
            }
        }

        private static void DrawScrollBar(ShopMenu menu, SpriteBatch b)
        {
            // private Rectangle scrollBarRunner;
            var scrollBarRunnerField = _helper.Reflection.GetField<Rectangle>(menu, "scrollBarRunner");
            var scrollBarRunner = scrollBarRunnerField.GetValue();

            if (menu.forSale.Count > 4)
            {
                IClickableMenu.drawTextureBox(b, menu.VisualTheme.ScrollBarBackTexture, menu.VisualTheme.ScrollBarBackSourceRect, scrollBarRunner.X, scrollBarRunner.Y, scrollBarRunner.Width, scrollBarRunner.Height, Color.White, 4f);
                menu.scrollBar.draw(b);
            }
        }

        private static void DrawHoverText(ShopMenu menu, SpriteBatch b)
        {
            if (string.IsNullOrWhiteSpace(menu.hoverText))
            {
                return;
            }
            
            var hoveredItem1 = menu.hoveredItem as Item;
            var hoveredItem2 = menu.hoveredItem;
            var materials = GetMaterialsPrice(hoveredItem2);

            var materialsHoverText = AddMaterialsToDescription(menu.hoverText, materials);

            // private string getHoveredItemExtraItemIndex()
            var getHoveredItemExtraItemIndexMethod = _helper.Reflection.GetMethod(menu, "getHoveredItemExtraItemIndex");
            var hoveredItemExtraItemIndex = getHoveredItemExtraItemIndexMethod.Invoke<string>();

            // private int getHoveredItemExtraItemAmount()
            var getHoveredItemExtraItemAmountMethod = _helper.Reflection.GetMethod(menu, "getHoveredItemExtraItemAmount");
            var hoveredItemExtraItemAmount = getHoveredItemExtraItemAmountMethod.Invoke<int>();

            if ((hoveredItem2 != null ? (hoveredItem2.IsRecipe ? 1 : 0) : 0) != 0)
            {
                IClickableMenu.drawToolTip(b, " ", menu.boldTitleText, hoveredItem1, menu.heldItem != null, currencySymbol: menu.currency, extraItemToShowIndex: hoveredItemExtraItemIndex, extraItemToShowAmount: hoveredItemExtraItemAmount, craftingIngredients: new CraftingRecipe(hoveredItem1?.BaseName ?? menu.hoveredItem.Name), moneyAmountToShowAtBottom: menu.hoverPrice > 0 ? menu.hoverPrice : -1);
            }
            else
            {
                IClickableMenu.drawToolTip(b, materialsHoverText, menu.boldTitleText, hoveredItem1, menu.heldItem != null, currencySymbol: menu.currency, extraItemToShowIndex: hoveredItemExtraItemIndex, extraItemToShowAmount: hoveredItemExtraItemAmount, moneyAmountToShowAtBottom: menu.hoverPrice > 0 ? menu.hoverPrice : -1);
            }
        }

        private static string AddMaterialsToDescription(string description, Dictionary<string, int> materials)
        {
            if (materials == null || !materials.Any())
            {
                return description;
            }

            var descriptionWithExtraMaterials = $"{description}{Environment.NewLine}";
            foreach (var (qualifiedId, amount) in materials)
            {
                var resolvedItem = ItemRegistry.Create(qualifiedId, amount);
                descriptionWithExtraMaterials += $"{Environment.NewLine}{amount} {resolvedItem.Name}";
            }

            return descriptionWithExtraMaterials;
        }

        private static void DrawHeldItem(ShopMenu menu, SpriteBatch b)
        {

            menu.heldItem?.drawInMenu(b, new Vector2(Game1.getOldMouseX() + 8, Game1.getOldMouseY() + 8), 1f, 1f, 0.9f, StackDrawType.Draw, Color.White, true);
        }

        private static void DrawMerchantPortraits(ShopMenu menu, SpriteBatch b)
        {
            var x = menu.xPositionOnScreen - 320;
            if (x <= 0 || !Game1.options.showMerchantPortraits)
            {
                return;
            }

            if (menu.portraitTexture != null)
            {
                Utility.drawWithShadow(b, menu.VisualTheme.PortraitBackgroundTexture, new Vector2(x, menu.yPositionOnScreen), menu.VisualTheme.PortraitBackgroundSourceRect, Color.White, 0.0f, Vector2.Zero, 4f, layerDepth: 0.91f);
                if (menu.portraitTexture != null)
                {
                    b.Draw(menu.portraitTexture, new Vector2(x + 20, menu.yPositionOnScreen + 20), new Rectangle?(new Rectangle(0, 0, 64, 64)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.92f);
                }
            }
            if (menu.potraitPersonDialogue != null)
            {
                var overrideX = menu.xPositionOnScreen - (int)Game1.dialogueFont.MeasureString(menu.potraitPersonDialogue).X - 64;
                if (overrideX > 0)
                {
                    IClickableMenu.drawHoverText(b, menu.potraitPersonDialogue, Game1.dialogueFont, overrideX: overrideX, overrideY: menu.yPositionOnScreen + (menu.portraitTexture != null ? 312 : 0), boxTexture: menu.VisualTheme.DialogueBackgroundTexture, boxSourceRect: new Rectangle?(menu.VisualTheme.DialogueBackgroundSourceRect), textColor: menu.VisualTheme.DialogueColor, textShadowColor: menu.VisualTheme.DialogueShadowColor);
                }
            }
        }

        private static void BaseDraw(ShopMenu menu, SpriteBatch b)
        {
            if (menu.upperRightCloseButton == null || !menu.shouldDrawCloseButton())
            {
                return;
            }

            menu.upperRightCloseButton.draw(b);
        }
    }
}
