using System;
using System.Collections.Generic;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Constants;
using StardewArchipelago.Extensions;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewValley.Menus;
using Bundle = StardewValley.Menus.Bundle;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Constants;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public static class JunimoNoteMenuInjections
    {
        private const int REMIXED_BUNDLE_INDEX_THRESHOLD = 100;
        private const int CUSTOM_BUNDLE_INDEX_THRESHOLD = 200;

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static LocationChecker _locationChecker;
        private static BundleReader _bundleReader;

        public static void Initialize(LogHandler logger, IModHelper modHelper, ArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundleReader bundleReader)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
            _bundleReader = bundleReader;
        }

        // public void checkForRewards()
        public static void CheckForRewards_SendBundleChecks_PostFix(JunimoNoteMenu __instance)
        {
            try
            {
                CheckAllBundleLocations();
                MarkAllRewardsAsAlreadyGrabbed();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(CheckForRewards_SendBundleChecks_PostFix)}:\n{ex}");
            }
        }

        private static void CheckAllBundleLocations()
        {
            var completedBundleNames = _bundleReader.GetAllCompletedBundles();
            foreach (var completedBundleName in completedBundleNames)
            {
                _locationChecker.AddCheckedLocation(completedBundleName + " Bundle");
            }
        }

        private static void MarkAllRewardsAsAlreadyGrabbed()
        {
            var communityCenter = Game1.locations.OfType<CommunityCenter>().First();
            var bundleRewardsDictionary = communityCenter.bundleRewards;
            foreach (var bundleRewardKey in bundleRewardsDictionary.Keys)
            {
                bundleRewardsDictionary[bundleRewardKey] = false;
            }
        }

        // public void setUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
        public static void SetupMenu_AddTextureOverrides_Postfix(JunimoNoteMenu __instance, int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            try
            {
                var remixedBundlesTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\BundleSprites");
                foreach (var bundle in __instance.bundles)
                {
                    var textureOverride = BundleIcons.GetBundleIcon(_logger, _modHelper, bundle.name, LogLevel.Trace);
                    if (textureOverride == null)
                    {
                        if (bundle.bundleIndex < REMIXED_BUNDLE_INDEX_THRESHOLD)
                        {
                            bundle.bundleTextureOverride = null;
                            bundle.bundleTextureIndexOverride = -1;
                            continue;
                        }

                        if (bundle.bundleIndex < CUSTOM_BUNDLE_INDEX_THRESHOLD)
                        {
                            bundle.bundleTextureOverride = remixedBundlesTexture;
                            bundle.bundleTextureIndexOverride = bundle.bundleIndex - REMIXED_BUNDLE_INDEX_THRESHOLD;
                            continue;
                        }

                        var bundleIndexString = bundle.bundleIndex.ToString();
                        if (bundleIndexString.Length == 4)
                        {
                            if (TryGetBundleName(bundleIndexString, out var moneyBundleName))
                            {
                                var texture = BundleIcons.GetBundleIcon(_logger, _modHelper, moneyBundleName);
                                bundle.bundleTextureOverride = texture;
                                bundle.bundleTextureIndexOverride = 0;
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

                    bundle.bundleTextureOverride = textureOverride;
                    bundle.bundleTextureIndexOverride = 0;
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(SetupMenu_AddTextureOverrides_Postfix)}:\n{ex}");
                return;
            }
        }

        private static bool TryGetBundleName(string bundleIndexString, out string moneyBundleName)
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

        // public string getRewardNameForArea(int whichArea)
        public static bool GetRewardNameForArea_ScoutRoomRewards_Prefix(JunimoNoteMenu __instance, int whichArea, ref string __result)
        {
            try
            {
                string apLocationToScout;
                if (__instance.specificBundlePage)
                {
                    if (!TryGetBundleLocationToScout(__instance, out apLocationToScout))
                    {
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }
                }
                else
                {
                    if (!TryGetRoomLocationToScout(whichArea, out apLocationToScout))
                    {
                        return MethodPrefix.RUN_ORIGINAL_METHOD;
                    }
                }

                var scoutedItem = _archipelago.ScoutSingleLocation(apLocationToScout, true);
                var rewardText = $"Reward: {scoutedItem.PlayerName}'s {scoutedItem.GetItemName(StringExtensions.TurnHeartsIntoStardewHearts)}";
                __result = rewardText;
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(GetRewardNameForArea_ScoutRoomRewards_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static bool TryGetBundleLocationToScout(JunimoNoteMenu junimoNoteMenu, out string apLocationToScout)
        {
            var bundle = junimoNoteMenu.currentPageBundle;
            if (bundle == null)
            {
                apLocationToScout = "";
                return false;
            }

            apLocationToScout = bundle.name + " Bundle";
            return true;
        }

        private static bool TryGetRoomLocationToScout(int whichArea, out string apAreaToScout)
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

        // public override void draw(SpriteBatch b)
        public static void Draw_AddCurrencyBoxes_Postfix(JunimoNoteMenu __instance, SpriteBatch b)
        {
            try
            {
                if (!__instance.specificBundlePage || !Game1.player.hasOrWillReceiveMail("canReadJunimoText"))
                {
                    Game1.specialCurrencyDisplay.ShowCurrency(null);
                    return;
                }

                var ingredient = __instance.currentPageBundle.ingredients.Last();
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
                var textPosition = new Vector2(__instance.xPositionOnScreen + 936 - textSize / 2f, __instance.yPositionOnScreen + 292);
                b.DrawString(Game1.dialogueFont, amountText, textPosition, Game1.textColor * 0.9f);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(Draw_AddCurrencyBoxes_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void DrawStarTokenCurrency()
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

        // public override void receiveLeftClick(int x, int y, bool playSound = true)
        public static bool ReceiveLeftClick_PurchaseCurrencyBundle_Prefix(JunimoNoteMenu __instance, int x, int y, bool playSound)
        {
            try
            {
                if (!JunimoNoteMenu.canClick || __instance.scrambledText || !__instance.specificBundlePage || __instance.purchaseButton == null || !__instance.purchaseButton.containsPoint(x, y))
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                var ingredient = __instance.currentPageBundle.ingredients.Last();
                var currency = ingredient.id;
                if (currency == IDProvider.MONEY)
                {
                    return MethodPrefix.RUN_ORIGINAL_METHOD;
                }

                TryPurchaseCurrentBundle(__instance, ingredient, __instance.currentPageBundle);
                return MethodPrefix.DONT_RUN_ORIGINAL_METHOD;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(ReceiveLeftClick_PurchaseCurrencyBundle_Prefix)}:\n{ex}");
                return MethodPrefix.RUN_ORIGINAL_METHOD;
            }
        }

        private static void TryPurchaseCurrentBundle(JunimoNoteMenu junimoNote, BundleIngredientDescription ingredient, Bundle currentPageBundle)
        {
            if (ingredient.id == IDProvider.QI_GEM)
            {
                TryPurchaseCurrentBundleWithQiGems(junimoNote, ingredient, currentPageBundle);
                return;
            }

            if (ingredient.id == IDProvider.QI_COIN)
            {
                TryPurchaseCurrentBundleWithQiCoins(junimoNote, ingredient, currentPageBundle);
                return;
            }

            if (ingredient.id == IDProvider.STAR_TOKEN)
            {
                TryPurchaseCurrentBundleWithStarTokens(junimoNote, ingredient, currentPageBundle);
                return;
            }
        }

        private static void TryPurchaseCurrentBundleWithQiGems(JunimoNoteMenu junimoNote, BundleIngredientDescription ingredient, Bundle currentPageBundle)
        {
            if (Game1.player.QiGems < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            Game1.player.QiGems -= ingredient.stack;

            PerformCurrencyPurchase(junimoNote, currentPageBundle);
        }

        private static void TryPurchaseCurrentBundleWithQiCoins(JunimoNoteMenu junimoNote, BundleIngredientDescription ingredient, Bundle currentPageBundle)
        {
            if (Game1.player.clubCoins < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            Game1.player.clubCoins -= ingredient.stack;

            PerformCurrencyPurchase(junimoNote, currentPageBundle);
        }

        private static void TryPurchaseCurrentBundleWithStarTokens(JunimoNoteMenu junimoNote, BundleIngredientDescription ingredient, Bundle currentPageBundle)
        {
            if (_state.StoredStarTokens < ingredient.stack)
            {
                Game1.dayTimeMoneyBox.moneyShakeTimer = 600;
                return;
            }

            _state.StoredStarTokens -= ingredient.stack;

            PerformCurrencyPurchase(junimoNote, currentPageBundle);
        }

        private static void PerformCurrencyPurchase(JunimoNoteMenu junimoNote, Bundle currentPageBundle)
        {
            Game1.playSound("select");
            currentPageBundle.completionAnimation(junimoNote);
            if (junimoNote.purchaseButton == null)
            {
            }
            else
            {
                junimoNote.purchaseButton.scale = junimoNote.purchaseButton.baseScale * 0.75f;
            }

            var communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");
            communityCenter.bundleRewards[currentPageBundle.bundleIndex] = true;
            communityCenter.bundles.FieldDict[currentPageBundle.bundleIndex][0] = true;
            junimoNote.checkForRewards();
            var flag = junimoNote.bundles.Any(bundle => !bundle.complete && !bundle.Equals(currentPageBundle));
            var whichArea = junimoNote.whichArea;
            if (!flag)
            {
                // private void restoreAreaOnExit()
                var restoreAreaOnExitMethod = _modHelper.Reflection.GetMethod(junimoNote, "restoreAreaOnExit");
                communityCenter.markAreaAsComplete(whichArea);
                junimoNote.exitFunction = () => restoreAreaOnExitMethod.Invoke();
                communityCenter.areaCompleteReward(whichArea);
            }
            else
            {
                communityCenter.getJunimoForArea(whichArea)?.bringBundleBackToHut(Bundle.getColorFromColorIndex(currentPageBundle.bundleColor),
                    Game1.getLocationFromName("CommunityCenter"));
            }

            // Game1.multiplayer.globalChatInfoMessage("Bundle");
        }
    }
}
