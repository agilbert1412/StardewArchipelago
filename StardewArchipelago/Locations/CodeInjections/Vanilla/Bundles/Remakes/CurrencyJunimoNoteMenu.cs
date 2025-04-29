using StardewValley.Menus;
using StardewValley;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley.Locations;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewValley.BellsAndWhistles;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes
{
    public class CurrencyJunimoNoteMenu : ArchipelagoJunimoNoteMenu
    {
        public ClickableTextureComponent PurchaseButton;

        public CurrencyJunimoNoteMenu(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader, bool fromGameMenu, int area = 1, bool fromThisMenu = false) : base(logger, modHelper, archipelago, state, locationChecker, bundleReader, fromGameMenu, area, fromThisMenu)
        {
        }

        public CurrencyJunimoNoteMenu(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader, int whichArea, Dictionary<int, bool[]> bundlesComplete) : base(logger, modHelper, archipelago, state, locationChecker, bundleReader, whichArea, bundlesComplete)
        {
        }

        public CurrencyJunimoNoteMenu(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader, BundleRemake bundle, string noteTexturePath) : base(logger, modHelper, archipelago, state, locationChecker, bundleReader, bundle, noteTexturePath)
        {
        }

        protected override void SetUpBundleSpecificPage(BundleRemake b)
        {
            base.SetUpBundleSpecificPage(b);
            SetUpPurchaseButton();
        }

        private void SetUpPurchaseButton()
        {
            if (FromGameMenu)
            {
                return;
            }
            var textureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 504, 260, 72), NoteTexture, new Rectangle(517, 286, 65, 20), 4f);
            textureComponent.myID = 797;
            textureComponent.leftNeighborID = REGION_BACK_BUTTON;
            PurchaseButton = textureComponent;
            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            currentlySnappedComponent = PurchaseButton;
            snapCursorToCurrentSnappedComponent();
        }

        public override void TakeDownBundleSpecificPage()
        {
            base.TakeDownBundleSpecificPage();
            PurchaseButton = null;
        }

        protected override bool TryReceiveLeftClickInBundleArea(int x, int y)
        {
            if (PurchaseButton != null && PurchaseButton.containsPoint(x, y))
            {
                var stack = CurrentPageBundle.Ingredients.Last().stack;
                if (Game1.player.Money >= stack)
                {
                    Game1.player.Money -= stack;
                    Game1.playSound("select");
                    CurrentPageBundle.CompletionAnimation(this);
                    if (PurchaseButton != null)
                    {
                        PurchaseButton.scale = PurchaseButton.baseScale * 0.75f;
                    }
                    var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");
                    communityCenter.bundleRewards[CurrentPageBundle.BundleIndex] = true;
                    communityCenter.bundles.FieldDict[CurrentPageBundle.BundleIndex][0] = true;
                    CheckForRewards();
                    var flag = false;
                    foreach (var bundle in Bundles)
                    {
                        if (!bundle.Complete && !bundle.Equals(CurrentPageBundle))
                        {
                            flag = true;
                            break;
                        }
                    }
                    if (!flag)
                    {
                        communityCenter.markAreaAsComplete(WhichArea);
                        exitFunction = restoreAreaOnExit;
                        communityCenter.areaCompleteReward(WhichArea);
                    }
                    else
                    {
                        communityCenter.getJunimoForArea(WhichArea)?.bringBundleBackToHut(BundleRemake.GetColorFromColorIndex(CurrentPageBundle.BundleColor), Game1.RequireLocation("CommunityCenter"));
                    }
                    Game1.Multiplayer.globalChatInfoMessage("Bundle");
                }
                else
                {
                    Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                }
            }

            return false;
        }

        protected override void PerformHoverActionInSpecificBundlePage(int x, int y)
        {
            base.PerformHoverActionInSpecificBundlePage(x, y);
            PurchaseButton?.tryHover(x, y);
        }

        protected virtual void DrawBundleRequirements(SpriteBatch b)
        {
            base.DrawBundleRequirements(b);
            DrawCurrencyBoxes(b);
            DrawPurchaseButton(b);
        }

        private void DrawPurchaseButton(SpriteBatch b)
        {
            if (PurchaseButton != null)
            {
                PurchaseButton.draw(b);
                Game1.dayTimeMoneyBox.drawMoneyBox(b);
            }
        }

        public void DrawCurrencyBoxes(SpriteBatch b)
        {
            base.draw(b);

            if (!this.SpecificBundlePage || !Game1.player.hasOrWillReceiveMail("canReadJunimoText"))
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
                return;
            }

            var ingredient = this.CurrentPageBundle.Ingredients.Last();
            var ingredientId = ingredient.id;
            if (ingredientId == IDProvider.MONEY)
            {
                return;
            }

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
            else
            {
                Game1.specialCurrencyDisplay.ShowCurrency(null);
                return;
            }

            var textSize = Game1.dialogueFont.MeasureString(amountText).X;
            var textPosition = new Vector2(this.xPositionOnScreen + 936 - textSize / 2f, this.yPositionOnScreen + 292);
            b.DrawString(Game1.dialogueFont, amountText, textPosition, Game1.textColor * 0.9f);
        }

        private void DrawStarTokenCurrency()
        {
            var spriteBatch = Game1.spriteBatch;
            spriteBatch.End();
            Game1.PushUIMode();
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            var tokenAmount = _state.StoredStarTokens;
            spriteBatch.Draw(Game1.fadeToBlackRect, new Rectangle(16, 16, 128 + (tokenAmount > 999 ? 16 : 0), 64), Color.Black * 0.75f);
            spriteBatch.Draw(Game1.mouseCursors, new Vector2(32f, 32f), new Rectangle(338, 400, 8, 8), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            Game1.drawWithBorder(tokenAmount.ToString() ?? "", Color.Black, Color.White, new Vector2(72f, (float)(21 + (LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en ? 8 : (LocalizedContentManager.CurrentLanguageLatin ? 16 : 8)))), 0.0f, 1f, 1f, false);
            //if (Game1.activeClickableMenu == null)
            //{
            // Game1.dayTimeMoneyBox.drawMoneyBox(spriteBatch, Game1.dayTimeMoneyBox.xPositionOnScreen, 4);
            //}
            spriteBatch.End();
            Game1.PopUIMode();
            spriteBatch.Begin(blendState: BlendState.AlphaBlend, samplerState: SamplerState.PointClamp);
            //if (Game1.IsMultiplayer)
            //{
            //    Game1.player.team.festivalScoreStatus.Draw(spriteBatch, new Vector2(32f, (float)(Game1.viewport.Height - 32)), draw_layer: 0.99f, vertical_origin: PlayerStatusList.VerticalAlignment.Bottom);
            //}
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (!JunimoNoteMenu.canClick || this.ScrambledText || !this.SpecificBundlePage || this.PurchaseButton == null || !this.PurchaseButton.containsPoint(x, y))
            {
                base.receiveLeftClick(x, y, playSound);
                return;
            }

            var ingredient = this.CurrentPageBundle.Ingredients.Last();
            var currency = ingredient.id;
            if (currency == IDProvider.MONEY)
            {
                base.receiveLeftClick(x, y, playSound);
                return;
            }

            TryPurchaseCurrentBundle(ingredient);
        }

        private void TryPurchaseCurrentBundle(BundleIngredientDescription ingredient)
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
        }

        private void TryPurchaseCurrentBundleWithQiGems(BundleIngredientDescription ingredient)
        {
            if (Game1.player.QiGems < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            Game1.player.QiGems -= ingredient.stack;

            PerformCurrencyPurchase();
        }

        private void TryPurchaseCurrentBundleWithQiCoins(BundleIngredientDescription ingredient)
        {
            if (Game1.player.clubCoins < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            Game1.player.clubCoins -= ingredient.stack;

            PerformCurrencyPurchase();
        }

        private void TryPurchaseCurrentBundleWithStarTokens(BundleIngredientDescription ingredient)
        {
            if (_state.StoredStarTokens < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            _state.StoredStarTokens -= ingredient.stack;

            PerformCurrencyPurchase();
        }

        private void PerformCurrencyPurchase()
        {
            Game1.playSound("select");
            CurrentPageBundle.CompletionAnimation(this);
            if (this.PurchaseButton == null)
            {
            }
            else
            {
                this.PurchaseButton.scale = this.PurchaseButton.baseScale * 0.75f;
            }

            var communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");
            communityCenter.bundleRewards[CurrentPageBundle.BundleIndex] = true;
            communityCenter.bundles.FieldDict[CurrentPageBundle.BundleIndex][0] = true;
            this.CheckForRewards();
            var flag = this.Bundles.Any(bundle => !bundle.Complete && !bundle.Equals(CurrentPageBundle));
            var whichArea = this.WhichArea;
            if (!flag)
            {
                communityCenter.markAreaAsComplete(whichArea);
                this.exitFunction = () => this.restoreAreaOnExit();
                communityCenter.areaCompleteReward(whichArea);
            }
            else
            {
                communityCenter.getJunimoForArea(whichArea)?.bringBundleBackToHut(BundleRemake.GetColorFromColorIndex(CurrentPageBundle.BundleColor),
                    Game1.getLocationFromName("CommunityCenter"));
            }

            // Game1.multiplayer.globalChatInfoMessage("Bundle");
        }
    }
}
