using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Stardew;
using StardewArchipelago.Textures;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using Bundle = StardewValley.Menus.Bundle;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.CC
{
    public static class JunimoNoteMenuInjections
    {
        private const int REMIXED_BUNDLE_INDEX_THRESHOLD = 37;
        private const int CUSTOM_BUNDLE_INDEX_THRESHOLD = 100;

        private static IMonitor _monitor;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static BundleReader _bundleReader;
        private static LocationChecker _locationChecker;

        public static void Initialize(IMonitor monitor, IModHelper modHelper, ArchipelagoClient archipelago, BundleReader bundleReader, LocationChecker locationChecker)
        {
            _monitor = monitor;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _bundleReader = bundleReader;
            _locationChecker = locationChecker;
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
                _monitor.Log($"Failed in {nameof(CheckForRewards_SendBundleChecks_PostFix)}:\n{ex}", LogLevel.Error);
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
                var remixedBundlesTexture = Game1.temporaryContent.Load<Texture2D>("LooseSprites\\JunimoNote");
                foreach (var bundle in __instance.bundles)
                {
                    var textureOverride = BundleIcons.GetBundleIcon(_modHelper, bundle.name);
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
                            bundle.bundleTextureIndexOverride = bundle.bundleIndex;
                            continue;
                        }

                        textureOverride = ArchipelagoTextures.GetColoredLogo(_modHelper, 32, ArchipelagoTextures.COLOR);
                    }

                    bundle.bundleTextureOverride = textureOverride;
                    bundle.bundleTextureIndexOverride = 0;
                }

                return;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(SetupMenu_AddTextureOverrides_Postfix)}:\n{ex}", LogLevel.Error);
                return;
            }
        }

        // public string getRewardNameForArea(int whichArea)
        public static bool GetRewardNameForArea_ScoutRoomRewards_Prefix(JunimoNoteMenu __instance, int whichArea, ref string __result)
        {
            try
            {
                var apAreaToScout = "???";
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
                        __result = "???";
                        return false; // don't run original logic
                }

                var scoutedItem = _archipelago.ScoutSingleLocation(apAreaToScout);
                var rewardText = $"Reward: {scoutedItem.PlayerName}'s {scoutedItem.GetItemName()}";
                __result = rewardText;
                return false; // don't run original logic
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(GetRewardNameForArea_ScoutRoomRewards_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
