using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Characters;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class RaccoonJunimoNoteMenu : ArchipelagoJunimoNoteMenu
    {
        private const string RACCOON_REQUEST_PREFIX = "Raccoon Request ";

        private readonly Raccoon _raccoon;
        private readonly StardewArchipelagoClient _archipelago;
        private readonly StardewLocationChecker _locationChecker;
        private readonly BundlesManager _bundlesManager;
        private readonly ArchipelagoStateDto _state;
        private int _currentBundleNumber = -1;

        public RaccoonJunimoNoteMenu(int bundleNumber, Raccoon raccoon, StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker, BundlesManager bundlesManager, ArchipelagoStateDto state) : base("LooseSprites\\raccoon_bundle_menu")
        {
            _raccoon = raccoon;
            _archipelago = archipelago;
            _locationChecker = locationChecker;
            _bundlesManager = bundlesManager;
            _state = state;

            InitializeArrows();
            var currentRaccoonBundle = InitializeBundle(bundleNumber);
            SetUpBundleSpecificPage(currentRaccoonBundle);

            behaviorBeforeCleanup = _ => _raccoon.mutex?.ReleaseLock();

            if (!Game1.options.SnappyMenus)
            {
                return;
            }
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        private ArchipelagoBundle InitializeBundle(int bundleNumber)
        {
            if (Game1.netWorldState.Value.SeasonOfCurrentRacconBundle != bundleNumber)
            {
                _state.CurrentRaccoonBundleStatus.TryAdd(bundleNumber, new List<bool>());
                // _state.CurrentRaccoonBundleStatus[bundleNumber].Clear();
                Game1.netWorldState.Value.SeasonOfCurrentRacconBundle = bundleNumber;
            }

            var ingredients = new List<BundleIngredientDescription>();
            var raccoonRequestsRoom = _bundlesManager.BundleRooms.Rooms[APName.RACCOON_REQUESTS_ROOM];
            var currentRaccoonBundleName = $"{APName.RACCOON_REQUEST_PREFIX}{bundleNumber}";
            var raccoonBundle = (ItemBundle)raccoonRequestsRoom.Bundles[currentRaccoonBundleName];
            var currentBundleStatus = _state.CurrentRaccoonBundleStatus[bundleNumber];
            for (var i = 0; i < raccoonBundle.Items.Count; i++)
            {
                if (currentBundleStatus.Count <= i)
                {
                    currentBundleStatus.Add(false);
                }
                if (raccoonBundle.Items[i] is null)
                {
                    throw new ArgumentException($"The raccoon must only have item bundles");
                }
                var bundleIngredient = raccoonBundle.Items[i].CreateBundleIngredientDescription(currentBundleStatus[i]);
                ingredients.Add(bundleIngredient);
            }

            var whichBundle = (bundleNumber - 1) % 5;
            var bundle = new ArchipelagoBundle(currentRaccoonBundleName, null, ingredients, new bool[1])
            {
                BundleTextureOverride = Game1.content.Load<Texture2D>("LooseSprites\\BundleSprites"),
                BundleTextureIndexOverride = 14 + whichBundle,
                NumberOfIngredientSlots = raccoonBundle.NumberRequired,
            };

            OnIngredientDeposit = x => currentBundleStatus[x] = true;

            SetArrowsVisibility();

            _currentBundleNumber = bundleNumber;
            return bundle;
        }

        private void InitializeArrows()
        {
            var textureComponent1 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + width - 128, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(365, 495, 12, 11), 4f);
            textureComponent1.visible = false;
            textureComponent1.myID = REGION_AREA_NEXT_BUTTON;
            textureComponent1.leftNeighborID = REGION_AREA_BACK_BUTTON;
            textureComponent1.leftNeighborImmutable = true;
            textureComponent1.downNeighborID = -99998;
            AreaNextButton = textureComponent1;
            var textureComponent2 = new ClickableTextureComponent(new Rectangle(xPositionOnScreen + 64, yPositionOnScreen, 48, 44), Game1.mouseCursors, new Rectangle(352, 495, 12, 11), 4f);
            textureComponent2.visible = false;
            textureComponent2.myID = REGION_AREA_BACK_BUTTON;
            textureComponent2.rightNeighborID = REGION_AREA_NEXT_BUTTON;
            textureComponent2.rightNeighborImmutable = true;
            textureComponent2.downNeighborID = -99998;
            AreaBackButton = textureComponent2;
        }

        private void SetArrowsVisibility()
        {
            var visible = GetAvailableMissingRaccoonNumbers(_archipelago, _locationChecker).Count > 1;
            AreaNextButton.visible = visible;
            AreaBackButton.visible = visible;
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (AreaNextButton.containsPoint(x, y))
            {
                SwapPage(1);
                return;
            }
            if (AreaBackButton.containsPoint(x, y))
            {
                SwapPage(-1);
                return;
            }
            base.receiveLeftClick(x, y, playSound);
        }

        public override void SwapPage(int direction)
        {
            var bundleNumbers = GetAvailableMissingRaccoonNumbers(_archipelago, _locationChecker);
            var currentIndex = bundleNumbers.IndexOf(_currentBundleNumber);
            var newIndex = (currentIndex + direction + bundleNumbers.Count) % bundleNumbers.Count;
            var newNumber = bundleNumbers[newIndex];

            var raccoonNoteMenu = new RaccoonJunimoNoteMenu(newNumber, _raccoon, _archipelago, _locationChecker, _bundlesManager, _state)
            {
                GameMenuTabToReturnTo = this.GameMenuTabToReturnTo,
                OnBundleComplete = this.OnBundleComplete,
                OnScreenSwipeFinished = this.OnScreenSwipeFinished,
            };
            Game1.activeClickableMenu = raccoonNoteMenu;
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);
            PerformHoverActionArrows(x, y);
        }

        protected override void PerformHoverActionArrows(int x, int y)
        {
            if (AreaNextButton.visible)
            {
                AreaNextButton.tryHover(x, y);
            }
            if (AreaBackButton.visible)
            {
                AreaBackButton.tryHover(x, y);
            }
        }

        protected override void DrawArrows(SpriteBatch b)
        {
            if (AreaNextButton.visible)
            {
                AreaNextButton.draw(b);
            }
            if (AreaBackButton.visible)
            {
                AreaBackButton.draw(b);
            }
        }

        public static List<string> GetRaccoonLocationsInSlot(StardewLocationChecker locationChecker)
        {
            var locations = locationChecker.GetAllLocationsStartingWith(RACCOON_REQUEST_PREFIX).ToList();
            return locations;
        }

        private static List<int> GetRaccoonNumbersInSlot(StardewLocationChecker locationChecker)
        {
            var locations = GetRaccoonLocationsInSlot(locationChecker);
            var numbers = locations.Select(x => int.Parse(x.Split(" ").Last())).ToList();
            return numbers;
        }

        private static List<int> GetAvailableRaccoonNumbers(StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            var raccoonNumbers = GetRaccoonNumbersInSlot(locationChecker);
            var receivedRaccoons = archipelago.GetReceivedItemCount(APItem.PROGRESSIVE_RACCOON);
            if (!archipelago.SlotData.QuestLocations.StoryQuestsEnabled)
            {
                receivedRaccoons += 1;
            }

            return raccoonNumbers.Where(x => x < receivedRaccoons).ToList();
        }

        public static List<int> GetAvailableMissingRaccoonNumbers(StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            var raccoonNumbers = GetAvailableRaccoonNumbers(archipelago, locationChecker);
            var missingNumbers = raccoonNumbers.Where(x => locationChecker.IsLocationMissing($"{RACCOON_REQUEST_PREFIX}{x}")).ToList();
            return missingNumbers;
        }
    }
}
