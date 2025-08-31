#nullable disable
using Archipelago.MultiClient.Net.Models;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.Gifting;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Modded;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Extensions;
using StardewArchipelago.Items.Traps;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Gacha;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes;
using StardewArchipelago.Logging;
using StardewArchipelago.Serialization;
using StardewArchipelago.Stardew;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using static HarmonyLib.Code;
using Color = Microsoft.Xna.Framework.Color;
using Object = StardewValley.Object;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ArchipelagoJunimoNoteMenu : JunimoNoteMenuRemake
    {
        private const int REMIXED_BUNDLE_INDEX_THRESHOLD = 100;
        private const int CUSTOM_BUNDLE_INDEX_THRESHOLD = 200;

        private static LogHandler _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static ArchipelagoWalletDto _wallet;
        private static BankHandler _bank;
        private static LocationChecker _locationChecker;
        private static BundleReader _bundleReader;
        private static TrapManager _trapManager;
        private BundleCurrencyManager _currencyManager;

        public static bool SisyphusStoneNeedsToFall = false;
        internal static int SisyphusIndex = 0;
        internal static int BureaucracyIndex = -1;
        internal ClothesMenu _clothesMenu;
        internal int _bundleBundleIndex = -1;
        internal const int NUMBER_SUB_BUNDLES = 4;
        internal int ingredientsPerSubBundle => IngredientList?.Count / NUMBER_SUB_BUNDLES ?? 0;
        internal int ingredientsSlotsPerSubBundle => CurrentPageBundle?.NumberOfIngredientSlots / NUMBER_SUB_BUNDLES ?? 0;
        internal static Stopwatch DayStopwatch = new Stopwatch();
        internal static int FloorIsLavaHasTouchedGroundToday = 0;
        internal static bool HasLookedAtRestraintBundleToday = false;
        internal static bool HasLookedAtStanleyBundleToday = false;
        internal static bool HasPurchasedRestraintBundleToday = false;
        internal static bool HasLookedAtHibernationBundleToday = false;
        internal static string IkeaItemQualifiedId = "";
        private Hint[] _hintsForMe;
        private Hint[] _hintsFromMe;
        private GachaResolver _gachaResolver;
        private static SlidingPuzzleHandler _slidingPuzzle;
        private string[] _asmrCues = null;
        private Dictionary<string, string[]> _squareHoleCues = null;
        private ICue _currentCue;
        private bool _isCurrentlySticky = false;
        private Point _stickyPosition = Point.Zero;

        public Texture2D MemeTexture;
        public Texture2D HumbleBundleTexture;
        private BundleButton _donateButton;
        public Dictionary<BundleButton, Action> ExtraButtons;

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
            _currencyManager = new BundleCurrencyManager(_logger, _modHelper, _wallet, _bank, this);
            var memeUIFolder = Path.Combine("Bundles", "UI");
            var memeAssetsPath = Path.Combine(memeUIFolder, "MemeBundleAssets.png");
            MemeTexture = TexturesLoader.GetTexture(memeAssetsPath);
            var humbleIconPath = Path.Combine(memeUIFolder, "humble_button.png");
            HumbleBundleTexture = TexturesLoader.GetTexture(humbleIconPath);
            ExtraButtons = new Dictionary<BundleButton, Action>();
            InitializeClothesMenu();
            _hintsForMe = _archipelago.GetActiveDesiredHintsForMe();
            _hintsFromMe = _archipelago.GetMyActiveDesiredHints();
        }

        private void InitializeClothesMenu()
        {
            var capacity = Game1.player.maxItems.Value;
            var rows = 6;
            var width = 64 * ((capacity == -1 ? 36 : capacity) / rows);
            var height = 64 * rows + 16;
            _clothesMenu = new ClothesMenu(_modHelper, xPositionOnScreen + 128, yPositionOnScreen + 140, width, height);
        }

        public static void InitializeArchipelago(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, BankHandler bank, LocationChecker locationChecker, TrapManager trapManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _wallet = state.Wallet;
            _bank = bank;
            _locationChecker = locationChecker;
            _trapManager = trapManager;
            _bundleReader = new BundleReader();
        }

        public override void CheckForRewards()
        {
            _bundleReader.CheckAllBundleLocations(_locationChecker);
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

        protected override int SetUpWhichArea(bool fromGameMenu, bool fromThisMenu, CommunityCenter communityCenter, int area)
        {
            if (fromGameMenu && !fromThisMenu)
            {
                for (var index = 0; index < communityCenter.areasComplete.Count; ++index)
                {
                    if (communityCenter.shouldNoteAppearInArea(index) && !communityCenter.areasComplete[index])
                    {
                        area = index;
                        WhichArea = area;
                        break;
                    }
                }

                var canAccessMissingBundle = Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("abandonedJojaMartAccessible");
                var missingBundleNotDone = _locationChecker.IsLocationMissing("The Missing Bundle");

                if (canAccessMissingBundle && missingBundleNotDone)
                {
                    area = 6;
                }
            }

            return area;
        }

        protected override InventoryMenu SetupInventoryMenu()
        {
            if (_modHelper.ModRegistry.IsLoaded(ModUniqueIds.UniqueIds[ModNames.BIGGER_BACKPACK]))
            {
                var xPosition = xPositionOnScreen + 104;
                var yPosition = yPositionOnScreen + 120;
                var capacity = 49;
                var rows = 7;
                var gap = 4;
                return SetupInventoryMenu(xPosition, yPosition, capacity, rows, gap);
            }
            return base.SetupInventoryMenu();
        }

        protected override ClickableTextureComponent SetUpBackButton()
        {
            if (_modHelper.ModRegistry.IsLoaded(ModUniqueIds.UniqueIds[ModNames.BIGGER_BACKPACK]))
            {
                var xPosition = xPositionOnScreen;
                var yPosition = yPositionOnScreen;
                return SetUpBackButton(xPosition, yPosition);
            }
            return base.SetUpBackButton();
        }

        protected override void SetUpScramblingAndMail(int whichArea)
        {
            if (!FromGameMenu)
            {
                _locationChecker.AddCheckedLocation("Quest: Rat Problem");
                Game1.player.removeQuest("26");
                if (!Game1.player.hasOrWillReceiveMail("seenJunimoNote"))
                {
                    Game1.player.mailReceived.Add("seenJunimoNote");
                }
                if (!Game1.player.hasOrWillReceiveMail("hasSeenAbandonedJunimoNote") && whichArea == 6)
                {
                    Game1.player.mailReceived.Add("hasSeenAbandonedJunimoNote");
                }
            }
            ScrambledText = !CanReadNote();
        }

        protected override bool CanReadNote()
        {
            return _archipelago.HasReceivedItem("Forest Magic");
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
            if (TryGetSpecialRewardName(whichArea, out var specialRewardName))
            {
                return specialRewardName;
            }

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

        private bool TryGetSpecialRewardName(int whichArea, out string specialRewardName)
        {
            if (TryGetClickbaitRewardName(whichArea, out specialRewardName))
            {
                return true;
            }

            if (TryGetInvestmentBundleRewardName(whichArea, out specialRewardName))
            {
                return true;
            }

            return false;
        }

        private bool TryGetClickbaitRewardName(int whichArea, out string specialRewardName)
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.CLICKBAIT)
            {
                specialRewardName = "";
                return false;
            }

            if (_hintsForMe.Any())
            {
                var hint = _hintsForMe.First();
                specialRewardName = $"Reward: {_archipelago.GetPlayerName(hint.ReceivingPlayer)}'s {_archipelago.GetItemName(hint.ItemId)}";
                return true;
            }

            if (_hintsFromMe.Any())
            {
                var hint = _hintsFromMe.First();
                specialRewardName = $"Reward: {_archipelago.GetPlayerName(hint.ReceivingPlayer)}'s {_archipelago.GetItemName(hint.ItemId)}";
                return true;
            }

            var goodItems = new List<string> { "Greenhouse", "Dwarvish Translation Guide", "Bridge Repair", "Rusty Key", "Bus Repair", "Minecarts Repair", "Gold Clock", "Desert Obelisk", "Island Obelisk" };
            goodItems = goodItems.Shuffle(new Random((int)(Game1.uniqueIDForThisGame / 2)));
            var myName = _archipelago.GetPlayerName();
            foreach (var goodItem in goodItems)
            {
                if (!_archipelago.HasReceivedItem(goodItem))
                {
                    specialRewardName = $"Reward: {myName}'s {goodItem}";
                    return true;
                }
            }

            var goodRepeatItems = new[] { "Progressive Weapon", "Progressive Coop", "Progressive Barn" };
            goodRepeatItems = goodRepeatItems.OrderBy(x => _archipelago.GetReceivedItemCount(x)).ToArray();

            specialRewardName = $"Reward: {myName}'s {goodRepeatItems[0]}";
            return true;
        }

        private bool TryGetInvestmentBundleRewardName(int whichArea, out string specialRewardName)
        {
            if (CurrentPageBundle == null || (CurrentPageBundle.name != MemeBundleNames.SCAM && CurrentPageBundle.name != MemeBundleNames.INVESTMENT))
            {
                specialRewardName = "";
                return false;
            }

            var price = CurrentPageBundle.Ingredients.First().stack;
            var minReturn = 1.6 * price;
            var maxReturn = 2.5 * price;
            specialRewardName = $"Reward: {minReturn}g to {maxReturn}g";
            return true;
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
                if (extraButton == null || extraButton.IsAnimating() || !extraButton.containsPoint(x, y))
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

            if (CurrentPageBundle.name == MemeBundleNames.RESTRAINT)
            {
                PurchaseButton = null;
                HasPurchasedRestraintBundleToday = true;
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

        public override void PerformCurrencyPurchase()
        {
            base.PerformCurrencyPurchase();
            if (CurrentPageBundle.name == MemeBundleNames.SCAM || CurrentPageBundle.name == MemeBundleNames.INVESTMENT)
            {
                var random = Utility.CreateDaySaveRandom();
                var paymentMethods = new[] {"gift cards", "Bitcoins", "Ethereum" };
                var companies = new[] { "CashPyramid.biz", "Stonks United", "TrustMeBro Capital", "GrandmaCoin Investments", "SafeAndSecureBank123", "ProfitHub", "Joja Crypto Fund", "Ponzi & Sons", "Bundle Return On Kapital Enterprise" };
                var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
                var company = companies[random.Next(companies.Length)];
                Game1.chatBox.addMessage($"We have received your {paymentMethod} without issue.", Color.Green);
                Game1.chatBox.addMessage($"Thank you for investing with {company}!", Color.Green);
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

        public override bool IsReadyToCloseMenuOrBundle()
        {
            if (_isCurrentlySticky)
            {
                if (SpecificBundlePage)
                {
                    var currentPageBundle = this.CurrentPageBundle;
                    if ((currentPageBundle != null ? currentPageBundle.CompletionTimer > 0 ? 1 : 0 : 0) != 0)
                    {
                        return false;
                    }
                }
                return true;
            }

            return base.IsReadyToCloseMenuOrBundle();
        }

        protected virtual bool TryGetBundleLocationToScout(out string apLocationToScout)
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

        protected override void SetUpIngredientButtons(BundleRemake b)
        {
            if (CurrentPageBundle.name == MemeBundleNames.JOURNALIST)
            {
                CurrentPageBundle.Ingredients[0] = FindOneEasilyObtainableItem();
            }
            if (CurrentPageBundle.name == MemeBundleNames.DOCTOR)
            {
                var numberApples = (int)Math.Clamp(Game1.stats.DaysPlayed, 1, 999);
                var existingIngredient = CurrentPageBundle.Ingredients.First();
                CurrentPageBundle.Ingredients.Clear(); ;
                var appleIngredient = new BundleIngredientDescription(QualifiedItemIds.APPLE, numberApples, 0, existingIngredient.completed);
                CurrentPageBundle.Ingredients.Add(appleIngredient);
            }
            base.SetUpIngredientButtons(b);
        }

        private BundleIngredientDescription FindOneEasilyObtainableItem()
        {
            if (TryPickFromInventory(out var easilyDonatedItem))
            {
                return easilyDonatedItem;
            }

            if (TryPickFromEntireWorld(out var easilyObtainableItem))
            {
                return easilyObtainableItem;
            }

            return Game1.season switch
            {
                Season.Spring => new BundleIngredientDescription(QualifiedItemIds.LEEK, 1, 0, false),
                Season.Summer => new BundleIngredientDescription(QualifiedItemIds.GRAPE, 1, 0, false),
                Season.Fall => new BundleIngredientDescription(QualifiedItemIds.BLACKBERRY, 1, 0, false),
                Season.Winter => new BundleIngredientDescription(QualifiedItemIds.CRYSTAL_FRUIT, 1, 0, false),
                _ => new BundleIngredientDescription(QualifiedItemIds.WOOD, 1, 0, false)
            };
        }

        private static bool TryPickFromInventory(out BundleIngredientDescription easilyObtainableItem)
        {
            var validObjects = new List<Object>();
            foreach (var playerItem in Game1.player.Items)
            {
                if (playerItem is Object ownedObject && ownedObject.HasBeenInInventory)
                {
                    validObjects.Add(ownedObject);
                }
            }

            return TryGetCheapestObject(validObjects, out easilyObtainableItem);
        }

        private static bool TryPickFromEntireWorld(out BundleIngredientDescription easilyObtainableItem)
        {
            var validObjects = new List<Object>();
            Utility.ForEachItem(item =>
            {
                if (item is Object ownedObject && ownedObject.HasBeenInInventory)
                    validObjects.Add(ownedObject);
                return true;
            });

            return TryGetCheapestObject(validObjects, out easilyObtainableItem);
        }

        private static bool TryGetCheapestObject(List<Object> validObjects, out BundleIngredientDescription easilyObtainableItem)
        {
            validObjects = validObjects.Where(x => x.salePrice() > 0).ToList();
            if (!validObjects.Any())
            {
                easilyObtainableItem = new BundleIngredientDescription();
                return false;
            }

            validObjects = validObjects.OrderBy(x => x.salePrice()).ToList();
            easilyObtainableItem = new BundleIngredientDescription(validObjects[0].ItemId, 1, 0, false);
            return true;
        }

        protected override void SetUpCurrencyButtons()
        {
            base.SetUpCurrencyButtons();
            switch (CurrentPageBundle.name)
            {
                case MemeBundleNames.VAMPIRE:
                case MemeBundleNames.EXHAUSTION:
                case MemeBundleNames.TICK_TOCK:
                    SetUpDonateButton(true);
                    break;
                case MemeBundleNames.COOKIE_CLICKER:
                    SetUpCookiesButtons();
                    break;
                case MemeBundleNames.GACHA:
                    SetUpGachaButtons();
                    break;
                case MemeBundleNames.HUMBLE:
                    SetUpHumbleBundleButtons();
                    break;
                case MemeBundleNames.DEATH:
                    SetUpMonstersButton();
                    break;
            }
        }

        protected override void SetUpPurchaseButton()
        {
            if (CurrentPageBundle.name == MemeBundleNames.NFT || CurrentPageBundle.name == MemeBundleNames.DEATH || CurrentPageBundle.name == MemeBundleNames.HONEYWELL)
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.FAST && DayStopwatch.ElapsedMilliseconds > BundleCurrencyManager.GetFastBundleAllowedTime(CurrentPageBundle.Ingredients.First()))
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.FLOOR_IS_LAVA && FloorIsLavaHasTouchedGroundToday > 0)
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.HIBERNATION && !FromGameMenu)
            {
                HasLookedAtHibernationBundleToday = true;
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.GACHA)
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.ASMR)
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.PUZZLE)
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.HUMBLE)
            {
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.HAIRY)
            {
                if (Game1.player.getHair() != BundleCurrencyManager.BALD_HAIR)
                {
                    SetUpDonateButton(false);
                }
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.CLIQUE)
            {
                SetUpCliqueButton();
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.RESTRAINT && !FromGameMenu)
            {
                HasLookedAtRestraintBundleToday = true;
            }
            if (CurrentPageBundle.name == MemeBundleNames.STANLEY)
            {
                HasLookedAtStanleyBundleToday = true;
                return;
            }

            base.SetUpPurchaseButton();
        }

        private void SetUpCliqueButton()
        {
            if (FromGameMenu)
            {
                return;
            }
            var textureComponent = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 800, yPositionOnScreen + 504, 260, 72), MemeTexture, new Rectangle(0, 20, 65, 20), 4f);
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

        private void SetUpDonateButton(bool abovePurchaseButton)
        {
            if (FromGameMenu)
            {
                return;
            }

            var yPosition = yPositionOnScreen + (abovePurchaseButton ? 350 : 504);
            var donateButton = new BundleButton(new Rectangle(xPositionOnScreen + 825, yPosition, 260, 72), MemeTexture, new Rectangle(0, 0, 53, 20), 4f);
            donateButton.myID = 796;
            donateButton.leftNeighborID = REGION_BACK_BUTTON;
            donateButton.rightNeighborID = REGION_PURCHASE_BUTTON;
            _donateButton = donateButton;
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
            var cursorBackground = new BundleButton(cursorButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cursorTextureRect = new Rectangle(0, 0, 8, 10);
            var cursorRect = GetCenteredTexture(cursorButtonRect, cursorTextureRect, buttonScale, buttonScale);
            var cursorButton = new BundleButton(cursorRect, Game1.mouseCursors, cursorTextureRect, buttonScale);
            cursorButton.myID = 793;

            var cookieButtonRect = new Rectangle(xStart + xPerButton, y, 72, 72);
            var cookieBackground = new BundleButton(cookieButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cookieTextureRect = new Rectangle(112, 144, 16, 16);
            var cookieRect = GetCenteredTexture(cookieButtonRect, cookieTextureRect, buttonScale, buttonScale);
            var cookieButton = new BundleButton(cookieRect, Game1.objectSpriteSheet, cookieTextureRect, buttonScale);
            cookieButton.myID = 794;

            var grandmaButtonRect = new Rectangle(xStart + (xPerButton*2), y, 72, 72);
            var grandmaBackground = new BundleButton(grandmaButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var grandmaTextureRect = new Rectangle(0, 168, 16, 24);
            var grandmaRect = GetCenteredTexture(grandmaButtonRect, grandmaTextureRect, buttonScale, grandmaScale);
            var grandmaButton = new BundleButton(grandmaRect, Game1.getCharacterFromName("Evelyn").Sprite.Texture, grandmaTextureRect, grandmaScale);
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

        private void SetUpMonstersButton()
        {
            if (FromGameMenu)
            {
                return;
            }

            var buttonBackgroundRectangle = new Rectangle(512, 244, 18, 18);
            var xStart = xPositionOnScreen + 800;
            var xPerButton = 100;
            var y = yPositionOnScreen + 504;
            var buttonScale = 4f;

            var dangerBackgroundRect = new Rectangle(xStart + xPerButton, y, 72, 72);
            var dangerBackground = new BundleButton(dangerBackgroundRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var dangerTextureRect = new Rectangle(240, 1808, 16, 16);
            var dangerRect = GetCenteredTexture(dangerBackgroundRect, dangerTextureRect, buttonScale, buttonScale);
            var dangerButton = new BundleButton(dangerRect, Game1.mouseCursors, dangerTextureRect, buttonScale);
            dangerButton.myID = 794;

            dangerButton.leftNeighborID = REGION_BACK_BUTTON;
            dangerButton.rightNeighborID = REGION_PURCHASE_BUTTON;

            ExtraButtons.Add(dangerBackground, () => { });
            ExtraButtons.Add(dangerButton, () =>
            {
                _trapManager.ExecuteTrapImmediately("Monsters Trap");
                _trapManager.TrapExecutor.MonsterSpawner.SpawnOneMonster(Game1.player.currentLocation, _archipelago.SlotData.TrapItemsDifficulty);
                exitThisMenu();
            });
        }

        private void SetUpGachaButtons()
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
            var chestsScale = 3f;

            var commonButtonRect = new Rectangle(xStart, y, 72, 72);
            var commonBackground = new BundleButton(commonButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var commonTextureRect = new Rectangle(64, 1952, 32, 32);
            var commonRect = GetCenteredTexture(commonButtonRect, commonTextureRect, buttonScale, chestsScale);
            var commonButton = new BundleButton(commonRect, Game1.mouseCursors, commonTextureRect, chestsScale);
            commonButton.myID = 793;
            commonButton.hoverText = "Common Chest";
            commonButton.SetDrawOffset(new Vector2(-2, -18));
            commonButton.SetPressAnimation(Game1.mouseCursors, commonTextureRect, new Vector2(32, 0), 4);

            var rareButtonRect = new Rectangle(xStart + xPerButton, y, 72, 72);
            var rareBackground = new BundleButton(rareButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var rareTextureRect = new Rectangle(64, 1920, 32, 32);
            var rareRect = GetCenteredTexture(rareButtonRect, rareTextureRect, buttonScale, chestsScale);
            var rareButton = new BundleButton(rareRect, Game1.mouseCursors, rareTextureRect, chestsScale);
            rareButton.myID = 794;
            rareButton.hoverText = "Rare Chest";
            rareButton.SetDrawOffset(new Vector2(-2, -18));
            rareButton.SetPressAnimation(Game1.mouseCursors, rareTextureRect, new Vector2(32, 0), 4);

            var legendaryButtonRect = new Rectangle(xStart + (xPerButton * 2), y, 72, 72);
            var legendaryBackground = new BundleButton(legendaryButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var legendaryTextureRect = new Rectangle(256, 75, 32, 32);
            var legendaryRect = GetCenteredTexture(legendaryButtonRect, legendaryTextureRect, buttonScale, chestsScale);
            var legendaryButton = new BundleButton(legendaryRect, Game1.mouseCursors_1_6, legendaryTextureRect, chestsScale);
            legendaryButton.myID = 795;
            legendaryButton.hoverText = "Legendary Chest";
            legendaryButton.SetDrawOffset(new Vector2(0, -6));
            legendaryButton.SetPressAnimation(Game1.mouseCursors_1_6, legendaryTextureRect, new Vector2(32, 0), 4);

            commonButton.leftNeighborID = REGION_BACK_BUTTON;
            commonButton.rightNeighborID = rareButton.myID;
            rareButton.leftNeighborID = commonButton.myID;
            rareButton.rightNeighborID = legendaryButton.myID;
            legendaryButton.leftNeighborID = rareButton.myID;
            legendaryButton.rightNeighborID = REGION_PURCHASE_BUTTON;

            _gachaResolver = new GachaResolver(CurrentPageBundle.Ingredients.First().stack);

            ExtraButtons.Add(rareBackground, () => { });
            ExtraButtons.Add(commonBackground, () => { });
            ExtraButtons.Add(legendaryBackground, () => { });
            ExtraButtons.Add(commonButton, () => _gachaResolver.PressButton(commonButton, GachaRoller.COMMON_PRICE, this));
            ExtraButtons.Add(rareButton, () => _gachaResolver.PressButton(rareButton, GachaRoller.RARE_PRICE, this));
            ExtraButtons.Add(legendaryButton, () => _gachaResolver.PressButton(legendaryButton, GachaRoller.LEGENDARY_PRICE, this));
        }

        private void SetUpHumbleBundleButtons()
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
            var iconScale = 0.5625f;
            var bundleTextureRect = new Rectangle(0, 0, 128, 128);

            var cheapButtonRect = new Rectangle(xStart, y, 72, 72);
            var cheapBackground = new BundleButton(cheapButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var cheapRect = GetCenteredTexture(cheapButtonRect, bundleTextureRect, buttonScale, iconScale);
            var cheapButton = new BundleButton(cheapRect, HumbleBundleTexture, bundleTextureRect, iconScale);
            cheapButton.myID = 793;
            cheapButton.hoverText = "Cheap Donation";
            
            var normalButtonRect = new Rectangle(xStart + xPerButton, y, 72, 72);
            var normalBackground = new BundleButton(normalButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var normalRect = GetCenteredTexture(normalButtonRect, bundleTextureRect, buttonScale, iconScale);
            var normalButton = new BundleButton(normalRect, HumbleBundleTexture, bundleTextureRect, iconScale);
            normalButton.myID = 794;
            normalButton.hoverText = "Normal Donation";
            
            var generousButtonRect = new Rectangle(xStart + (xPerButton * 2), y, 72, 72);
            var generousBackground = new BundleButton(generousButtonRect, NoteTexture, buttonBackgroundRectangle, buttonScale);
            var generousRect = GetCenteredTexture(generousButtonRect, bundleTextureRect, buttonScale, iconScale);
            var generousButton = new BundleButton(generousRect, HumbleBundleTexture, bundleTextureRect, iconScale);
            generousButton.myID = 795;
            generousButton.hoverText = "Generous Donation";

            cheapButton.leftNeighborID = REGION_BACK_BUTTON;
            cheapButton.rightNeighborID = normalButton.myID;
            normalButton.leftNeighborID = cheapButton.myID;
            normalButton.rightNeighborID = generousButton.myID;
            generousButton.leftNeighborID = normalButton.myID;
            generousButton.rightNeighborID = REGION_PURCHASE_BUTTON;

            _gachaResolver = new GachaResolver(CurrentPageBundle.Ingredients.First().stack);

            ExtraButtons.Add(normalBackground, () => { });
            ExtraButtons.Add(cheapBackground, () => { });
            ExtraButtons.Add(generousBackground, () => { });
            ExtraButtons.Add(cheapButton, () => _currencyManager.PurchaseWithCharityDonation(this, 0.1, 0.5));
            ExtraButtons.Add(normalButton, () => _currencyManager.PurchaseWithCharityDonation(this, 1, 2));
            ExtraButtons.Add(generousButton, () => _currencyManager.PurchaseWithCharityDonation(this, 10, 5));
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
            if (_currentCue != null)
            {
                _currentCue.Stop(AudioStopOptions.Immediate);
                _currentCue.Dispose();
                _currentCue = null;
            }

            base.TakeDownSpecificBundleComponents();
            _donateButton = null;
            ExtraButtons.Clear();
            _isCurrentlySticky = false;
        }

        public override void update(GameTime time)
        {
            base.update(time);
            if (_isCurrentlySticky && Game1.ticks % 2 == 0)
            {
                var nextPosition = GetPointTowardsSticky(0.05);
                Game1.setMousePosition(nextPosition);
            }
        }

        private Point GetPointTowardsSticky(double percentDistance)
        {
            var mousePosition = Game1.getMousePosition();
            var deltaX = _stickyPosition.X - mousePosition.X;
            var deltaY = _stickyPosition.Y - mousePosition.Y;
            deltaX = deltaX > 0 ? (int)Math.Ceiling(deltaX * percentDistance) : (int)Math.Floor(deltaX * percentDistance);
            deltaY = deltaY > 0 ? (int)Math.Ceiling(deltaY * percentDistance) : (int)Math.Floor(deltaY * percentDistance);
            var nextX = mousePosition.X + deltaX;
            var nextY = mousePosition.Y + deltaY;
            var nextPosition = new Point(nextX, nextY);
            return nextPosition;
        }

        protected override void PerformHoverActionSpecificBundlePage(int x, int y)
        {
            base.PerformHoverActionSpecificBundlePage(x, y);
            TryStickToSomething(x, y);
        }

        private void TryStickToSomething(int x, int y)
        {
            if (CurrentPageBundle.name != MemeBundleNames.VERY_STICKY)
            {
                return;
            }

            var texturePosition = new Vector2(xPositionOnScreen + 872, yPositionOnScreen + 88);
            var textureRectangle = new Rectangle((int)texturePosition.X, (int)texturePosition.Y, 128, 128);
            if (textureRectangle.Contains(x, y))
            {
                _isCurrentlySticky = true;
                _stickyPosition = textureRectangle.Center;
                return;
            }

            for (var i = GetIngredientsStartIndex(); i < GetIngredientsEndIndex(); i++)
            {
                var ingredient = IngredientList[i];
                if (ingredient.bounds.Contains(x, y))
                {
                    _isCurrentlySticky = true;
                    _stickyPosition = ingredient.bounds.Center;
                    return;
                }
            }

            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.bounds.Contains(x, y) && ingredientSlot.item != null)
                {
                    _isCurrentlySticky = true;
                    _stickyPosition = ingredientSlot.bounds.Center;
                    return;
                }
            }
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
                AfterUpdateIngredientSlotsBundleBundle();
                UpdateIngredientSlotsBundleBundle();
                return;
            }
            BeforeUpdateIngredientSlotsSpecialBundles();
            base.UpdateIngredientSlots();
            AfterUpdateIngredientSlotsSpecialBundles();
        }

        private void BeforeUpdateIngredientSlotsSpecialBundles()
        {
        }

        private void AfterUpdateIngredientSlotsSpecialBundles()
        {
            AfterUpdateIngredientSlotsSisyphus();
            AfterUpdateIngredientSlotsBureaucracy();
            AfterUpdateIngredientSlotsSchrodinger();
            AfterUpdateIngredientSlotsIKEA();
            AfterUpdateIngredientSlotsSquareHole();
        }

        private void AfterUpdateIngredientSlotsIKEA()
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.IKEA)
            {
                return;
            }

            if (IngredientList.Last().item.QualifiedItemId == IkeaItemQualifiedId)
            {
                return;
            }

            var ingredientRectangles = GenerateIngredientRectangles(1);
            CreateIngredientComponent(IkeaItemQualifiedId, 1, 0, ingredientRectangles);

            for (var i = 0; i < IngredientList.Count - 1; i++)
            {
                IngredientList[i].visible = false;
                IngredientList[i].bounds.X *= 100;
                IngredientList[i].bounds.Y *= 100;
            }
        }

        private void AfterUpdateIngredientSlotsSquareHole()
        {
            if (CurrentPageBundle == null || CurrentPageBundle.name != MemeBundleNames.SQUARE_HOLE)
            {
                return;
            }

            for (var i = 0; i < IngredientSlots.Count; i++)
            {
                var sourceRect = GetSquareHoleSourceRect(i, false);
                IngredientSlots[i].texture = MemeTexture;
                IngredientSlots[i].sourceRect.X = sourceRect.X;
                IngredientSlots[i].sourceRect.Y = sourceRect.Y;
            }
        }

        private Point GetSquareHoleSourceRect(int index, bool highlight)
        {
            const int size = 18;
            var x = 0 + (highlight ? 126 : 0);
            var y = 122 + (index * size);
            return new Point(x, y);
        }

        private void AfterUpdateIngredientSlotsSisyphus()
        {
            if (CurrentPageBundle?.name != MemeBundleNames.SISYPHUS)
            {
                return;
            }
            for (var slotIndex = 0; slotIndex < IngredientSlots.Count; ++slotIndex)
            {
                if (IngredientSlots[slotIndex].item != null)
                {
                    continue;
                }

                SisyphusIndex = slotIndex;
                var slotToFocus = SisyphusIndex;
                if (SisyphusStoneNeedsToFall)
                {
                    slotToFocus--;
                }
                FocusOnOneIngredientSlot(slotToFocus);
                return;
            }
        }

        private void AfterUpdateIngredientSlotsBureaucracy()
        {
            if (CurrentPageBundle?.name != MemeBundleNames.PERMIT_A38)
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
            var ingredientSlotRectangles = GenerateIngredientSlotsRectangles(ingredientsSlotsPerSubBundle);

            for (var indexBundle = 0; indexBundle < NUMBER_SUB_BUNDLES; indexBundle++)
            {
                for (var indexIngredient = 0; indexIngredient < ingredientsPerSubBundle; indexIngredient++)
                {
                    var ingredientIndex = indexBundle * ingredientsPerSubBundle + indexIngredient;
                    IngredientList[ingredientIndex].bounds = ingredientRectangles[indexIngredient];
                }
                for (var indexIngredientSlot = 0; indexIngredientSlot < ingredientsSlotsPerSubBundle; indexIngredientSlot++)
                {
                    var ingredientSlotIndex = indexBundle * ingredientsSlotsPerSubBundle + indexIngredientSlot;
                    IngredientSlots[ingredientSlotIndex].bounds = ingredientSlotRectangles[indexIngredientSlot];
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
            if (CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                if (ReceiveLeftClickInBundleBundle(x, y))
                {
                    return true;
                }
            }

            if (FromGameMenu)
            {
                return base.ReceiveLeftClickInSpecificBundlePage(x, y);
            }

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
            if (CurrentPageBundle.name == MemeBundleNames.PUZZLE)
            {
                if (_slidingPuzzle.ReceiveLeftClick(x - xPositionOnScreen, y - yPositionOnScreen))
                {
                    if (_slidingPuzzle.IsPuzzleSolved())
                    {
                        PerformCurrencyPurchase();
                    }
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
                var startIndex = NUMBER_SUB_BUNDLES * ingredientsSlotsPerSubBundle;
                var endIndex = startIndex + NUMBER_SUB_BUNDLES;
                for (var i = startIndex; i < endIndex; i++)
                {
                    if (IngredientSlots[i].containsPoint(x, y))
                    {
                        _bundleBundleIndex = i - startIndex;
                        Game1.playSound("shwip");
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

        internal override bool CheckIfAllIngredientsAreDeposited()
        {
            if (CurrentPageBundle.name == MemeBundleNames.REVERSE || CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                return CheckIfAllIngredientsAreTakenOut();
            }
            if (CurrentPageBundle.name == MemeBundleNames.PERMIT_A38)
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

        protected override void DrawIngredientAndShadow(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, bool drawShadow, ClickableTextureComponent ingredientBox, float overlayTransparency)
        {
            if (CurrentPageBundle.name == MemeBundleNames.OFF_YOUR_BACK)
            {
                if (ingredientBox.item == null || !ingredientBox.visible)
                {
                    return;
                }
                DrawWornHat(spriteBatch, ingredient, ingredientBox, overlayTransparency, drawShadow);
                DrawWornBoots(spriteBatch, ingredient, ingredientBox, overlayTransparency, drawShadow);
                DrawWornPants(spriteBatch, ingredient, ingredientBox, overlayTransparency, drawShadow);
                DrawWornShirt(spriteBatch, ingredient, ingredientBox, overlayTransparency, drawShadow);
                DrawWornLeftRing(spriteBatch, ingredient, ingredientBox, overlayTransparency, drawShadow);
                DrawWornRightRing(spriteBatch, ingredient, ingredientBox, overlayTransparency, drawShadow);
                return;
            }

            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                return;
            }

            base.DrawIngredientAndShadow(spriteBatch, ingredient, drawShadow, ingredientBox, overlayTransparency);
        }

        private void DrawWornHat(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, bool drawShadow)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_HAT, "Used Hat",
                Game1.player.hat.Value, 42,
                x => ItemRegistry.Create(x) is Hat, drawShadow);
        }

        private void DrawWornBoots(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, bool drawShadow)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_BOOTS, "Used Boots",
                Game1.player.boots.Value, 40,
                x => ItemRegistry.Create(x) is Boots, drawShadow);
        }

        private void DrawWornPants(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, bool drawShadow)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_PANTS, "Used Pants",
                Game1.player.pantsItem.Value, 68,
                x => ItemRegistry.Create(x) is Clothing clothing && clothing.clothesType.Value == Clothing.ClothesType.PANTS, drawShadow);
        }

        private void DrawWornShirt(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, bool drawShadow)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_SHIRT, "Used Shirt",
                Game1.player.shirtItem.Value, 69,
                x => ItemRegistry.Create(x) is Clothing clothing && clothing.clothesType.Value == Clothing.ClothesType.SHIRT, drawShadow);
        }

        private void DrawWornLeftRing(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, bool drawShadow)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_LEFT_RING, "Used Left Ring",
                Game1.player.leftRing.Value, 41,
                x => ItemRegistry.Create(x) is Ring, drawShadow); 
        }

        private void DrawWornRightRing(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, bool drawShadow)
        {
            DrawWornItem(spriteBatch, ingredient, ingredientBox, overlayTransparency, MemeIDProvider.WORN_RIGHT_RING, "Used Right Ring",
                Game1.player.rightRing.Value, 41,
                x => ItemRegistry.Create(x) is Ring, drawShadow, true);
        }

        private void DrawWornItem(SpriteBatch spriteBatch, BundleIngredientDescription ingredient, ClickableTextureComponent ingredientBox, float overlayTransparency, string wornItemId, string hoverText, Item wornItem,
            int emptySlotTilePosition, Func<string, bool> IsItemValid, bool drawShadow, bool last = false)
        {
            if (ingredient.id != wornItemId)
            {
                return;
            }

            ingredientBox.hoverText = hoverText;
            if (ingredient.completed)
            {
                var donatedItemId = _state.QualifiedIdsClothesDonated.First(IsItemValid);
                if (last)
                {
                    donatedItemId = _state.QualifiedIdsClothesDonated.Last(IsItemValid);
                }
                var donatedItem = ItemRegistry.Create(donatedItemId);
                DrawIngredientInMenu(donatedItem, spriteBatch, ingredientBox, (drawShadow ? overlayTransparency : 0.25f));
                return;
            }
            if (wornItem == null)
            {
                var heldValid = HeldItem != null && IsItemValid(HeldItem.QualifiedItemId);
                if (heldValid)
                {
                    if (drawShadow)
                    {
                        DrawIngredientShadow(spriteBatch, ingredientBox, overlayTransparency);
                    }
                    DrawIngredientInMenu(HeldItem, spriteBatch, ingredientBox, (drawShadow ? overlayTransparency : 0.25f));
                }
                else
                {
                    spriteBatch.Draw(Game1.menuTexture, ingredientBox.bounds, Game1.getSourceRectForStandardTileSheet(Game1.menuTexture, emptySlotTilePosition), Color.White);
                }
            }
            else
            {
                if (drawShadow)
                {
                    DrawIngredientShadow(spriteBatch, ingredientBox, overlayTransparency);
                }
                DrawIngredientInMenu(wornItem, spriteBatch, ingredientBox, (drawShadow ? overlayTransparency : 0.25f));
            }
        }

        private void DrawIngredientInMenu(Item item, SpriteBatch spriteBatch, ClickableTextureComponent ingredientBox, float transparency = 1f)
        {
            item.drawInMenu(spriteBatch, new Vector2(ingredientBox.bounds.X, ingredientBox.bounds.Y), ingredientBox.scale / 4f, 1f, 0.9f, StackDrawType.Draw, Color.White * transparency, false);
        }

        protected override void PickItemFromInventory(int x, int y)
        {
            if (CurrentPageBundle.name == MemeBundleNames.OFF_YOUR_BACK)
            {
                HeldItem = _clothesMenu.leftClick(x, y, HeldItem);
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.SQUARE_HOLE && HeldItem == null)
            {
                if (!CurrentPageBundle.Complete && CurrentPageBundle.CompletionTimer <= 0)
                {
                    HeldItem = Inventory.leftClick(x, y, HeldItem);
                    if (HeldItem != null)
                    {
                        PlaySquareHoleTakeItemSound();
                    }
                }
                return;
            }
            base.PickItemFromInventory(x, y);
        }

        protected override void PickItemFromInventoryRightClick(int x, int y)
        {
            if (CurrentPageBundle.name == MemeBundleNames.SQUARE_HOLE && HeldItem == null)
            {
                HeldItem = Inventory.rightClick(x, y, HeldItem);
                if (HeldItem != null)
                {
                    PlaySquareHoleTakeItemSound();
                }
                return;
            }
            base.PickItemFromInventoryRightClick(x, y);
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
                b.Draw(CurrentPageBundle.BundleTextureOverride, new Vector2(0, 0), new Microsoft.Xna.Framework.Rectangle(Game1.viewport.Width, 0, Game1.viewport.Width, Game1.viewport.Height), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.1f);
            }

            if (_isCurrentlySticky)
            {
                var sap = ItemRegistry.GetDataOrErrorItem(QualifiedItemIds.SAP);
                var seed = Game1.ticks / 10;
                var random = new Random(seed);
                if (random.NextDouble() < 0.01)
                {
                    int a = 5;
                }
                var maxDistance = 32;
                var mouseOffset = 32;
                for (var i = 0.0; i <= 1; i += 0.02)
                {
                    var nextPosition = GetPointTowardsSticky(i);
                    var x = (int)Math.Round((random.NextDouble() * maxDistance) - (maxDistance / 2));
                    var y = (int)Math.Round((random.NextDouble() * maxDistance) - (maxDistance / 2));
                    var position = new Vector2(nextPosition.X + x - mouseOffset, nextPosition.Y + y - mouseOffset);
                    var sourceRect = sap.GetSourceRect(0, sap.SpriteIndex);
                    b.Draw(sap.GetTexture(), position + new Vector2(32f, 32f), sourceRect, Color.White * 1f, 0.0f, new Vector2((float)(sourceRect.Width / 2), (float)(sourceRect.Height / 2)), 4f, SpriteEffects.None, 0.9f);
                    // sap.drawInMenu(b, position, 1.0f);
                }
            }
        }

        public static void OnUpdateTickedStatic(UpdateTickedEventArgs e)
        {
            if (Game1.activeClickableMenu is ArchipelagoJunimoNoteMenu junimoNoteMenu)
            {
                junimoNoteMenu.OnUpdateTicked();
                junimoNoteMenu._currencyManager.OnUpdateTicked(e);
            }
        }

        public void OnUpdateTicked()
        {
            if (!SpecificBundlePage || CurrentPageBundle == null)
            {
                return;
            }

            if (CurrentPageBundle.name == MemeBundleNames.NFT && DidPlayerJustScreenshot())
            {
                PerformCurrencyPurchase();
                return;
            }
            if (CurrentPageBundle.name == MemeBundleNames.ASMR)
            {
                if (_currentCue == null)
                {
                    return;
                }

                if (_currentCue.IsPlaying)
                {
                    return;
                }

                PlayNextASMR();
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
            var hasPressedCtrl = pressedKeys.Contains(Keys.LeftControl) || pressedKeys.Contains(Keys.RightControl);
            var hasPressedCopy = hasPressedCtrl && pressedKeys.Contains(Keys.C);
            var hasPressedSave = hasPressedCtrl && pressedKeys.Contains(Keys.S);
            return hasPressedScreenshotKey || hasSummonedSnippingTool || hasPressedCopy || hasPressedSave;
        }

        protected override ClickableTextureComponent CreateIngredientButton(ParsedItemData dataOrErrorItem, Rectangle bounds, int index, string hoverText, Item flavoredItem)
        {
            if (CurrentPageBundle.name == MemeBundleNames.TRAP)
            {
                var offsetY = INGREDIENT_SLOTS_CENTER_Y - INGREDIENTS_CENTER_Y;
                bounds.Y += offsetY;
            }
            if (CurrentPageBundle.name == MemeBundleNames.EMMALUTION)
            {
                if (hoverText == "Rain Totem")
                {
                    hoverText = "Brain Totem";
                }
            }
            if (CurrentPageBundle.name == MemeBundleNames.LOSER_CLUB)
            {
                if (hoverText == "Tuna")
                {
                    hoverText = "Trash Tuna";
                }
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
            if (CurrentPageBundle.name == MemeBundleNames.SQUARE_HOLE)
            {
                DrawIngredientSlotSquareHole(b, index);
                return;
            }

            base.DrawIngredientSlot(b, index);
        }

        private void DrawIngredientSlotSquareHole(SpriteBatch spriteBatch, int index)
        {
            var alpha = 1f;
            var ingredientSlot = IngredientSlots[index];
            if (PartialDonationItem != null && ingredientSlot.item != PartialDonationItem)
            {
                alpha = 0.25f;
            }
            if (ingredientSlot.item == null || index != 0 || PartialDonationItem != null && ingredientSlot.item == PartialDonationItem)
            {
                ingredientSlot.draw(spriteBatch, (FromGameMenu ? Color.LightGray * 0.5f : Color.White) * alpha, 0.89f);
            }
            if (index != 0)
            {
                return;
            }
            var firstSlot = IngredientSlots[0];
            var i = 0;
            foreach (var slotWithItem in IngredientSlots.Where(x => x.item != null))
            {
                var offsetX = ((i % 3 - 1) * 12) + 4;
                var offsetY = (((double)(i / 3) - 0.5) * 12) + 4;
                slotWithItem.item.drawInMenu(spriteBatch, new Vector2((float)(firstSlot.bounds.X + offsetX), (float)(firstSlot.bounds.Y + offsetY)), firstSlot.scale / 8f, alpha, 0.9f);
                // slotWithItem.drawItem(spriteBatch, 4, 4, alpha);
                i++;
            }
        }

        protected override void DrawIngredients(SpriteBatch spriteBatch)
        {
            if (CurrentPageBundle.name == MemeBundleNames.IKEA)
            {
                var ingredientDescription = new BundleIngredientDescription(IkeaItemQualifiedId, 1, 0, false);
                DrawIngredientAndShadow(spriteBatch, ingredientDescription, IngredientList.Count - 1);
                return;
            }
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
                var startIndex = NUMBER_SUB_BUNDLES * ingredientsSlotsPerSubBundle;
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

        internal bool SubBundleComplete(int subBundleIndex)
        {
            var startIndex = subBundleIndex * ingredientsSlotsPerSubBundle;
            var endIndex = startIndex + ingredientsSlotsPerSubBundle;
            var num = 0;
            for (var index = startIndex; index < endIndex; index++)
            {
                var ingredientSlot = IngredientSlots[index];
                if (ingredientSlot.item != null)
                {
                    ++num;
                }
            }
            return num >= ingredientsSlotsPerSubBundle;
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
            if (CurrentPageBundle != null && CurrentPageBundle.name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                return GetIngredientsEndIndex(_bundleBundleIndex);
            }

            return base.GetIngredientsEndIndex();
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
                return NUMBER_SUB_BUNDLES * ingredientsSlotsPerSubBundle;
            }

            return bundleBundleIndex * ingredientsSlotsPerSubBundle;
        }

        private int GetIngredientSlotsEndIndex(int bundleBundleIndex)
        {

            if (bundleBundleIndex == -1)
            {
                return (NUMBER_SUB_BUNDLES + 1) * ingredientsSlotsPerSubBundle;
            }

            return (bundleBundleIndex + 1) * ingredientsSlotsPerSubBundle;
        }

        protected void UpdateIngredientSlotsBundleBundle()
        {
            for (var bundleIndex = 0; bundleIndex < NUMBER_SUB_BUNDLES; bundleIndex++)
            {
                var startIndex = GetIngredientsStartIndex(bundleIndex);
                var endIndex = GetIngredientsEndIndex(bundleIndex);
                var ingredientSlotIndex = GetIngredientSlotsStartIndex(bundleIndex);
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
            CurrentPageBundle.IngredientDepositAnimation(IngredientSlots[index], NOTE_TEXTURE_NAME, new Rectangle(530, 244, 18, 18), this, true);
            ++index;
            return index;
        }

        protected override bool SlotCanReceiveItem(ClickableTextureComponent slot)
        {
            if (CurrentPageBundle != null && CurrentPageBundle.name == MemeBundleNames.SQUARE_HOLE)
            {
                return slot.sourceRect.Y == 122;
            }

            return base.SlotCanReceiveItem(slot);
        }

        public static bool IsBundleRemaining(string bundleName)
        {
            var bundleIndex = GetBundleId(bundleName, out var communityCenter, out _);
            if (bundleIndex <= -1)
            {
                return false;
            }

            return IsBundleRemaining(bundleName, bundleIndex, communityCenter);
        }

        private static bool IsBundleRemaining(string bundleName, int bundleIndex, CommunityCenter communityCenter)
        {
            var bundleData = Game1.netWorldState.Value.BundleData;
            var bundleInfo = bundleData.First(x => x.Key.EndsWith($"{bundleIndex}")).Value;
            var bundlesDict = communityCenter.bundlesDict();
            var bundleState = bundlesDict[bundleIndex];
            var bundleInfoArray = bundleInfo.Split('/');
            //var bundleIngredientsArray = ArgUtility.SplitBySpace(bundleInfoArray[2]);
            //var bundle = new ArchipelagoBundle(bundleIndex, bundleInfo, bundleState, Point.Zero, NOTE_TEXTURE_NAME, null);
            var isComplete = BundleRemake.IsBundleComplete(bundleState, bundleInfoArray);
            if (isComplete && _locationChecker.IsLocationMissing($"{bundleName} Bundle"))
            {
                _locationChecker.AddCheckedLocation($"{bundleName} Bundle");
            }
            return !isComplete;
        }

        public static int TryDonateToBundle(string bundleName, string itemName, int itemAmount)
        {
            var bundleIndex = GetBundleId(bundleName, out var communityCenter, out var area);
            if (bundleIndex <= -1)
            {
                return 0;
            }

            var desiredBundleKey = "";
            var desiredBundleData = "";
            foreach (var (bundleKey, bundleData) in Game1.netWorldState.Value.BundleData)
            {
                var name = bundleData.Split("/").First();
                if (name == bundleName)
                {
                    desiredBundleKey = bundleKey;
                    desiredBundleData = bundleData;
                    break;
                }
            }

            var bundleParts = desiredBundleData.Split("/");
            var ingredientsData = bundleParts[2];
            var ingredientFields = ingredientsData.Split(" ");
            var requiredDonations = string.IsNullOrWhiteSpace(bundleParts[4]) ? ingredientFields.Length / 3 : int.Parse(bundleParts[4]);
            for (var i = 0; i < ingredientFields.Length; i += 3)
            {
                if (communityCenter.bundles.FieldDict[bundleIndex][i/3])
                {
                    continue;
                }

                var id = ingredientFields[i];
                var amount = int.Parse(ingredientFields[i + 1]);
                var requiredItem = ItemRegistry.Create(id);
                if (itemName == requiredItem.Name && itemAmount >= amount)
                {
                    if (communityCenter.bundles.FieldDict[bundleIndex].Count(x => x) >= requiredDonations-1)
                    {
                        CompleteBundleIfExists(bundleName);
                        return amount;
                    }
                    communityCenter.bundles.FieldDict[bundleIndex][i / 3] = true;
                    return amount;
                }
            }

            return 0;
        }

        public static void CompleteBundleIfExists(string bundleName)
        {
            var bundleIndex = GetBundleId(bundleName, out var communityCenter, out var area);
            if (bundleIndex <= -1)
            {
                return;
            }

            if (IsBundleRemaining(bundleName, bundleIndex, communityCenter))
            {
                CompleteBundleInMenu(bundleIndex, area);
            }

            var bundleReader = new BundleReader();
            bundleReader.CheckAllBundleLocations(_locationChecker);
        }

        private static int GetBundleId(string bundleName)
        {
            return GetBundleId(bundleName, out _, out _);
        }

        public static int GetBundleId(string bundleName, out CommunityCenter communityCenter, out int area)
        {
            communityCenter = (CommunityCenter)Game1.getLocationFromName("CommunityCenter");
            area = 0;
            foreach (var (bundleKey, bundleData) in Game1.netWorldState.Value.BundleData)
            {
                var name = bundleData.Split("/").First();
                if (name.Equals(bundleName) || name.Equals($"{bundleName} Bundle") || (bundleName.EndsWith(" Bundle") && name.Equals(bundleName[..^" Bundle".Length])))
                {
                    var keyFields = bundleKey.Split("/");
                    area = CommunityCenter.getAreaNumberFromName(keyFields.First());
                    return int.Parse(keyFields.Last());
                }
            }
            return -1;
        }

        public static void OnDayStarted(GiftReceiver giftReceiver)
        {
            DayStopwatch.Reset();
            DayStopwatch.Start();
            FloorIsLavaHasTouchedGroundToday = 0;
            HasLookedAtRestraintBundleToday = false;
            HasPurchasedRestraintBundleToday = false;
            HasLookedAtHibernationBundleToday = false;
            HasLookedAtStanleyBundleToday = false;
            _slidingPuzzle = null;

            SendMinistryOfMadnessGift(giftReceiver);
        }

        private static void SendMinistryOfMadnessGift(GiftReceiver giftReceiver)
        {
            if (!IsBundleRemaining(MemeBundleNames.MINISTRY_OF_MADNESS) || !_archipelago.HasReceivedItem(APItem.FOREST_MAGIC))
            {
                return;
            }

            var daySaveRandom = Utility.CreateDaySaveRandom();
            if (!(daySaveRandom.NextDouble() < 0.1))
            {
                return;
            }
            var potentialObjects = new[] { ObjectIds.TRASH, ObjectIds.JOJA_COLA, ObjectIds.BROKEN_GLASSES, ObjectIds.BROKEN_CD, ObjectIds.SOGGY_NEWSPAPER };
            var chosenTrash = potentialObjects[daySaveRandom.Next(potentialObjects.Length)];
            giftReceiver.ReceiveFakeGift(chosenTrash, daySaveRandom.Next(0, 100), MemeBundleNames.MINISTRY_OF_MADNESS);
        }

        public static void OnDayEnded()
        {
            if (HasLookedAtRestraintBundleToday && !HasPurchasedRestraintBundleToday)
            {
                CompleteBundleIfExists(MemeBundleNames.RESTRAINT);
            }
            if (HasLookedAtStanleyBundleToday)
            {
                _state.LastDayLookedAtStanleyBundle = (int)Game1.stats.DaysPlayed;
            }
            else if (_state != null && _state.LastDayLookedAtStanleyBundle > -1)
            {
                var daysSinceLastLooked = Game1.stats.DaysPlayed - _state.LastDayLookedAtStanleyBundle;
                if (daysSinceLastLooked >= (112 * 5))
                {
                    CompleteBundleIfExists(MemeBundleNames.STANLEY);
                }
            }
        }

        public static void HasBeenHibernatingFor(int numberOfDaysSlept)
        {
            if (!IsBundleRemaining(MemeBundleNames.HIBERNATION))
            {
                return;
            }
            
            foreach (var (bundleKey, bundleData) in Game1.netWorldState.Value.BundleData)
            {
                var name = bundleData.Split("/").First();
                if (name != MemeBundleNames.HIBERNATION)
                {
                    continue;
                }

                var ingredients = bundleData.Split("/")[2];
                var parts = ingredients.Split(" ");
                var price = int.Parse(parts[1]);
                if (numberOfDaysSlept >= price)
                {
                    CompleteBundleIfExists(MemeBundleNames.HIBERNATION);
                }
                return;
            }
        }

        protected override void DrawBundleTexture(SpriteBatch b)
        {
            if (CurrentPageBundle.name == MemeBundleNames.PUZZLE)
            {
                _slidingPuzzle.DrawPuzzle(b, xPositionOnScreen, yPositionOnScreen);
                return;
            }
            base.DrawBundleTexture(b);
        }

        protected override void DrawBundleLabel(SpriteBatch b)
        {
            if (CurrentPageBundle.name == MemeBundleNames.PUZZLE)
            {
                if (CurrentPageBundle.label == null)
                {
                    return;
                }
                DrawBundleLabel(b, xPositionOnScreen + 936, yPositionOnScreen + 120 + SlidingPuzzleHandler.IMAGE_SIZE, 8);
                return;
            }
            base.DrawBundleLabel(b);
        }

        protected override void SetUpBundleSpecificPage(ArchipelagoBundle bundle)
        {
            base.SetUpBundleSpecificPage(bundle);
            if (bundle.name == MemeBundleNames.PUZZLE)
            {
                if (_slidingPuzzle == null)
                {
                    _slidingPuzzle = new SlidingPuzzleHandler(_modHelper, MemeTexture, GetSlidingPuzzleSize(bundle));
                }
            }
            if (bundle.name == MemeBundleNames.ASMR && !FromGameMenu)
            {
                StartPlayingASMR();
            }
            if (bundle.name == MemeBundleNames.SQUARE_HOLE)
            {
                RegisterSquareHoleCues();
            }
        }

        private void StartPlayingASMR()
        {
            RegisterASMRCues();
            PlayNextASMR();
        }

        private void PlayNextASMR()
        {
            var nextIndex = 0;
            if (_currentCue != null)
            {
                var currentName = _currentCue.Name;
                var currentIndex = Array.IndexOf(_asmrCues, currentName);
                nextIndex = currentIndex + 1;
            }
            if (nextIndex < _asmrCues.Length)
            {
                Game1.playSound(_asmrCues[nextIndex], out var cue);
                _currentCue = cue;
            }
            else
            {
                _currentCue.Stop(AudioStopOptions.Immediate);
                _currentCue.Dispose();
                _currentCue = null;
                if (CurrentPageBundle != null && CurrentPageBundle.name == MemeBundleNames.ASMR)
                {
                    PerformCurrencyPurchase();
                }
            }
        }

        private void RegisterASMRCues()
        {
            if (_asmrCues != null && _asmrCues.Any())
            {
                return;
            }

            var currentModFolder = _modHelper.DirectoryPath;
            var soundsFolder = "Sounds";
            var asmrFolder = "ASMR";
            var relativePathToAsmrSounds = Path.Combine(currentModFolder, soundsFolder, asmrFolder);
            var files = Directory.EnumerateFiles(relativePathToAsmrSounds, "*.wav", SearchOption.TopDirectoryOnly);
            var cues = new List<string>();
            foreach (var file in files)
            {
                var soundName = new FileInfo(file).Name;
                var cueDefinition = new CueDefinition(soundName, SoundEffect.FromFile(file), 0);
                Game1.soundBank.AddCue(cueDefinition);
                cues.Add(soundName);
            }

            var random = Utility.CreateDaySaveRandom();
            _asmrCues = cues.OrderBy(x => x.Contains("stardrop_asmr") ? 999 : random.NextDouble()).ToArray();
        }

        private int GetSlidingPuzzleSize(ArchipelagoBundle bundle)
        {
            var stack = bundle.Ingredients.First().stack;
            return stack switch
            {
                <= 1 => 2,
                < 10 => 3,
                10 => 4,
                < 40 => 5,
                >= 40 => 6,
            };

        }

        protected override void CompleteBundleInMenu()
        {
            if (CurrentPageBundle.name == MemeBundleNames.MERMAID)
            {
                Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite()
                {
                    interval = 1f,
                    delayBeforeAnimationStart = 2425 + 400,
                    endFunction = (x) => base.CompleteBundleInMenu(),
                });
                return;
            }

            base.CompleteBundleInMenu();
        }

        protected override void PerformHoverIngredientSlots(int x, int y)
        {
            if (CurrentPageBundle != null && CurrentPageBundle.name == MemeBundleNames.SQUARE_HOLE)
            {
                PerformHoverIngredientSlotsSquareHole(x, y);
                return;
            }
            base.PerformHoverIngredientSlots(x, y);
        }

        private void PerformHoverIngredientSlotsSquareHole(int x, int y)
        {
            if (HeldItem == null)
            {
                return;
            }

            foreach (var ingredientSlot in IngredientSlots)
            {
                if (ingredientSlot.bounds.Contains(x, y) && CanBePartiallyOrFullyDonated(HeldItem) && (PartialDonationItem == null || ingredientSlot.item == PartialDonationItem))
                {
                    ingredientSlot.sourceRect.X = 126;
                }
                else
                {
                    ingredientSlot.sourceRect.X = 0;
                }
            }
        }

        private const string _failDepositKey = "sh-deposit-sound";
        private const string _takeItemKey = "sh-take-item";
        private const string _squareHoleKey = "sh-square-hole";

        private void RegisterSquareHoleCues()
        {
            if (_squareHoleCues != null && _squareHoleCues.Any())
            {
                return;
            }

            var currentModFolder = _modHelper.DirectoryPath;
            var soundsFolder = "Sounds";
            var squareHoleFolder = "Square Hole";
            var relativePathToSquareHoleSounds = Path.Combine(currentModFolder, soundsFolder, squareHoleFolder);
            var files = Directory.EnumerateFiles(relativePathToSquareHoleSounds, "*.wav", SearchOption.TopDirectoryOnly);
            var cues = new Dictionary<string, List<string>>()
            {
                { _failDepositKey, new List<string>() },
                { _takeItemKey, new List<string>() },
                { _squareHoleKey, new List<string>() },
            };
            foreach (var file in files)
            {
                var soundName = new FileInfo(file).Name;
                var cueDefinition = new CueDefinition(soundName, SoundEffect.FromFile(file), 0);
                Game1.soundBank.AddCue(cueDefinition);
                if (soundName.StartsWith("deposit_sound"))
                {
                    cues[_failDepositKey].Add(soundName);
                }
                else if (soundName.StartsWith("goes_in_square_hole_"))
                {
                    cues[_squareHoleKey].Add(soundName);
                }
                else
                {
                    cues[_takeItemKey].Add(soundName);
                }
            }

            var random = Utility.CreateDaySaveRandom();
            _squareHoleCues = cues.ToDictionary(x => x.Key, x => x.Value.OrderBy(_ => random.NextDouble()).ToArray());
        }

        private void PlaySquareHoleTakeItemSound()
        {
            RegisterSquareHoleCues();
            var cues = _squareHoleCues[_takeItemKey];
            var randomIndex = Game1.random.Next(0, cues.Length);
            Game1.playSound(cues[randomIndex]);
        }

        public void PlaySquareHoleFailDepositSound()
        {
            RegisterSquareHoleCues();
            var cues = _squareHoleCues[_failDepositKey];
            var randomIndex = Game1.random.Next(0, cues.Length);
            Game1.playSound(cues[randomIndex]);
        }

        public void PlaySquareHoleSuccessSound()
        {
            RegisterSquareHoleCues();
            var cues = _squareHoleCues[_squareHoleKey];
            var randomIndex = Game1.random.Next(0, cues.Length);
            Game1.playSound(cues[randomIndex]);
        }
    }
}
