using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Bundles;
using StardewArchipelago.Constants;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Serialization;
using StardewValley.Characters;
using KaitoKid.ArchipelagoUtilities.Net;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class RaccoonJunimoNoteMenu : ArchipelagoJunimoNoteMenu
    {
        private const string RACCOON_REQUEST_PREFIX = "Raccoon Request ";

        private readonly Raccoon _raccoon;
        private readonly StardewLocationChecker _locationChecker;
        private readonly BundlesManager _bundlesManager;
        private readonly ArchipelagoStateDto _state;
        private int _currentBundleNumber = -1;

        public RaccoonJunimoNoteMenu(int bundleNumber, Raccoon raccoon, StardewLocationChecker _locationChecker, BundlesManager bundlesManager, ArchipelagoStateDto state) : base("LooseSprites\\raccoon_bundle_menu")
        {
            _raccoon = raccoon;
            this._locationChecker = _locationChecker;
            _bundlesManager = bundlesManager;
            _state = state;

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
                _state.CurrentRaccoonBundleStatus[bundleNumber].Clear();
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
            OnBundleComplete = _ => BundleComplete(_raccoon);
            OnScreenSwipeFinished = _ => BundleCompleteAfterSwipe(_raccoon);

            SetArrowsVisibility();

            _currentBundleNumber = bundleNumber;
            return bundle;
        }
        private void SetArrowsVisibility()
        {

        }

        public override void SwapPage(int direction)
        {
            base.SwapPage(direction);
        }

        private static List<string> GetRaccoonLocationsInSlot(StardewLocationChecker locationChecker)
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

        private static List<int> GetAvailableMissingRaccoonNumbers(StardewArchipelagoClient archipelago, StardewLocationChecker locationChecker)
        {
            var raccoonNumbers = GetAvailableRaccoonNumbers(archipelago, locationChecker);
            var missingNumbers = raccoonNumbers.Where(x => locationChecker.IsLocationMissing($"{RACCOON_REQUEST_PREFIX}{x}")).ToList();
            return missingNumbers;
        }
    }
}
