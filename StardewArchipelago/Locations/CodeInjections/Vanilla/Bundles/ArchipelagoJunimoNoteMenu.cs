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

        protected static ArchipelagoWrapperBundles _apWrapper;

        protected ArchipelagoJunimoNoteMenu(bool fromGameMenu, int area = 1, bool fromThisMenu = false) : base(fromGameMenu, area, fromThisMenu)
        {
        }

        protected ArchipelagoJunimoNoteMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete) : base(whichArea, bundlesComplete)
        {
        }

        protected ArchipelagoJunimoNoteMenu(BundleRemake bundle, string noteTexturePath) : base(bundle, noteTexturePath)
        {
        }

        public static void CreateWrapper(LogHandler logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker)
        {
            var bundleFactory = new BundleFactory(logger, modHelper, archipelago, state, locationChecker);
            var wrapper = new ArchipelagoWrapperBundles(logger, modHelper, archipelago, state, locationChecker, bundleFactory, new BundleReader());
            _apWrapper = wrapper;
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
                var menu = _apWrapper.Factory.CreateJunimoNoteMenu(this, FromGameMenu, WhichArea, FromThisMenu);
                menu.GameMenuTabToReturnTo = GameMenuTabToReturnTo;
                menu.MenuToReturnTo = MenuToReturnTo;
                return menu;
            }
            else
            {
                var menu = _apWrapper.Factory.CreateJunimoNoteMenu(this, WhichArea, Game1.RequireLocation<CommunityCenter>("CommunityCenter").bundlesDict());
                menu.GameMenuTabToReturnTo = GameMenuTabToReturnTo;
                menu.MenuToReturnTo = MenuToReturnTo;
                return menu;
            }
        }

        public override void SetUpMenu(int whichArea, Dictionary<int, bool[]> bundlesComplete)
        {
            base.SetUpMenu(whichArea, bundlesComplete);
            var remixedBundlesTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\BundleSprites");
            foreach (var bundle in this.Bundles)
            {
                var textureOverride = BundleIcons.GetBundleIcon(_apWrapper.Logger, _apWrapper.ModHelper, bundle.name, LogLevel.Trace);
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
                            var texture = BundleIcons.GetBundleIcon(_apWrapper.Logger, _apWrapper.ModHelper, moneyBundleName);
                            bundle.BundleTextureOverride = texture;
                            bundle.BundleTextureIndexOverride = 0;
                            if (texture == null)
                            {
                                _apWrapper.Logger.LogWarning($"Could not find a proper icon for money bundle '{moneyBundleName}', using default Archipelago Icon");
                            }
                            continue;
                        }
                    }

                    _apWrapper.Logger.LogWarning($"Could not find a proper icon for bundle '{bundle.name}', using default Archipelago Icon");
                    textureOverride = ArchipelagoTextures.GetArchipelagoLogo(_apWrapper.Logger, _apWrapper.ModHelper, 32, ArchipelagoTextures.COLOR);
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

            var scoutedItem = _apWrapper.Archipelago.ScoutStardewLocation(apLocationToScout, true);
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
        }

        protected override bool TryReceiveLeftClickInBundleArea(int x, int y)
        {
            return false;
        }

        private void CheckAllBundleLocations()
        {
            var completedBundleNames = _apWrapper.Reader.GetAllCompletedBundles();
            foreach (var completedBundleName in completedBundleNames)
            {
                _apWrapper.LocationChecker.AddCheckedLocation(completedBundleName + " Bundle");
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

        protected override JunimoNoteMenuRemake CreateNewMenu(int whichArea)
        {
            var snappedId = -1;
            if (currentlySnappedComponent != null && (currentlySnappedComponent.myID >= REGION_BUNDLE_MODIFIER || currentlySnappedComponent.myID == REGION_AREA_NEXT_BUTTON || currentlySnappedComponent.myID == REGION_AREA_BACK_BUTTON))
            {
                snappedId = currentlySnappedComponent.myID;
            }
            var junimoNoteMenu = _apWrapper.Factory.CreateJunimoNoteMenu(this, true, whichArea, true);
            junimoNoteMenu.GameMenuTabToReturnTo = GameMenuTabToReturnTo;
            if (snappedId >= 0)
            {
                junimoNoteMenu.currentlySnappedComponent = junimoNoteMenu.getComponentWithID(currentlySnappedComponent.myID);
                junimoNoteMenu.snapCursorToCurrentSnappedComponent();
            }
            if (junimoNoteMenu.getComponentWithID(AreaNextButton.leftNeighborID) != null)
            {
                junimoNoteMenu.AreaNextButton.leftNeighborID = AreaNextButton.leftNeighborID;
            }
            else
            {
                junimoNoteMenu.AreaNextButton.leftNeighborID = junimoNoteMenu.AreaBackButton.myID;
            }
            junimoNoteMenu.AreaNextButton.rightNeighborID = AreaNextButton.rightNeighborID;
            junimoNoteMenu.AreaNextButton.upNeighborID = AreaNextButton.upNeighborID;
            junimoNoteMenu.AreaNextButton.downNeighborID = AreaNextButton.downNeighborID;
            if (junimoNoteMenu.getComponentWithID(AreaBackButton.rightNeighborID) != null)
            {
                junimoNoteMenu.AreaBackButton.leftNeighborID = AreaBackButton.leftNeighborID;
            }
            else
            {
                junimoNoteMenu.AreaBackButton.leftNeighborID = junimoNoteMenu.AreaNextButton.myID;
            }
            junimoNoteMenu.AreaBackButton.rightNeighborID = AreaBackButton.rightNeighborID;
            junimoNoteMenu.AreaBackButton.upNeighborID = AreaBackButton.upNeighborID;
            junimoNoteMenu.AreaBackButton.downNeighborID = AreaBackButton.downNeighborID;

            return junimoNoteMenu;
        }
    }
}
