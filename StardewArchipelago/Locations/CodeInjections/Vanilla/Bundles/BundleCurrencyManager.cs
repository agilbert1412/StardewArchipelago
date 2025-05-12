
using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewArchipelago.Serialization;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData.HomeRenovations;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class BundleCurrencyManager
    {
        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoWalletDto _wallet;
        private ArchipelagoJunimoNoteMenu _menu;

        public BundleCurrencyManager(LogHandler logger, IModHelper modHelper, ArchipelagoWalletDto wallet, ArchipelagoJunimoNoteMenu menu)
        {
            _logger = logger;
            _modHelper = modHelper;
            _wallet = wallet;
            _menu = menu;
        }

        public void DrawCurrency(SpriteBatch b)
        {
            if (!_menu.SpecificBundlePage || !Game1.player.hasOrWillReceiveMail("canReadJunimoText"))
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
                return;
            }

            var ingredient = _menu.CurrentPageBundle.Ingredients.Last();
            var ingredientId = ingredient.id;
            if (ingredientId == IDProvider.MONEY)
            {
                return;
            }

            var amountText = DrawCurrencyAndGetAmount(b, ingredient, ingredientId);
            if (string.IsNullOrWhiteSpace(amountText))
            {
                return;
            }

            DrawText(b, amountText, 936, 292);
        }

        private void DrawText(SpriteBatch b, string text, int relativePositionX, int relativePositionY, SpriteFont font = null)
        {
            if (font == null)
            {
                font = Game1.dialogueFont;
            }
            var textSize = font.MeasureString(text).X;
            var textPosition = new Vector2(_menu.xPositionOnScreen + relativePositionX - textSize / 2f, _menu.yPositionOnScreen + relativePositionY);
            b.DrawString(font, text, textPosition, Game1.textColor * 0.9f);
        }

        private string DrawCurrencyAndGetAmount(SpriteBatch b, BundleIngredientDescription ingredient, string ingredientId)
        {
            var amountText = $"{ingredient.stack}";

            if (ingredientId == IDProvider.QI_GEM)
            {
                Game1.specialCurrencyDisplay.ShowCurrency("qiGems");
                amountText += " Qi Gems";
            }
            else if (ingredientId == IDProvider.QI_COIN)
            {
                SpriteText.drawStringWithScrollBackground(b, Game1.player.clubCoins.ToString(), 64, 16);
                amountText += " Qi Coins";
            }
            else if (ingredientId == IDProvider.STAR_TOKEN)
            {
                DrawStarTokenCurrency();
                amountText += " Star Tokens";
            }
            else if (ingredientId == MemeIDProvider.BLOOD)
            {
                DrawBloodCurrency();
                amountText += " Blood";
            }
            else if (ingredientId == MemeIDProvider.ENERGY)
            {
                DrawEnergyCurrency();
                amountText += " Energy";
            }
            else if (ingredientId == MemeIDProvider.TIME)
            {
                DrawTimeCurrency();
                amountText += " Minutes";
            }
            else if (ingredientId == MemeIDProvider.STEP)
            {
                DrawStepsCurrency();
                amountText += " Steps";
            }
            else if (ingredientId == MemeIDProvider.CLIC)
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
                return "";
            }
            else if (ingredientId == MemeIDProvider.COOKIES_CLICKS)
            {
                DrawCookiesCurrency(b);
                amountText += " Cookies";
            }
            else if (ingredientId == MemeIDProvider.DEATH)
            {
                amountText += " Death";
            }
            else if (ingredientId == MemeIDProvider.COOKIES_CLICKS)
            {
                DrawTimeElapsedCurrency();
                var allowedTime = ArchipelagoJunimoNoteMenu.AllowedMillisecondsOnThisSlot;
                var seconds = allowedTime / 1000;
                var milliseconds = allowedTime % 1000;
                amountText += $"{seconds}:{milliseconds}";
            }
            else
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
                return "";
            }

            return amountText;
        }

        private void DrawStarTokenCurrency()
        {
            DrawSpecialCurrency(_wallet.StarTokens, Game1.mouseCursors, new Rectangle(338, 400, 8, 8));
        }

        private void DrawBloodCurrency()
        {
            DrawSpecialCurrency(_wallet.Blood, _menu.MemeTexture, new Rectangle(58, 0, 5, 9), 4f);
        }

        private void DrawEnergyCurrency()
        {
            DrawSpecialCurrency(_wallet.Energy, Game1.mouseCursors, new Rectangle(1, 412, 14, 14));
        }

        private void DrawTimeCurrency()
        {
            DrawSpecialCurrency(_wallet.Time, Game1.mouseCursors, new Rectangle(434, 475, 9, 9));
        }

        private void DrawTimeElapsedCurrency()
        {
            var elapsedTime = ArchipelagoJunimoNoteMenu.DayStopwatch.ElapsedMilliseconds;
            var elapsedMinutes = elapsedTime / (60 * 1000);
            var totalSeconds = elapsedTime % (60 * 1000);
            var elapsedSeconds = totalSeconds / 1000;
            var elapsedMilliseconds = totalSeconds % 1000;
            DrawSpecialCurrency($"{elapsedMinutes}:{elapsedSeconds}:{elapsedMilliseconds}", Game1.mouseCursors, new Rectangle(434, 475, 9, 9));
        }

        private void DrawStepsCurrency()
        {
            DrawSpecialCurrency((int)Game1.stats.StepsTaken, Game1.mouseCursors, new Rectangle(100, 428, 10, 10));
        }

        private void DrawCookiesCurrency(SpriteBatch spriteBatch)
        {
            DrawSpecialCurrency(_wallet.CookieClicker.GetCookies(), Game1.objectSpriteSheet, new Rectangle(112, 144, 16, 16), 3f);

            var pricesY = 350;
            var amountsY = 410;
            var centeredX = 936;
            var pricesXOffset = 200;
            var amountsXOffset = 100;
            var cursorPriceX = centeredX - pricesXOffset;
            var grandmaPriceX = centeredX + pricesXOffset;
            var cursorAmountX = centeredX - amountsXOffset;
            var grandmaAmountX = centeredX + amountsXOffset;

            var font = Game1.smallFont;

            DrawText(spriteBatch, $"cost: {_wallet.CookieClicker.GetCursorUpgradePrice()}", cursorPriceX, pricesY, font);
            DrawText(spriteBatch, $"{_wallet.CookieClicker.CursorUpgrades}", cursorAmountX, amountsY, font);
            DrawText(spriteBatch, $"{_wallet.CookieClicker.Grandmas}", grandmaAmountX, amountsY, font);
            DrawText(spriteBatch, $"cost: {_wallet.CookieClicker.GetGrandmaUpgradePrice()}", grandmaPriceX, pricesY, font);
        }

        private void DrawDeathCurrency()
        {
            DrawSpecialCurrency(_wallet.Deaths, Game1.mouseCursors, new Rectangle(497, 1776, 14, 16));
        }

        private static void DrawSpecialCurrency(int amountOwned, Texture2D texture, Rectangle sourceRectangle, float scale = 4f)
        {
            DrawSpecialCurrency(amountOwned.ToString(), texture, sourceRectangle, scale);
        }

        private static void DrawSpecialCurrency(string amountOwned, Texture2D texture, Rectangle sourceRectangle, float scale = 4f)
        {
            amountOwned ??= "";
            var spriteBatch = Game1.spriteBatch;
            spriteBatch.End();
            Game1.PushUIMode();
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            var width = 128 + (amountOwned.Length > 3 ? (amountOwned.Length-3) * 16 : 0);
            spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(16, 16, width, 64), Color.Black * 0.75f);
            spriteBatch.Draw(texture, new Vector2(32f, 32f), sourceRectangle, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
            Game1.drawWithBorder(amountOwned, Color.Black, Color.White, new Vector2(72f, (float)(21 + (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8)))), 0.0f, 1f, 1f, false);

            spriteBatch.End();
            Game1.PopUIMode();
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
        }

        public void TryPurchaseCurrentBundle(BundleIngredientDescription ingredient)
        {
            if (ingredient.id == IDProvider.QI_GEM)
            {
                TryPurchaseCurrentBundleWithQiGems(ingredient);
                return;
            }

            if (ingredient.id == IDProvider.QI_COIN)
            {
                TryPurchaseCurrentBundleWithQiCoins(ingredient);
                return;
            }

            if (ingredient.id == IDProvider.STAR_TOKEN)
            {
                TryPurchaseCurrentBundleWithStarTokens(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.BLOOD)
            {
                TryPurchaseCurrentBundleWithBlood(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.ENERGY)
            {
                TryPurchaseCurrentBundleWithEnergy(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.TIME)
            {
                TryPurchaseCurrentBundleWithTime(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.STEP)
            {
                TryPurchaseCurrentBundleWithSteps(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.COOKIES_CLICKS)
            {
                TryPurchaseCurrentBundleWithCookies(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.CLIC)
            {
                TryPurchaseCurrentBundleWithOneClic(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.CLIC)
            {
                TryPurchaseCurrentBundleWithTimeElapsed(ingredient);
                return;
            }
        }

        private void TryPurchaseCurrentBundleWithQiGems(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, Game1.player.QiGems, payAmount => Game1.player.QiGems -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithQiCoins(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, Game1.player.clubCoins, payAmount => Game1.player.clubCoins -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithStarTokens(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.StarTokens, payAmount => _wallet.StarTokens -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithBlood(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.Blood, payAmount => _wallet.Blood -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithEnergy(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.Energy, payAmount => _wallet.Energy -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithTime(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.Time, payAmount => _wallet.Time -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithSteps(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, (int)Game1.stats.StepsTaken, payAmount => Game1.stats.StepsTaken -= (uint)payAmount);
        }

        private void TryPurchaseCurrentBundleWithCookies(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.CookieClicker.GetCookies(), payAmount => _wallet.CookieClicker.SpendCookies(payAmount));
        }

        private void TryPurchaseCurrentBundleWithOneClic(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, 1, _ => {});
        }

        private void TryPurchaseCurrentBundleWithTimeElapsed(BundleIngredientDescription ingredient)
        {
            if (ArchipelagoJunimoNoteMenu.DayStopwatch.ElapsedMilliseconds > ArchipelagoJunimoNoteMenu.AllowedMillisecondsOnThisSlot)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, 1, _ => { });
        }

        private void TryPurchaseCurrentBundleWithWalletCurrency(BundleIngredientDescription ingredient, int amountOwned, Action<int> payAction)
        {
            if (amountOwned < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            payAction(ingredient.stack);

            _menu.PerformCurrencyPurchase();
        }

        public void DonateToBundle(string currencyId)
        {
            if (currencyId == MemeIDProvider.BLOOD)
            {
                const int donationAmount = 25;
                var donation = Math.Max(0, Math.Min(Game1.player.health, donationAmount));
                Game1.player.health -= donationAmount;
                _wallet.Blood += donation;
                return;
            }

            if (currencyId == MemeIDProvider.ENERGY)
            {
                const int donationAmount = 50;
                var donation = Math.Min(Game1.player.Stamina, donationAmount);
                Game1.player.Stamina -= donationAmount;
                _wallet.Energy += (int)donation;
                return;
            }

            if (currencyId == MemeIDProvider.TIME)
            {
                if (Game1.timeOfDay < 2600)
                {
                    Game1.performTenMinuteClockUpdate();
                    _wallet.Time += 10;
                }
                return;
            }
        }
    }
}
