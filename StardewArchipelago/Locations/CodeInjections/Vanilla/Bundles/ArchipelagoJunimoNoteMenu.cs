#nullable disable
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes;
using StardewArchipelago.Stardew;
using StardewValley.Locations;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using Microsoft.Xna.Framework;
using StardewArchipelago.Extensions;
using StardewArchipelago.Constants;
using StardewValley.BellsAndWhistles;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ArchipelagoJunimoNoteMenu : JunimoNoteMenuRemake
    {
        private const int REMIXED_BUNDLE_INDEX_THRESHOLD = 100;
        private const int CUSTOM_BUNDLE_INDEX_THRESHOLD = 200;

        private LogHandler _logger;
        private IModHelper _modHelper;
        private StardewArchipelagoClient _archipelago;
        private ArchipelagoStateDto _state;
        private LocationChecker _locationChecker;
        private BundleReader _bundleReader;

        public ArchipelagoJunimoNoteMenu(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader, bool fromGameMenu, int area = 1, bool fromThisMenu = false) : base(fromGameMenu, area, fromThisMenu)
        {
            InitializeArchipelago(logger, modHelper, archipelago, state, locationChecker, bundleReader);
        }

        public ArchipelagoJunimoNoteMenu(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader, int whichArea, Dictionary<int, bool[]> bundlesComplete) : base(whichArea, bundlesComplete)
        {
            InitializeArchipelago(logger, modHelper, archipelago, state, locationChecker, bundleReader);
        }

        public ArchipelagoJunimoNoteMenu(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader, BundleRemake b, string noteTexturePath) : base(b, noteTexturePath)
        {
            InitializeArchipelago(logger, modHelper, archipelago, state, locationChecker, bundleReader);
        }

        private void InitializeArchipelago(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
            _bundleReader = bundleReader;
        }

        public override void CheckForRewards()
        {
            base.CheckForRewards();
            CheckAllBundleLocations();
            MarkAllRewardsAsAlreadyGrabbed();
        }

        protected override JunimoNoteMenuRemake CreateJunimoNoteMenu()
        {
            if (FromGameMenu || FromThisMenu)
            {
                return new ArchipelagoJunimoNoteMenu(_logger, _modHelper, _archipelago, _state, _locationChecker, _bundleReader, FromGameMenu, WhichArea, FromThisMenu)
                {
                    GameMenuTabToReturnTo = GameMenuTabToReturnTo,
                    MenuToReturnTo = MenuToReturnTo,
                };
            }
            else
            {
                return new ArchipelagoJunimoNoteMenu(_logger, _modHelper, _archipelago, _state, _locationChecker, _bundleReader, WhichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict())
                {
                    GameMenuTabToReturnTo = GameMenuTabToReturnTo,
                    MenuToReturnTo = MenuToReturnTo,
                };
            }
        }

        public override void SetUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            base.SetUpMenu(whichArea, bundlesComplete);
            var remixedBundlesTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\BundleSprites");
            foreach (var bundle in this.Bundles)
            {
                var textureOverride = BundleIcons.GetBundleIcon(_logger, _modHelper, bundle.name, LogLevel.Trace);
                if (textureOverride == null)
                {
                    if (bundle.BundleIndex < REMIXED_BUNDLE_INDEX_THRESHOLD)
                    {
                        bundle.BundleTextureOverride = null;
                        bundle.BundleTextureIndexOverride = -1;
                        continue;
                    }

                    if (bundle.BundleIndex < CUSTOM_BUNDLE_INDEX_THRESHOLD)
                    {
                        bundle.BundleTextureOverride = remixedBundlesTexture;
                        bundle.BundleTextureIndexOverride = bundle.BundleIndex - REMIXED_BUNDLE_INDEX_THRESHOLD;
                        continue;
                    }

                    var bundleIndexString = bundle.BundleIndex.ToString();
                    if (bundleIndexString.Length == 4)
                    {
                        if (TryGetBundleName(bundleIndexString, out var moneyBundleName))
                        {
                            var texture = BundleIcons.GetBundleIcon(_logger, _modHelper, moneyBundleName);
                            bundle.BundleTextureOverride = texture;
                            bundle.BundleTextureIndexOverride = 0;
                            if (texture == null)
                            {
                                _logger.LogWarning($"Could not find a proper icon for money bundle '{moneyBundleName}', using default Archipelago Icon");
                            }
                            continue;
                        }
                    }

                    _logger.LogWarning($"Could not find a proper icon for bundle '{bundle.name}', using default Archipelago Icon");
                    textureOverride = ArchipelagoTextures.GetArchipelagoLogo(_logger, _modHelper, 32, ArchipelagoTextures.COLOR);
                }

                bundle.BundleTextureOverride = textureOverride;
                bundle.BundleTextureIndexOverride = 0;
            }
        }

        public override string GetRewardNameForArea(int whichArea)
        {
            string apLocationToScout;
            if (SpecificBundlePage)
            {
                if (!TryGetBundleLocationToScout(out apLocationToScout))
                {
                    return base.GetRewardNameForArea(whichArea);
                }
            }
            else
            {
                if (!TryGetRoomLocationToScout(whichArea, out apLocationToScout))
                {
                    return base.GetRewardNameForArea(whichArea);
                }
            }

            var scoutedItem = _archipelago.ScoutStardewLocation(apLocationToScout, true);
            var playerName = "Unknown Player";
            var itemName = "Unknown Item";
            if (scoutedItem != null)
            {
                itemName = scoutedItem.GetItemName(StringExtensions.TurnHeartsIntoStardewHearts);
                playerName = scoutedItem.PlayerName;
            }
            var rewardText = $"Reward: {playerName}'s {itemName}";
            return rewardText;
        }

        public override void draw(SpriteBatch b)
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

        private void CheckAllBundleLocations()
        {
            var completedBundleNames = _bundleReader.GetAllCompletedBundles();
            foreach (var completedBundleName in completedBundleNames)
            {
                _locationChecker.AddCheckedLocation(completedBundleName + " Bundle");
            }
        }

        private void MarkAllRewardsAsAlreadyGrabbed()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            var bundleRewardsDictionary = communityCenter.bundleRewards;
            foreach (var bundleRewardKey in bundleRewardsDictionary.Keys)
            {
                bundleRewardsDictionary[bundleRewardKey] = false;
            }
        }

        private bool TryGetBundleName(string bundleIndexString, out string moneyBundleName)
        {
            switch (bundleIndexString[..2])
            {
                case "23":
                    moneyBundleName = "money_cheap";
                    return true;
                case "24":
                    moneyBundleName = "money_medium";
                    return true;
                case "25":
                    moneyBundleName = "money_expensive";
                    return true;
                case "26":
                    moneyBundleName = "money_rich";
                    return true;
                default:
                    moneyBundleName = "";
                    return false;
            }
        }

        private bool TryGetBundleLocationToScout(out string apLocationToScout)
        {
            var bundle = CurrentPageBundle;
            if (bundle == null)
            {
                apLocationToScout = "";
                return false;
            }

            if (bundle.name.StartsWith("Raccoon Request "))
            {
                apLocationToScout = bundle.name;
                return true;
            }

            apLocationToScout = bundle.name + " Bundle";
            return true;
        }

        private bool TryGetRoomLocationToScout(int whichArea, out string apAreaToScout)
        {
            apAreaToScout = "???";
            switch ((Area)whichArea)
            {
                case Area.Pantry:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_PANTRY;
                    break;
                case Area.CraftsRoom:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_CRAFTS_ROOM;
                    break;
                case Area.FishTank:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_FISH_TANK;
                    break;
                case Area.BoilerRoom:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_BOILER_ROOM;
                    break;
                case Area.Vault:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_VAULT;
                    break;
                case Area.Bulletin:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_BULLETIN_BOARD;
                    break;
                case Area.AbandonedJojaMart:
                    apAreaToScout = CommunityCenterInjections.AP_LOCATION_ABANDONED_JOJA_MART;
                    break;
                default:
                    apAreaToScout = "???";
                    return false;
            }
            return true;
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
