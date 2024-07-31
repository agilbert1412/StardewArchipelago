using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using StardewArchipelago.Bundles;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley.Menus;
using Bundle = StardewValley.Menus.Bundle;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public static class BundleInjections
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
        private static LocationChecker _locationChecker;
        private static BundlesManager _bundlesManager;

        public static void Initialize(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundlesManager bundlesManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _locationChecker = locationChecker;
            _bundlesManager = bundlesManager;
        }

        // public Bundle(int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, Point position, string textureName, JunimoNoteMenu menu)
        public static void BundleConstructor_GenerateBundleIngredients_Postfix(Bundle __instance, int bundleIndex, string rawBundleInfo,
            bool[] completedIngredientsList, Point position, string textureName, JunimoNoteMenu menu)
        {
            try
            {
                InitializeBundle(__instance, bundleIndex, rawBundleInfo, completedIngredientsList, textureName, menu);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(BundleConstructor_GenerateBundleIngredients_Postfix)}:\n{ex}");
                return;
            }
        }

        private static void InitializeBundle(Bundle bundle, int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, string textureName, JunimoNoteMenu menu)
        {
            var rawBundleParts = rawBundleInfo.Split('/');
            var bundleName = rawBundleParts[0];
            var bundleDisplayName = $"{bundleName} Bundle";
            var bundleFromArchipelago = _bundlesManager.BundleRooms.BundlesByName[bundleDisplayName];
            if (bundleFromArchipelago is not ItemBundle itemBundle)
            {
                return;
            }
            
            bundle.name = bundleName;
            bundle.label = bundleName;
            bundle.rewardDescription = string.Empty;
            bundle.complete = true;

            var numberAlreadyDonated = 0;
            var ingredients = new List<BundleIngredientDescription>();
            for (var i = 0; i < itemBundle.Items.Count; i++)
            {
                var bundleItem = itemBundle.Items[i];
                var alreadyCompleted = completedIngredientsList[i];
                var ingredient = bundleItem.CreateBundleIngredientDescription(alreadyCompleted);
                ingredients.Add(ingredient);
                if (alreadyCompleted)
                {
                    ++numberAlreadyDonated;
                }
                else
                {
                    bundle.complete = false;
                }
            }

            bundle.ingredients = ingredients;
            bundle.bundleColor = itemBundle.ColorIndex;
            bundle.numberOfIngredientSlots = itemBundle.NumberRequired;

            if (numberAlreadyDonated >= bundle.numberOfIngredientSlots)
            {
                bundle.complete = true;
            }
        }
    }
}
