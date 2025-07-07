using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Gacha;
using StardewValley.Menus;
using StardewValley;
using StardewArchipelago.Logging;
using StardewModdingAPI;
using StardewArchipelago.Serialization;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewModdingAPI.Events;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class BundleCurrencyManager
    {
        public static int BALD_HAIR = 52;

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoWalletDto _wallet;
        private static BankHandler _bank;
        private ArchipelagoJunimoNoteMenu _menu;

        public BundleCurrencyManager(LogHandler logger, IModHelper modHelper, ArchipelagoWalletDto wallet, BankHandler bank, ArchipelagoJunimoNoteMenu menu)
        {
            _logger = logger;
            _modHelper = modHelper;
            _wallet = wallet;
            _bank = bank;
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

            var memeBundlesThatShouldDisplayMoney = new[] { MemeBundleNames.COMMUNISM, MemeBundleNames.CLICKBAIT };
            if (_menu.CurrentPageBundle.name == MemeBundleNames.SCAM || _menu.CurrentPageBundle.name == MemeBundleNames.INVESTMENT)
            {
                Game1.dayTimeMoneyBox.drawMoneyBox(b);
                DrawInvestmentOpportunityLabel(b);
                amountText += "g";
            }
            else if (_menu.CurrentPageBundle.name == MemeBundleNames.GACHA)
            {
                Game1.dayTimeMoneyBox.drawMoneyBox(b);
                DrawGachaPrices(b);
                amountText = "";
            }
            else if(_menu.CurrentPageBundle.name == MemeBundleNames.HUMBLE)
            {
                Game1.dayTimeMoneyBox.drawMoneyBox(b);
                DrawHumbleDonationPrices(b);
                amountText = "";
            }
            else if (_menu.CurrentPageBundle.name == MemeBundleNames.STANLEY)
            {
                DrawGoOutsideAchievement(b);
                amountText = "Go Outside";
            }
            else if (ingredientId == IDProvider.MONEY && memeBundlesThatShouldDisplayMoney.Contains(_menu.CurrentPageBundle.name) )
            {
                Game1.dayTimeMoneyBox.drawMoneyBox(b);
                amountText += "g";
            }
            else if (ingredientId == IDProvider.QI_GEM)
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
            else if (ingredientId == MemeIDProvider.TIME_ELAPSED)
            {
                DrawTimeElapsedCurrency();
                var allowedTime = GetFastBundleAllowedTime(ingredient);
                var minutes = allowedTime / 60000;
                var seconds = (allowedTime % 60000) / 1000;
                var milliseconds = (allowedTime % 60000) % 1000;
                amountText = $"{minutes.ToString("D2")}:{seconds.ToString("D2")}:{milliseconds.ToString("D3")}";
            }
            else if (ingredientId == MemeIDProvider.DEAD_CROP)
            {
                DrawDeadCropsCurrency();
                amountText += " Dead Crops";
            }
            else if (ingredientId == MemeIDProvider.DEAD_PUMPKIN)
            {
                DrawDeadPumpkinsCurrency();
                amountText += " Dead Pumpkins";
            }
            else if (ingredientId == MemeIDProvider.MISSED_FISH)
            {
                DrawUncaughtFishCurrency();
                amountText += " Missed Fish";
            }
            else if (ingredientId == MemeIDProvider.CHILD)
            {
                DrawChildrenCurrency();
                amountText += " Children";
            }
            else if (ingredientId == MemeIDProvider.BANK_MONEY)
            {
                DrawBankCurrency();
                amountText += "$";
            }
            else if (ingredientId == MemeIDProvider.DEATHLINKS)
            {
                DrawDeathLinksCurrency();
                amountText += " DeathLinks";
            }
            else
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
                return "";
            }

            return amountText;
        }

        internal static int GetFastBundleAllowedTime(BundleIngredientDescription ingredient)
        {
            var walkingTime = 42 * 1000; // 42 sec
            var coffeeTime = 39 * 1000; // 39
            var horseTime = 36 * 1000; // 36
            var horseCoffeeTime = 34 * 1000; // 34
            switch (ingredient.stack)
            {
                case 100:
                    return walkingTime * 2;
                case 200:
                    return (int)(Math.Round(walkingTime * 1.4));
                case 600:
                    return (int)(Math.Round(walkingTime * 1.2));
                case 1000:
                    return walkingTime;
                case 1400:
                    return coffeeTime;
                case 1800:
                    return horseTime;
                case 4000:
                    return horseCoffeeTime;
            }

            return walkingTime;
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
            DrawSpecialCurrency(_wallet.Energy, Game1.mouseCursors, new Rectangle(0, 410, 16, 16), 3f);
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
            DrawSpecialCurrency($"{elapsedMinutes.ToString("D2")}:{elapsedSeconds.ToString("D2")}:{elapsedMilliseconds.ToString("D3")}", Game1.mouseCursors, new Rectangle(434, 475, 9, 9));
        }

        private void DrawStepsCurrency()
        {
            DrawSpecialCurrency((int)Game1.stats.StepsTaken, Game1.mouseCursors, new Rectangle(100, 428, 10, 10));
        }

        private void DrawCookiesCurrency(SpriteBatch spriteBatch)
        {
            DrawSpecialCurrency(_wallet.CookieClicker.GetCookies(), Game1.objectSpriteSheet, new Rectangle(112, 144, 16, 16), 3f);

            if (_menu.FromGameMenu)
            {
                return;
            }

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

        private void DrawGachaPrices(SpriteBatch spriteBatch)
        {
            if (_menu.FromGameMenu)
            {
                return;
            }

            var pricesY = 410;
            var centeredX = 936;
            var pricesXOffset = 100;
            var commonPriceX = centeredX - pricesXOffset;
            var rarePriceX = centeredX;
            var legendaryPriceX = centeredX + pricesXOffset;

            var font = Game1.smallFont;
            
            DrawText(spriteBatch, $"{GachaRoller.COMMON_PRICE}g", commonPriceX, pricesY, font);
            DrawText(spriteBatch, $"{GachaRoller.RARE_PRICE}g", rarePriceX, pricesY, font);
            DrawText(spriteBatch, $"{GachaRoller.LEGENDARY_PRICE}g", legendaryPriceX, pricesY, font);
        }

        private void DrawHumbleDonationPrices(SpriteBatch spriteBatch)
        {
            if (_menu.FromGameMenu)
            {
                return;
            }

            var pricesY = 410;
            var centeredX = 936;
            var pricesXOffset = 100;
            var cheapPriceX = centeredX - pricesXOffset;
            var normalPriceX = centeredX;
            var generousPriceX = centeredX + pricesXOffset;

            var font = Game1.smallFont;
            var price = _menu.CurrentPageBundle.Ingredients.First().stack;

            DrawText(spriteBatch, $"Pay what you want!", centeredX, pricesY - 120, font);
            DrawText(spriteBatch, $"{(int)(price * 0.1)}g", cheapPriceX, pricesY, font);
            DrawText(spriteBatch, $"{price}g", normalPriceX, pricesY, font);
            DrawText(spriteBatch, $"{price * 10}g", generousPriceX, pricesY, font);
        }

        private void DrawInvestmentOpportunityLabel(SpriteBatch spriteBatch)
        {
            if (_menu.FromGameMenu)
            {
                return;
            }

            var y = 340;
            var font = Game1.smallFont;
            // Stardew Capital Appreciation Method
            DrawText(spriteBatch, $"Big investment opportunity in the", 936, y, font);
            DrawText(spriteBatch, $"Stardew Capital Archipelago Mutuals!", 936, y + 30, font);
            DrawText(spriteBatch, $"60% to 150% profit by end of month!", 936, y + 60, font);
            // DrawText(spriteBatch, $"by the end of the month!", 936, y + 90, font);
        }

        private void DrawDeadCropsCurrency()
        {
            DrawSpecialCurrency(_wallet.DeadCropsById.Sum(x => x.Value), Game1.objectSpriteSheet, new Rectangle(64, 496, 16, 16), 3f);
        }

        private void DrawDeadPumpkinsCurrency()
        {
            _wallet.DeadCropsById.TryAdd(ObjectIds.PUMPKIN, 0);
            DrawSpecialCurrency(_wallet.DeadCropsById[ObjectIds.PUMPKIN], Game1.objectSpriteSheet, new Rectangle(48, 496, 16, 16), 3f);
        }

        private void DrawUncaughtFishCurrency()
        {
            DrawSpecialCurrency(_wallet.MissedFishById.Sum(x => x.Value), Game1.mouseCursors, new Rectangle(614, 1840, 20, 20), 2f);
        }

        private void DrawChildrenCurrency()
        {
            DrawSpecialCurrency(Game1.player.getChildrenCount(), Game1.mouseCursors, new Rectangle(416, 1962, 16, 16), 3f);
        }

        private void DrawBankCurrency()
        {
            DrawSpecialCurrency(_currentBankString, Game1.mouseCursors, new Rectangle(280, 411, 16, 16), 3f);
        }

        private void DrawDeathLinksCurrency()
        {
            DrawSpecialCurrency(_wallet.DeathLinks, _menu.MemeTexture, new Rectangle(63, 0, 16, 16), 3f);
        }

        private void DrawGoOutsideAchievement(SpriteBatch spriteBatch)
        {
            var textY = 335;
            var centeredX = 936;
            var font = Game1.smallFont;
            DrawText(spriteBatch, $"Do not look at this bundle for 5 years", centeredX, textY, font);
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
            var position = new Vector2(32f, 32f);
            if (sourceRectangle.Width >= 16)
            {
                position.X -= 8;
            }
            if (sourceRectangle.Height >= 16)
            {
                position.Y -= 8;
            }
            spriteBatch.Draw(texture, position, sourceRectangle, Color.White, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
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

            if (ingredient.id == MemeIDProvider.TIME_ELAPSED)
            {
                TryPurchaseCurrentBundleWithTimeElapsed(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.DEAD_CROP)
            {
                TryPurchaseCurrentBundleWithDeadCrops(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.DEAD_PUMPKIN)
            {
                TryPurchaseCurrentBundleWithDeadPumpkins(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.MISSED_FISH)
            {
                TryPurchaseCurrentBundleWithUncaughtFish(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.CHILD)
            {
                TryPurchaseCurrentBundleWithChildren(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.BANK_MONEY)
            {
                TryPurchaseCurrentBundleWithBankMoney(ingredient);
                return;
            }

            if (ingredient.id == MemeIDProvider.DEATHLINKS)
            {
                TryPurchaseCurrentBundleWithDeathLinks(ingredient);
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
            if (ArchipelagoJunimoNoteMenu.DayStopwatch.ElapsedMilliseconds > GetFastBundleAllowedTime(ingredient))
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, ingredient.stack*2, _ => { });
        }

        private void TryPurchaseCurrentBundleWithDeadCrops(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.DeadCropsById.Sum(x => x.Value), payAmount =>
            {
                var keepLooping = true;
                while (payAmount > 0 && keepLooping)
                {
                    keepLooping = false;
                    foreach (var key in _wallet.DeadCropsById.Keys)
                    {
                        if (_wallet.DeadCropsById[key] <= 0)
                        {
                            continue;
                        }

                        _wallet.DeadCropsById[key]--;
                        payAmount--;
                        keepLooping = true;
                        break;
                    }
                }

            });
        }

        private void TryPurchaseCurrentBundleWithDeadPumpkins(BundleIngredientDescription ingredient)
        {
            _wallet.DeadCropsById.TryAdd(ObjectIds.PUMPKIN, 0);
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.DeadCropsById[ObjectIds.PUMPKIN], payAmount => _wallet.DeadCropsById[ObjectIds.PUMPKIN] -= payAmount);
        }

        private void TryPurchaseCurrentBundleWithUncaughtFish(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.MissedFishById.Sum(x => x.Value), payAmount =>
            {
                var keepLooping = true;
                while (payAmount > 0 && keepLooping)
                {
                    keepLooping = false;
                    foreach (var key in _wallet.MissedFishById.Keys)
                    {
                        if (_wallet.MissedFishById[key] <= 0)
                        {
                            continue;
                        }

                        _wallet.MissedFishById[key]--;
                        payAmount--;
                        keepLooping = true;
                        break;
                    }
                }

            });
        }

        private void TryPurchaseCurrentBundleWithChildren(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, Game1.player.getChildrenCount(), payAmount =>
            {
                var remainingPayAmount = payAmount;
                var homeOfFarmer = Utility.getHomeOfFarmer(Game1.player);
                for (var index = homeOfFarmer.characters.Count - 1; index >= 0; --index)
                {
                    if (homeOfFarmer.characters[index] is Child character)
                    {
                        homeOfFarmer.GetChildBed((int)character.Gender)?.mutex.ReleaseLock();
                        if (character.hat.Value != null)
                        {
                            var hat = character.hat.Value;
                            character.hat.Value = (Hat)null;
                            Game1.player.addItemToInventory(hat);
                        }
                        homeOfFarmer.characters.RemoveAt(index);
                        var num = (int)Game1.stats.Increment("childrenTurnedToDoves");
                        remainingPayAmount--;
                        if (remainingPayAmount <= 0)
                        {
                            break;
                        }
                    }
                }
                if (remainingPayAmount <= 0)
                {
                    BroadcastSacrificeSprites();
                }
            });
        }

        private void BroadcastSacrificeSprites()
        {
            Game1.Multiplayer.broadcastSprites(Game1.currentLocation, new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(536, 1945, 8, 8), new Vector2(156f, 388f), false, 0.0f, Color.White)
            {
                interval = 50f,
                totalNumberOfLoops = 99999,
                animationLength = 7,
                layerDepth = 0.0385000035f,
                scale = 4f,
            });
            for (var index = 0; index < 20; ++index)
            {
                Game1.Multiplayer.broadcastSprites(Game1.currentLocation,
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(372, 1956, 10, 10), new Vector2(2f, 6f) * 64f + new Vector2((float)Game1.random.Next(-32, 64), (float)Game1.random.Next(16)),
                        false, 1f / 500f, Color.LightGray)
                    {
                        alpha = 0.75f,
                        motion = new Vector2(1f, -0.5f),
                        acceleration = new Vector2(-1f / 500f, 0.0f),
                        interval = 99999f,
                        layerDepth = (float)(0.03840000182390213 + (double)Game1.random.Next(100) / 10000.0),
                        scale = 3f,
                        scaleChange = 0.01f,
                        rotationChange = (float)((double)Game1.random.Next(-5, 6) * 3.1415927410125732 / 256.0),
                        delayBeforeAnimationStart = index * 25,
                    });
            }
            Game1.currentLocation.playSound("fireball");
            Game1.Multiplayer.broadcastSprites(Game1.currentLocation,
                new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, false, true, 1f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f)
                {
                    motion = new Vector2(4f, -2f),
                });
            if (Game1.player.getChildrenCount() > 1)
            {
                Game1.Multiplayer.broadcastSprites(Game1.currentLocation,
                    new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(388, 1894, 24, 22), 100f, 6, 9999, new Vector2(2f, 5f) * 64f, false, true, 1f, 0.0f, Color.White, 4f, 0.0f, 0.0f, 0.0f)
                    {
                        motion = new Vector2(4f, -1.5f),
                        delayBeforeAnimationStart = 50,
                    });
            }
        }

        private void TryPurchaseCurrentBundleWithBankMoney(BundleIngredientDescription ingredient)
        {
            var currentBank = _bank.GetBankMoneyAmount();
            var currentBankInteger = currentBank > int.MaxValue ? int.MaxValue : (int)currentBank;
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, currentBankInteger, amount => { _bank.RemoveFromBank(amount); });
        }

        private void TryPurchaseCurrentBundleWithDeathLinks(BundleIngredientDescription ingredient)
        {
            TryPurchaseCurrentBundleWithWalletCurrency(ingredient, _wallet.DeathLinks, amount => { _wallet.DeathLinks -= amount; });
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

        public void PurchaseWithCharityDonation(ArchipelagoJunimoNoteMenu menu, double priceRate, double donationRate)
        {
            if (menu.CurrentPageBundle == null || menu.CurrentPageBundle.name != MemeBundleNames.HUMBLE)
            {
                return;
            }

            var normalPrice = menu.CurrentPageBundle.Ingredients.Last().stack;
            var paidPrice = (int)Math.Round(normalPrice * priceRate);
            var donatedPrice = (int)Math.Round(paidPrice * donationRate);
            if (Game1.player.Money < paidPrice)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            Game1.player.Money -= paidPrice;
            _bank.AddToBank(donatedPrice);
            var donationQualifier = "";
            if (priceRate < 0.5)
            {
                donationQualifier = " cheap";
            }
            if (priceRate > 5)
            {
                donationQualifier = " generous";
            }
            Game1.chatBox.addMessage($"Thank you for your{donationQualifier} donation! {donatedPrice}g has been donated to your community", Color.Green);
            menu.PerformCurrencyPurchase();
        }

        public void DonateToBundle(string currencyId)
        {
            if (currencyId == MemeIDProvider.BLOOD)
            {
                const int donationAmount = 25;
                var donation = Math.Max(0, Math.Min(Game1.player.health, donationAmount));
                Game1.player.health -= donationAmount;
                _wallet.Blood += donation;
                if (Game1.player.health <= 0)
                {
                    Game1.activeClickableMenu?.exitThisMenu();
                }
                return;
            }

            if (currencyId == MemeIDProvider.ENERGY)
            {
                var donationAmount = 50;
                if (Game1.player.Stamina + 16 < donationAmount)
                {
                    donationAmount = (int)Math.Ceiling(Game1.player.Stamina + 16);
                }
                var donation = Math.Max(0, donationAmount);
                Game1.player.Stamina -= donationAmount;
                _wallet.Energy += donation;
                if (Game1.player.Stamina <= -16)
                {
                    Game1.activeClickableMenu?.exitThisMenu();
                }
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

            if (_menu.CurrentPageBundle.name == MemeBundleNames.HAIRY)
            {
                Game1.player.changeHairStyle(BALD_HAIR);
                _menu.PerformCurrencyPurchase();
            }
        }

        private string _currentBankString;
        public void OnUpdateTicked(UpdateTickedEventArgs e)
        {
            if (e.IsMultipleOf(60) && ArchipelagoJunimoNoteMenu.IsBundleRemaining(MemeBundleNames.CROWDFUNDING) && _menu?.CurrentPageBundle?.name == MemeBundleNames.CROWDFUNDING)
            {
                var currentBank = _bank.GetBankMoneyAmount();
                _currentBankString = currentBank.ToString();
            }
        }
    }
}
