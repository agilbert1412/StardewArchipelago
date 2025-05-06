using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using KaitoKid.ArchipelagoUtilities.Net;
using Microsoft.Xna.Framework;
using StardewArchipelago.Bundles;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles.Remakes;
using StardewArchipelago.Serialization;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Logging;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ArchipelagoBundle : BundleRemake
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoWalletDto _wallet;
        private static LocationChecker _locationChecker;
        private static BundlesManager _bundlesManager;

        public ArchipelagoBundle(string name, string displayName, List<BundleIngredientDescription> ingredients, bool[] completedIngredientsList, string rewardListString = "") : base(name, displayName, ingredients, completedIngredientsList, rewardListString)
        {
        }

        public ArchipelagoBundle(int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, Point position, string textureName, ArchipelagoJunimoNoteMenu menu) : base(bundleIndex, rawBundleInfo, completedIngredientsList, position, textureName, menu)
        {
            InitializeBundle(bundleIndex, rawBundleInfo, completedIngredientsList, textureName, menu);
        }

        public static void InitializeArchipelago(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, ArchipelagoWalletDto wallet, LocationChecker locationChecker, BundlesManager bundlesManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _wallet = wallet;
            _locationChecker = locationChecker;
            _bundlesManager = bundlesManager;
        }

        private void InitializeBundle(int bundleIndex, string rawBundleInfo, bool[] completedIngredientsList, string textureName, ArchipelagoJunimoNoteMenu menu)
        {
            var rawBundleParts = rawBundleInfo.Split('/');
            var bundleName = rawBundleParts[0];
            var bundleDisplayName = $"{bundleName} Bundle";
            var bundleFromArchipelago = _bundlesManager.BundleRooms.BundlesByName[bundleDisplayName];
            if (bundleFromArchipelago is not ItemBundle itemBundle)
            {
                return;
            }

            name = bundleName;
            label = bundleName;
            RewardDescription = string.Empty;
            Complete = true;

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
                    Complete = false;
                }
            }

            Ingredients = ingredients;
            BundleColor = itemBundle.ColorIndex;
            NumberOfIngredientSlots = itemBundle.NumberRequired;

            if (numberAlreadyDonated >= NumberOfIngredientSlots)
            {
                Complete = true;
            }
        }
    }
}
