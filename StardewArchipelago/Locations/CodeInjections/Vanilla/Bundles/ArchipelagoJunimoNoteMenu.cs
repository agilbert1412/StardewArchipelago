#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaitoKid.ArchipelagoUtilities.Net;
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
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using System.Reflection;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ArchipelagoJunimoNoteMenu : JunimoNoteMenuRemake
    {
        private const int REMIXED_BUNDLE_INDEX_THRESHOLD = 100;
        private const int CUSTOM_BUNDLE_INDEX_THRESHOLD = 200;

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoWalletDto _wallet;
        private static LocationChecker _locationChecker;
        private static BundleReader _bundleReader;
        private BundleCurrencyManager _currencyManager;

        public Texture2D MemeTexture;
        private ClickableTextureComponent _donateButton;
        public Dictionary<ClickableTextureComponent, Action> ExtraButtons;

        public ArchipelagoJunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false) : base(fromGameMenu, area, fromThisMenu)
        {
            InitializeFields();
        }

        public ArchipelagoJunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete) : base(whichArea, bundlesComplete)
        {
            InitializeFields();
        }

        public ArchipelagoJunimoNoteMenu(ArchipelagoBundle b, string noteTexturePath) : base(b, noteTexturePath)
        {
            InitializeFields();
        }

        private void InitializeFields()
        {
            _currencyManager = new BundleCurrencyManager(_logger, _modHelper, _wallet, this);
            var memeAssetsPath = Path.Combine("Bundles", "UI", "MemeBundleAssets.png");
            MemeTexture = TexturesLoader.GetTexture(_logger, _modHelper, memeAssetsPath);
            ExtraButtons = new Dictionary<ClickableTextureComponent, Action>();
        }

        public static void InitializeArchipelago(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoWalletDto wallet, LocationChecker locationChecker)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _wallet = wallet;
            _locationChecker = locationChecker;
            _bundleReader = new BundleReader();
        }

        public override void CheckForRewards()
        {
            CheckAllBundleLocations();
            MarkAllRewardsAsAlreadyGrabbed();
        }

        protected override JunimoNoteMenuRemake CreateJunimoNoteMenu()
        {
            if (FromGameMenu || FromThisMenu)
            {
                return new ArchipelagoJunimoNoteMenu(FromGameMenu, WhichArea, FromThisMenu)
                {
                    GameMenuTabToReturnTo = GameMenuTabToReturnTo,
                    MenuToReturnTo = MenuToReturnTo,
                };
            }
            else
            {
                return new ArchipelagoJunimoNoteMenu(WhichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict())
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
            _currencyManager.DrawCurrency(b);
        }

        protected override void DrawButtons(SpriteBatch b)
        {
            base.DrawButtons(b);
            foreach (var (extraButton, _) in ExtraButtons)
            {
                extraButton?.draw(b);
            }
        }

        protected override void ReceiveLeftClickInButtons(int x, int y)
        {
            base.ReceiveLeftClickInButtons(x, y);
            foreach (var (extraButton, actionWhenClicked) in ExtraButtons)
            {
                if (extraButton == null || !extraButton.containsPoint(x, y))
                {
                    continue;
                }

                actionWhenClicked();
            }
        }

        protected override void ReceiveLeftClickPurchaseButton(int x, int y)
        {
            if (this.PurchaseButton == null || !this.PurchaseButton.containsPoint(x, y))
            {
                base.ReceiveLeftClickPurchaseButton(x, y);
                return;
            }

            var ingredient = this.CurrentPageBundle.Ingredients.Last();
            var currency = ingredient.id;
            if (currency == IDProvider.MONEY)
            {
                base.ReceiveLeftClickPurchaseButton(x, y);
                return;
            }

            _currencyManager.TryPurchaseCurrentBundle(ingredient);
        }

        public void PerformCurrencyPurchase()
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

        protected override ArchipelagoBundle CreateBundle(Dictionary<int, bool[]> bundlesComplete, string key, Dictionary<string, string> bundleData, int whichBundle)
        {
            var int32 = Convert.ToInt32(key.Split('/')[1]);
            var bundle = new ArchipelagoBundle(int32, bundleData[key], bundlesComplete[int32], GetBundleLocationFromNumber(whichBundle), NOTE_TEXTURE_NAME, this);
            bundle.myID = whichBundle + REGION_BUNDLE_MODIFIER;
            bundle.rightNeighborID = -7777;
            bundle.leftNeighborID = -7777;
            bundle.upNeighborID = -7777;
            bundle.downNeighborID = -7777;
            bundle.fullyImmutable = true;
            return bundle;
        }

        protected override void SetUpCurrencyButtons()
        {
            base.SetUpCurrencyButtons();
            switch (CurrentPageBundle.name)
            {
                case MemeBundleNames.CLIQUE:

                    // TODO: Clique Button
                    // PurchaseButton.texture = null;
                    break;
                case MemeBundleNames.VAMPIRE:
                case MemeBundleNames.EXHAUSTION:
                case MemeBundleNames.TICK_TOCK:
                    SetUpDonateButton();
                    break;
                case MemeBundleNames.COOKIE_CLICKER:
                    SetUpCookiesButtons();
                    break;
            }
        }

        private void SetUpDonateButton()
        {
            if (FromGameMenu)
            {
                return;
            }

            var textureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 400, 260, 72), MemeTexture, new Rectangle(0, 0, 53, 20), 4f);
            textureComponent.myID = 796;
            textureComponent.leftNeighborID = REGION_BACK_BUTTON;
            textureComponent.rightNeighborID = REGION_PURCHASE_BUTTON;
            _donateButton = textureComponent;
            ExtraButtons.Add(_donateButton, () => _currencyManager.DonateToBundle(CurrentPageBundle.Ingredients.Last().id));
        }

        private void SetUpCookiesButtons()
        {
            if (FromGameMenu)
            {
                return;
            }

            var buttonBackgroundRectangle = new Rectangle(512, 244, 18, 18);
            var xStart = xPositionOnScreen + 800;
            var xPerButton = 100;
            var y = yPositionOnScreen + 350;
            var buttonScale = 4f;
            var grandmaScale = 3f;

            var cookieButtonRect = new Rectangle(xStart, y, 72, 72);
            var cookieBackground = new ClickableTextureComponent(cookieButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cookieTextureRect = new Rectangle(112, 144, 16, 16);
            var cookieRect = GetCenteredTexture(cookieButtonRect, cookieTextureRect, buttonScale, buttonScale);
            var cookieButton = new ClickableTextureComponent(cookieRect, Game1.objectSpriteSheet, cookieTextureRect, buttonScale);
            cookieButton.myID = 793;

            var cursorButtonRect = new Rectangle(xStart + xPerButton, y, 72, 72);
            var cursorBackground = new ClickableTextureComponent(cursorButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cursorTextureRect = new Rectangle(0, 0, 8, 10);
            var cursorRect = GetCenteredTexture(cursorButtonRect, cursorTextureRect, buttonScale, buttonScale);
            var cursorButton = new ClickableTextureComponent(cursorRect, Game1.mouseCursors, cursorTextureRect, buttonScale);
            cursorButton.myID = 794;

            var grandmaButtonRect = new Rectangle(xStart + (xPerButton*2), y, 72, 72);
            var grandmaBackground = new ClickableTextureComponent(grandmaButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var grandmaTextureRect = new Rectangle(0, 168, 16, 24);
            var grandmaRect = GetCenteredTexture(grandmaButtonRect, grandmaTextureRect, buttonScale, grandmaScale);
            var grandmaButton = new ClickableTextureComponent(grandmaRect, Game1.getCharacterFromName("Evelyn").Sprite.Texture, grandmaTextureRect, grandmaScale);
            grandmaButton.myID = 795;

            cookieButton.leftNeighborID = REGION_BACK_BUTTON;
            cookieButton.rightNeighborID = cursorButton.myID;
            cursorButton.leftNeighborID = cookieButton.myID;
            cursorButton.rightNeighborID = grandmaButton.myID;
            grandmaButton.leftNeighborID = cursorButton.myID;
            grandmaButton.rightNeighborID = REGION_PURCHASE_BUTTON;

            ExtraButtons.Add(cookieBackground, () => { });
            ExtraButtons.Add(cursorBackground, () => { });
            ExtraButtons.Add(grandmaBackground, () => { });
            ExtraButtons.Add(cookieButton, _wallet.CookieClicker.ClickCookie);
            ExtraButtons.Add(cursorButton, _wallet.CookieClicker.UpgradeCursor);
            ExtraButtons.Add(grandmaButton, _wallet.CookieClicker.UpgradeGrandma);
        }

        private static Rectangle GetCenteredTexture(Rectangle buttonRect, Rectangle textureRect, float buttonScale, float textureScale)
        {
            var buttonScaledWidth = (int)(buttonRect.Width / buttonScale);
            var buttonScaledHeight = (int)(buttonRect.Height / buttonScale);
            var textureScaledWidth = (int)(textureRect.Width / buttonScale * textureScale);
            var textureScaledHeight = (int)(textureRect.Height / buttonScale * textureScale);
            var widthDifference = buttonScaledWidth - textureScaledWidth;
            var heightDifference = buttonScaledHeight - textureScaledHeight;
            var xOffset = (int)(widthDifference * buttonScale / 2);
            var yOffset = (int)(heightDifference * buttonScale / 2);
            var centeredX = buttonRect.X + xOffset;
            var centeredY = buttonRect.Y + yOffset;
            return new Rectangle(centeredX,
                centeredY,
                buttonRect.Width,
                buttonRect.Height);
        }

        protected override void TakeDownSpecificBundleComponents()
        {
            base.TakeDownSpecificBundleComponents();
            _donateButton = null;
            ExtraButtons.Clear();
        }

        protected override void TryHoverButtons(int x, int y)
        {
            base.TryHoverButtons(x, y);
            foreach (var (extraButton, _) in ExtraButtons)
            {
                extraButton?.tryHover(x, y);
            }
        }
    }
}
