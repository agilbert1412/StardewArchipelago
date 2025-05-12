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
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Items.Traps;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Minigames;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Bundle = StardewArchipelago.Bundles.Bundle;

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
        private static TrapManager _trapManager;
        private BundleCurrencyManager _currencyManager;

        internal static bool SisyphusStoneFell = false;
        internal static int SisyphusIndex = -1;
        internal static int BureaucracyIndex = -1;
        internal ClothesMenu _clothesMenu;
        internal int _bundleBundleIndex = -1;
        internal const int NUMBER_SUB_BUNDLES = 4;
        internal int ingredientsPerSubBundle => IngredientList.Count / NUMBER_SUB_BUNDLES;

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
            MemeTexture = TexturesLoader.GetTexture(memeAssetsPath);
            ExtraButtons = new Dictionary<ClickableTextureComponent, Action>();
            InitializeClothesMenu();
        }

        private void InitializeClothesMenu()
        {
            var capacity = Game1.player.maxItems.Value;
            var rows = 6;
            var width = 64 * ((capacity == -1 ? 36 : capacity) / rows);
            var height = 64 * rows + 16;
            _clothesMenu = new ClothesMenu(xPositionOnScreen + 128, yPositionOnScreen + 140, width, height);
        }

        public static void InitializeArchipelago(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoWalletDto wallet, LocationChecker locationChecker, TrapManager trapManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _wallet = wallet;
            _locationChecker = locationChecker;
            _trapManager = trapManager;
            _bundleReader = new BundleReader();
        }

        public override void CheckForRewards()
        {
            CheckAllBundleLocations();
            MarkAllRewardsAsAlreadyGrabbed();
        }

        protected override ArchipelagoJunimoNoteMenu CreateJunimoNoteMenu()
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
                AssignBundleIcon(bundle, remixedBundlesTexture);
            }
        }

        private void AssignBundleIcon(ArchipelagoBundle bundle, Texture2D remixedBundlesTexture)
        {
            var textureOverride = BundleIcons.GetBundleIcon(_logger, _modHelper, bundle.name, LogLevel.Trace);
            if (textureOverride != null)
            {
                bundle.BundleTextureOverride = textureOverride;
                bundle.BundleTextureIndexOverride = 0;
                return;
            }

            if (TryAssignSpecialMemeIcon(bundle))
            {
                return;
            }

            if (TryAssignVanillaRemixedIcon(bundle))
            {
                return;
            }

            if (TryAssignIconFromVanillaTextures(bundle, remixedBundlesTexture))
            {
                return;
            }

            if (TryAssignMoneyBundleIcon(bundle))
            {
                return;
            }

            AssignDefaultIcon(bundle);
        }

        private static bool TryAssignSpecialMemeIcon(ArchipelagoBundle bundle)
        {
            if (bundle.name == MemeBundleNames.TRAP)
            {
                var textureOverride = ArchipelagoTextures.GetArchipelagoLogo(32, ArchipelagoTextures.RED);
                bundle.BundleTextureOverride = textureOverride;
                bundle.BundleTextureIndexOverride = 0;
                return true;
            }
            return false;
        }

        private static bool TryAssignVanillaRemixedIcon(ArchipelagoBundle bundle)
        {
            if (bundle.BundleIndex < REMIXED_BUNDLE_INDEX_THRESHOLD)
            {
                bundle.BundleTextureOverride = null;
                bundle.BundleTextureIndexOverride = -1;
                return true;
            }
            return false;
        }

        private static bool TryAssignIconFromVanillaTextures(ArchipelagoBundle bundle, Texture2D remixedBundlesTexture)
        {
            if (bundle.BundleIndex < CUSTOM_BUNDLE_INDEX_THRESHOLD)
            {
                bundle.BundleTextureOverride = remixedBundlesTexture;
                bundle.BundleTextureIndexOverride = bundle.BundleIndex - REMIXED_BUNDLE_INDEX_THRESHOLD;
                return true;
            }
            return false;
        }

        private bool TryAssignMoneyBundleIcon(ArchipelagoBundle bundle)
        {
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
                    return true;
                }
            }
            return false;
        }

        private static void AssignDefaultIcon(ArchipelagoBundle bundle)
        {
            _logger.LogWarning($"Could not find a proper icon for bundle '{bundle.name}', using default Archipelago Icon");
            var textureOverride = ArchipelagoTextures.GetArchipelagoLogo(32, ArchipelagoTextures.COLOR);
            bundle.BundleTextureOverride = textureOverride;
            bundle.BundleTextureIndexOverride = 0;
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

            if (_locationChecker.IsLocationChecked(apLocationToScout))
            {
                return $"No Reward Remaining";
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
                if (_locationChecker.IsLocationMissing(completedBundleName + " Bundle"))
                {
                    _locationChecker.AddCheckedLocation(completedBundleName + " Bundle");
                }
                else if (_locationChecker.IsLocationMissing(completedBundleName))
                {
                    _locationChecker.AddCheckedLocation(completedBundleName);
                }
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
            if (_locationChecker.LocationExists(apLocationToScout))
            {
                return true;
            }

            apLocationToScout = bundle.name;
            return _locationChecker.LocationExists(apLocationToScout);
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

        protected override void SetUpPurchaseButton()
        {
            if (CurrentPageBundle.name == MemeBundleNames.NFT)
            {
                return;
            }

            base.SetUpPurchaseButton();
        }

        private void SetUpDonateButton()
        {
            if (FromGameMenu)
            {
                return;
            }

            var textureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 825, yPositionOnScreen + 400, 260, 72), MemeTexture, new Rectangle(0, 0, 53, 20), 4f);
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
            var y = yPositionOnScreen + 335;
            var buttonScale = 4f;
            var grandmaScale = 3f;

            var cursorButtonRect = new Rectangle(xStart, y, 72, 72);
            var cursorBackground = new ClickableTextureComponent(cursorButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cursorTextureRect = new Rectangle(0, 0, 8, 10);
            var cursorRect = GetCenteredTexture(cursorButtonRect, cursorTextureRect, buttonScale, buttonScale);
            var cursorButton = new ClickableTextureComponent(cursorRect, Game1.mouseCursors, cursorTextureRect, buttonScale);
            cursorButton.myID = 793;

            var cookieButtonRect = new Rectangle(xStart + xPerButton, y, 72, 72);
            var cookieBackground = new ClickableTextureComponent(cookieButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cookieTextureRect = new Rectangle(112, 144, 16, 16);
            var cookieRect = GetCenteredTexture(cookieButtonRect, cookieTextureRect, buttonScale, buttonScale);
            var cookieButton = new ClickableTextureComponent(cookieRect, Game1.objectSpriteSheet, cookieTextureRect, buttonScale);
            cookieButton.myID = 794;

            var grandmaButtonRect = new Rectangle(xStart + (xPerButton*2), y, 72, 72);
            var grandmaBackground = new ClickableTextureComponent(grandmaButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var grandmaTextureRect = new Rectangle(0, 168, 16, 24);
            var grandmaRect = GetCenteredTexture(grandmaButtonRect, grandmaTextureRect, buttonScale, grandmaScale);
            var grandmaButton = new ClickableTextureComponent(grandmaRect, Game1.getCharacterFromName("Evelyn").Sprite.Texture, grandmaTextureRect, grandmaScale);
            grandmaButton.myID = 795;

            cursorButton.leftNeighborID = REGION_BACK_BUTTON;
            cursorButton.rightNeighborID = cookieButton.myID;
            cookieButton.leftNeighborID = cursorButton.myID;
            cookieButton.rightNeighborID = grandmaButton.myID;
            grandmaButton.leftNeighborID = cookieButton.myID;
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
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                if (_bundleBundleIndex != -1)
                {
                    _bundleBundleIndex = -1;
                    return;
                }
            }

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

        protected override void UpdateIngredientSlots()
        {
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                UpdateIngredientSlotsBundleBundle();
                AfterUpdateIngredientSlotsBundleBundle();
                return;
            }
            base.UpdateIngredientSlots();
            AfterUpdateIngredientSlotsSpecialBundles();
        }

        private void AfterUpdateIngredientSlotsSpecialBundles()
        {
            AfterUpdateIngredientSlotsSisyphus();
            AfterUpdateIngredientSlotsBureaucracy();
            AfterUpdateIngredientSlotsSchrodinger();
        }

        private void AfterUpdateIngredientSlotsSisyphus()
        {
            if (CurrentPageBundle?.name != MemeBundleNames.SISYPHUS)
            {
                return;
            }
            if (SisyphusIndex >= 0 && !SisyphusStoneFell)
            {
                FocusOnOneIngredientSlot(SisyphusIndex);
                return;
            }
            for (var slotIndex = 0; slotIndex < IngredientSlots.Count; ++slotIndex)
            {
                if (IngredientSlots[slotIndex].item != null)
                {
                    continue;
                }

                FocusOnOneIngredientSlot(slotIndex);
                SisyphusIndex = slotIndex;
                SisyphusStoneFell = false;
                return;
            }
        }

        private void AfterUpdateIngredientSlotsBureaucracy()
        {
            if (CurrentPageBundle?.name != MemeBundleNames.BUREAUCRACY)
            {
                return;
            }

            for (var slotIndex = 0; slotIndex < CurrentPageBundle.Ingredients.Count; ++slotIndex)
            {
                var ingredient = CurrentPageBundle.Ingredients[slotIndex];
                if (Game1.player.Items.ContainsId(ingredient.id))
                {
                    continue;
                }

                FocusOnOneIngredientSlot(slotIndex);
                BureaucracyIndex = slotIndex;
                return;
            }

            FocusOnOneIngredientSlot(0);
            BureaucracyIndex = 0;
        }

        private void AfterUpdateIngredientSlotsBundleBundle()
        {
            if (CurrentPageBundle?.name != MemeBundleNames.BUNDLE_BUNDLE)
            {
                return;
            }

            var ingredientRectangles = GenerateIngredientRectangles(ingredientsPerSubBundle);
            var ingredientSlotRectangles = GenerateIngredientSlotsRectangles(ingredientsPerSubBundle);

            for (var indexBundle = 0; indexBundle < NUMBER_SUB_BUNDLES; indexBundle++)
            {
                for (var indexIngredient = 0; indexIngredient < ingredientsPerSubBundle; indexIngredient++)
                {
                    var index = indexBundle * ingredientsPerSubBundle + indexIngredient;
                    IngredientList[index].bounds = ingredientRectangles[indexIngredient];
                    IngredientSlots[index].bounds = ingredientSlotRectangles[indexIngredient];
                }
            }

            var SubBundleRectangles = GenerateIngredientSlotsRectangles(NUMBER_SUB_BUNDLES);
            for (var indexSubBundle = 0; indexSubBundle < NUMBER_SUB_BUNDLES; indexSubBundle++)
            {
                var sourceRect = new Rectangle(512, 244, 18, 18);
                IngredientSlots.Add(new ClickableTextureComponent(SubBundleRectangles[indexSubBundle], NoteTexture, sourceRect, 4f));
            }
        }

        private void AfterUpdateIngredientSlotsSchrodinger()
        {
            if (CurrentPageBundle?.name != MemeBundleNames.SCHRODINGER)
            {
                return;
            }
            
            var index = GetValidSchrodingerIndex();
            for (var i = GetIngredientsEndIndex() - 1; i >= 0; i--)
            {
                if (i == index)
                {
                    continue;
                }

                IngredientList.RemoveAt(i);
                CurrentPageBundle.Ingredients.RemoveAt(i);
            }
        }

        private void FocusOnOneIngredientSlot(int focusedSlotIndex)
        {
            for (var slotIndex = 0; slotIndex < IngredientSlots.Count; ++slotIndex)
            {
                var toAddTo1 = new List<Rectangle>();
                if (slotIndex == focusedSlotIndex)
                {
                    AddRectangleRowsToList(toAddTo1, 1, 932, 540);
                }
                else
                {
                    AddRectangleRowsToList(toAddTo1, 1, 932 * 100, 540 * 100);
                }
                foreach (var tempSprite in TempSprites)
                {
                    if (tempSprite.Position.X == IngredientSlots[slotIndex].bounds.X && tempSprite.Position.Y == IngredientSlots[slotIndex].bounds.Y)
                    {
                        tempSprite.Position = new Vector2(toAddTo1[0].X, toAddTo1[0].Y);
                    }
                }
                IngredientSlots[slotIndex].bounds = toAddTo1[0];
            }
            for (var slotIndex = 0; slotIndex < IngredientList.Count; ++slotIndex)
            {
                var toAddTo1 = new List<Rectangle>();
                if (slotIndex == focusedSlotIndex)
                {
                    AddRectangleRowsToList(toAddTo1, 1, 932, 364);
                }
                else
                {
                    AddRectangleRowsToList(toAddTo1, 1, 932 * 100, 364 * 100);
                }
                foreach (var tempSprite in TempSprites)
                {
                    if (tempSprite.Position.X == IngredientList[slotIndex].bounds.X && tempSprite.Position.Y == IngredientList[slotIndex].bounds.Y)
                    {
                        tempSprite.Position = new Vector2(toAddTo1[0].X, toAddTo1[0].Y);
                    }
                }
                IngredientList[slotIndex].bounds = toAddTo1[0];
            }
        }

        protected override int UpdateIngredientSlot(BundleIngredientDescription ingredient, int index)
        {
            if (CurrentPageBundle.name == MemeBundleNames.REVERSE)
            {
                return UpdateReverseIngredientSlot(ingredient, index);
            }
            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                return UpdateTrapIngredientSlot(ingredient, index);
            }

            return base.UpdateIngredientSlot(ingredient, index);
        }

        private int UpdateReverseIngredientSlot(BundleIngredientDescription ingredient, int index)
        {
            if (ingredient.completed && index < IngredientSlots.Count)
            {
                return index;
            }

            var representativeItemId = GetRepresentativeItemId(ingredient);
            if (ingredient.preservesId != null)
            {
                IngredientSlots[index].item = Utility.CreateFlavoredItem(representativeItemId, ingredient.preservesId, ingredient.quality, ingredient.stack);
            }
            else
            {
                IngredientSlots[index].item = ItemRegistry.Create(representativeItemId, ingredient.stack, ingredient.quality);
            }
            // CurrentPageBundle.IngredientDepositAnimation(IngredientSlots[index], NOTE_TEXTURE_NAME, true);
            ++index;
            return index;
        }

        private int UpdateTrapIngredientSlot(BundleIngredientDescription ingredient, int index)
        {
            if (ingredient.completed && index < IngredientSlots.Count)
            {
                return index;
            }

            IngredientSlots[index].item = ItemRegistry.Create(MemeIDProvider.FUN_TRAP, ingredient.stack, ingredient.quality);
            ++index;
            return index;
        }

        protected override bool ReceiveLeftClickInSpecificBundlePage(int x, int y)
        {
            if (CurrentPageBundle.name == MemeBundleNames.REVERSE)
            {
                if (ReceiveLeftClickInReverseBundle(x, y))
                {
                    return true;
                }
            }
            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                if (ReceiveLeftClickInTrapBundle(x, y))
                {
                    return true;
                }
            }
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                if (ReceiveLeftClickInBundleBundle(x, y))
                {
                    return true;
                }
            }

            return base.ReceiveLeftClickInSpecificBundlePage(x, y);
        }

        private bool ReceiveLeftClickInReverseBundle(int x, int y)
        {
            if (HeldItem != null || !CurrentPageBundle.DepositsAllowed)
            {
                return false;
            }

            for (var index = 0; index < IngredientSlots.Count; ++index)
            {
                if (!IngredientSlots[index].containsPoint(x, y))
                {
                    continue;
                }

                if (IngredientSlots[index].item == null)
                {
                    continue;
                }

                HeldItem = IngredientSlots[index].item;
                IngredientSlots[index].item = null;
                var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");

                for (var ingredientIndex = 0; ingredientIndex < CurrentPageBundle.Ingredients.Count; ++ingredientIndex)
                {
                    var ingredientDescription = CurrentPageBundle.Ingredients[ingredientIndex];
                    if (CurrentPageBundle.IsValidItemForThisIngredientDescription(HeldItem, ingredientDescription, ingredientIndex, this))
                    {
                        var completedDescription = new BundleIngredientDescription(ingredientDescription, true);
                        CurrentPageBundle.Ingredients[ingredientIndex] = completedDescription;
                        communityCenter.bundles.FieldDict[CurrentPageBundle.BundleIndex][ingredientIndex] = true;
                        if (OnIngredientDeposit != null)
                        {
                            OnIngredientDeposit(ingredientIndex);
                            break;
                        }
                        break;
                    }
                }

                CheckIfBundleIsComplete();
            }

            return false;
        }

        private bool ReceiveLeftClickInTrapBundle(int x, int y)
        {
            if (HeldItem != null || !CurrentPageBundle.DepositsAllowed)
            {
                return false;
            }

            for (var index = 0; index < IngredientSlots.Count; ++index)
            {
                if (!IngredientSlots[index].containsPoint(x, y))
                {
                    continue;
                }

                if (IngredientSlots[index].item == null)
                {
                    continue;
                }

                var item = IngredientSlots[index].item;
                IngredientSlots[index].item = null;
                var communityCenter = Game1.RequireLocation<CommunityCenter>("CommunityCenter");

                for (var ingredientIndex = 0; ingredientIndex < CurrentPageBundle.Ingredients.Count; ++ingredientIndex)
                {
                    var ingredientDescription = CurrentPageBundle.Ingredients[ingredientIndex];
                    if (CurrentPageBundle.IsValidItemForThisIngredientDescription(item, ingredientDescription, ingredientIndex, this))
                    {
                        var completedDescription = new BundleIngredientDescription(ingredientDescription, true);
                        CurrentPageBundle.Ingredients[ingredientIndex] = completedDescription;
                        communityCenter.bundles.FieldDict[CurrentPageBundle.BundleIndex][ingredientIndex] = true;
                        ExecuteRandomTrap(ingredientIndex);
                        if (OnIngredientDeposit != null)
                        {
                            OnIngredientDeposit(ingredientIndex);
                        }
                        break;
                    }
                }

                CheckIfBundleIsComplete();
            }

            return false;
        }

        private bool ReceiveLeftClickInBundleBundle(int x, int y)
        {
            if (_bundleBundleIndex == -1)
            {
                var startIndex = NUMBER_SUB_BUNDLES * ingredientsPerSubBundle;
                var endIndex = startIndex + NUMBER_SUB_BUNDLES;
                for (var i = startIndex; i < endIndex; i++)
                {
                    if (IngredientSlots[i].containsPoint(x, y))
                    {
                        _bundleBundleIndex = i - startIndex;
                        return true;
                    }
                }
            }

            return false;
        }

        private void ExecuteRandomTrap(int ingredientIndex)
        {
            var chosenTrap = _trapManager.ExecuteRandomTrapImmediately(ingredientIndex);
            var message = _archipelago.SendFakeItemMessage(chosenTrap, $"Trap Bundle Item {ingredientIndex}");
            Game1.chatBox?.addMessage(message, Color.Gold);
        }

        protected override bool CheckIfAllIngredientsAreDeposited()
        {
            if (CurrentPageBundle.name == MemeBundleNames.REVERSE || CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                return CheckIfAllIngredientsAreTakenOut();
            }
            if (CurrentPageBundle.name == MemeBundleNames.BUREAUCRACY)
            {
                var isComplete = CheckIfAnyIngredientsIsDeposited();
                if (isComplete)
                {
                    DoPermitA39EasterEgg();
                }
                return isComplete;
            }
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                return CheckIfAllIngredientsIsDepositedExcludingTheBundlesThemselves();
            }

            return base.CheckIfAllIngredientsAreDeposited();
        }

        protected virtual bool CheckIfAnyIngredientsIsDeposited()
        {
            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item != null)
                {
                    return true;
                }
            }

            return false;
        }

        protected virtual void DoPermitA39EasterEgg()
        {
            var numberOwned = 0;
            foreach (var ingredient in CurrentPageBundle.Ingredients)
            {
                if (Game1.player.Items.ContainsId(ingredient.id))
                {
                    numberOwned++;
                }
            }

            if (numberOwned < CurrentPageBundle.Ingredients.Count - 1)
            {
                Game1.chatBox.addMessage($"Congrats on obtaining Permit A39!", Color.Purple);
            }
        }

        private bool CheckIfAllIngredientsAreTakenOut()
        {
            var num = 0;
            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item == null)
                {
                    ++num;
                }
            }
            if (num < CurrentPageBundle.NumberOfIngredientSlots)
            {
                return false;
            }
            return true;
        }

        protected virtual bool CheckIfAllIngredientsIsDepositedExcludingTheBundlesThemselves()
        {
            var num = 0;
            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.item != null)
                {
                    ++num;
                }
            }
            return num >= CurrentPageBundle.NumberOfIngredientSlots;
        }

        protected override void DrawInventory(SpriteBatch b)
        {
            if (CurrentPageBundle.name == MemeBundleNames.OFF_YOUR_BACK)
            {
                _clothesMenu.draw(b);
                return;
            }
            base.DrawInventory(b);
        }

        protected override void DrawIngredientShadow(SpriteBatch spriteBatch, ClickableTextureComponent ingredient, float transparency)
        {
            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                return;
            }
            base.DrawIngredientShadow(spriteBatch, ingredient, transparency);
        }

        protected override void DrawIngredient(SpriteBatch spriteBatch, BundleIngredientDescription? ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            if (CurrentPageBundle.name == MemeBundleNames.OFF_YOUR_BACK)
            {
                if (ingredient == null || ingredientBox.item == null || !ingredientBox.visible)
                {
                    return;
                }
                DrawWornHat(spriteBatch, ingredient.Value, ingredientBox, overlayTransparency);
                DrawWornBoots(spriteBatch, ingredient.Value, ingredientBox, overlayTransparency);
                DrawWornPants(spriteBatch, ingredient.Value, ingredientBox, overlayTransparency);
                DrawWornShirt(spriteBatch, ingredient.Value, ingredientBox, overlayTransparency);
                DrawWornRing(spriteBatch, ingredient.Value, ingredientBox, overlayTransparency);
                return;
            }

            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                return;
            }

            base.DrawIngredient(spriteBatch, ingredient, ingredientBox, overlayTransparency);
        }

        private static void DrawWornHat(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_HAT, "Used Hat", Game1.player.hat.Value, 42);
        }

        private static void DrawWornBoots(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_BOOTS, "Used Boots", Game1.player.boots.Value, 40);
        }

        private static void DrawWornPants(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_PANTS, "Used Pants", Game1.player.pantsItem.Value, 68);
        }

        private static void DrawWornShirt(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_SHIRT, "Used Shirt", Game1.player.shirtItem.Value, 69);
        }

        private static void DrawWornRing(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_LEFT_RING, "Used Left Ring", Game1.player.leftRing.Value, 41);
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_RIGHT_RING, "Used Right Ring", Game1.player.rightRing.Value, 41);
        }

        private static void DrawWornItem(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, string wornItemId, string hoverText, Item wornItem, int emptySlotTilePosition)
        {
            if (ingredient.id != wornItemId)
            {
                return;
            }

            ingredientBox.hoverText = hoverText;
            if (wornItem == null)
            {
                spriteBatch.Draw(Game1.menuTexture, ingredientBox.bounds, new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, emptySlotTilePosition)), Color.White);
            }
            else
            {
                wornItem.drawInMenu(spriteBatch, new Vector2(ingredientBox.bounds.X, ingredientBox.bounds.Y), ingredientBox.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * overlayTransparency, false);
            }
        }

        protected override void PickItemFromInventory(int x, int y)
        {
            if (CurrentPageBundle.name == MemeBundleNames.OFF_YOUR_BACK)
            {
                HeldItem = _clothesMenu.leftClick(x, y, HeldItem);
                return;
            }
            base.PickItemFromInventory(x, y);
        }

        protected override string GetBundleNameText()
        {
            if (CurrentPageBundle.name == MemeBundleNames.BUN_DLE)
            {
                return MemeBundleNames.BUN_DLE;
            }
            return base.GetBundleNameText();
        }

        protected override void DrawSpecificBundle(SpriteBatch b)
        {
            base.DrawSpecificBundle(b);
            if (CurrentPageBundle.name == MemeBundleNames.FLASHBANG)
            {
                b.Draw(CurrentPageBundle.BundleTextureOverride, new Vector2(0, 0), new Rectangle(Game1.viewport.Width, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            }
        }

        public static void OnUpdateTickedStatic()
        {
            if (Game1.activeClickableMenu is ArchipelagoJunimoNoteMenu junimoNoteMenu)
            {
                junimoNoteMenu.OnUpdateTicked();
            }
        }

        public void OnUpdateTicked()
        {
            if (!SpecificBundlePage || CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.NFT)
            {
                return;
            }

            if (DidPlayerJustScreenshot())
            {
                PerformCurrencyPurchase();
            }
        }

        public bool DidPlayerJustScreenshot(bool ignoreNonMouseHeldInput = false)
        {
            var keyboard = Game1.input.GetKeyboardState();
            var pressedKeys = keyboard.GetPressedKeys(); 
            var hasPressedScreenshotKey = pressedKeys.Contains(Keys.F12) || pressedKeys.Contains(Keys.PrintScreen) || pressedKeys.Contains(Keys.Print);
            var hasPressedWindows = pressedKeys.Contains(Keys.LeftWindows) || pressedKeys.Contains(Keys.RightWindows);
            var hasPressedShift = pressedKeys.Contains(Keys.LeftShift) || pressedKeys.Contains(Keys.RightShift);
            var hasSummonedSnippingTool = hasPressedWindows && hasPressedShift && pressedKeys.Contains(Keys.S);
            return hasPressedScreenshotKey || hasSummonedSnippingTool;
        }

        protected override ClickableTextureComponent CreateIngredientButton(ParsedItemData dataOrErrorItem, Rectangle bounds, int index, string hoverText, Item flavoredItem)
        {
            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                var offsetY = INGREDIENT_SLOTS_CENTER_Y - INGREDIENTS_CENTER_Y;
                bounds.Y += offsetY;
            }
            return base.CreateIngredientButton(dataOrErrorItem, bounds, index, hoverText, flavoredItem);
        }

        protected override void DrawIngredientSlot(SpriteBatch b, int index)
        {
            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                var ingredientSlot = IngredientSlots[index];
                ingredientSlot.draw(b, (FromGameMenu ? Color.LightGray * 0.5f : Color.White), 0.89f);
                ingredientSlot.drawItem(b, 4, 4, 1f);
                return;
            }

            base.DrawIngredientSlot(b, index);
        }

        protected override void DrawIngredients(SpriteBatch spriteBatch)
        {
            base.DrawIngredients(spriteBatch);
        }

        public int GetValidSchrodingerIndex()
        {
            var inventorySeed = GetInventorySeed();
            var seed = Utility.CreateRandomSeed(Game1.uniqueIDForThisGame, inventorySeed);
            var random = new Random(seed);
            var index = random.Next(GetIngredientsStartIndex(), GetIngredientsEndIndex());
            return index;
        }

        public static int GetInventorySeed()
        {
            unchecked
            {
                var inventorySeed = 0;
                foreach (var playerItem in Game1.player.Items)
                {
                    if (playerItem == null)
                    {
                        inventorySeed = (inventorySeed * 9) + 3;
                        continue;
                    }
                    inventorySeed = (inventorySeed * 7) + playerItem.Category;
                    inventorySeed = (inventorySeed * 7) + playerItem.ParentSheetIndex;
                    inventorySeed = (inventorySeed * 7) + playerItem.Quality;
                    inventorySeed = (inventorySeed * 7) + playerItem.Stack;
                }
                return inventorySeed;
            }
        }

        protected override void DrawIngredientSlots(SpriteBatch b)
        {
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                DrawIngredientSlotsBundleBundle(b);
                return;
            }
            base.DrawIngredientSlots(b);
        }

        private void DrawIngredientSlotsBundleBundle(SpriteBatch b)
        {
            if (_bundleBundleIndex == -1)
            {
                var startIndex = NUMBER_SUB_BUNDLES * ingredientsPerSubBundle;
                var endIndex = startIndex + NUMBER_SUB_BUNDLES;
                for (var index = startIndex; index < endIndex; ++index)
                {
                    var subBundleIndex = index - startIndex;
                    var y = 244 + (subBundleIndex * 16);
                    var sourceRect = new Rectangle(16, y, 16, 16);
                    if (SubBundleComplete(subBundleIndex))
                    {
                        sourceRect.X += 9 * 16;
                    }
                    else
                    {
                        IngredientSlots[index].draw(b, (FromGameMenu ? Color.LightGray * 0.5f : Color.White), 0.89f);
                    }
                    b.Draw(NoteTexture, IngredientSlots[index].bounds, sourceRect, Color.White);
                   // DrawIngredientSlot(b, index);
                }
                return;
            }
            base.DrawIngredientSlots(b);
        }

        private bool SubBundleComplete(int subBundleIndex)
        {
            var startIndex = subBundleIndex * ingredientsPerSubBundle;
            var endIndex = startIndex + ingredientsPerSubBundle;
            var num = 0;
            for (var index = startIndex; index < endIndex; index++)
            {
                var ingredientSlot = IngredientSlots[index];
                if (ingredientSlot.item != null)
                {
                    ++num;
                }
            }
            return num >= ingredientsPerSubBundle;
        }

        protected override void DrawTemporarySprites(SpriteBatch b)
        {
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                if (_bundleBundleIndex == -1)
                {
                    return;
                }
            }
            base.DrawTemporarySprites(b);
        }

        protected override int GetIngredientsStartIndex()
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.BUNDLE_BUNDLE)
            {
                return base.GetIngredientsStartIndex();
            }

            return GetIngredientsStartIndex(_bundleBundleIndex);
        }

        protected override int GetIngredientsEndIndex()
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.BUNDLE_BUNDLE)
            {
                return base.GetIngredientsEndIndex();
            }

            return GetIngredientsEndIndex(_bundleBundleIndex);
        }

        protected override int GetIngredientSlotsStartIndex()
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.BUNDLE_BUNDLE)
            {
                return base.GetIngredientSlotsStartIndex();
            }

            return GetIngredientSlotsStartIndex(_bundleBundleIndex);
        }

        protected override int GetIngredientSlotsEndIndex()
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.BUNDLE_BUNDLE)
            {
                return base.GetIngredientSlotsEndIndex();
            }

            return GetIngredientSlotsEndIndex(_bundleBundleIndex);
        }

        private int GetIngredientsStartIndex(int bundleBundleIndex)
        {
            if (bundleBundleIndex == -1)
            {
                return 0;
            }

            return bundleBundleIndex * ingredientsPerSubBundle;
        }

        private int GetIngredientsEndIndex(int bundleBundleIndex)
        {
            if (bundleBundleIndex == -1)
            {
                return 0;
            }

            return (bundleBundleIndex + 1) * ingredientsPerSubBundle;
        }

        private int GetIngredientSlotsStartIndex(int bundleBundleIndex)
        {
            if (bundleBundleIndex == -1)
            {
                return NUMBER_SUB_BUNDLES * ingredientsPerSubBundle;
            }

            return bundleBundleIndex * ingredientsPerSubBundle;
        }

        private int GetIngredientSlotsEndIndex(int bundleBundleIndex)
        {

            if (bundleBundleIndex == -1)
            {
                return (NUMBER_SUB_BUNDLES + 1) * ingredientsPerSubBundle;
            }

            return (bundleBundleIndex + 1) * ingredientsPerSubBundle;
        }

        protected void UpdateIngredientSlotsBundleBundle()
        {
            for (var bundleIndex = 0; bundleIndex < NUMBER_SUB_BUNDLES; bundleIndex++)
            {
                var startIndex = GetIngredientSlotsStartIndex(bundleIndex);
                var endIndex = GetIngredientSlotsEndIndex(bundleIndex);
                var ingredientSlotIndex = startIndex;
                for (var i = startIndex; i < endIndex; i++)
                {
                    var ingredient = CurrentPageBundle.Ingredients[i];
                    ingredientSlotIndex = UpdateIngredientSlotBundleBundle(ingredient, ingredientSlotIndex);
                }
            }
        }

        protected int UpdateIngredientSlotBundleBundle(BundleIngredientDescription ingredient, int index)
        {
            if (!ingredient.completed || index >= IngredientSlots.Count)
            {
                return index;
            }

            var representativeItemId = GetRepresentativeItemId(ingredient);
            if (ingredient.preservesId != null)
            {
                IngredientSlots[index].item = Utility.CreateFlavoredItem(representativeItemId, ingredient.preservesId, ingredient.quality, ingredient.stack);
            }
            else
            {
                IngredientSlots[index].item = ItemRegistry.Create(representativeItemId, ingredient.stack, ingredient.quality);
            }
            CurrentPageBundle.IngredientDepositAnimation(IngredientSlots[index], NOTE_TEXTURE_NAME, true);
            ++index;
            return index;
        }
    }
}
