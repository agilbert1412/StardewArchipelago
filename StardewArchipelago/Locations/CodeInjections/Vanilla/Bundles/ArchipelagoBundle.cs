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
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.Logging;
using StardewValley;
using Netcode;
using StardewValley.Network;
using StardewValley.TerrainFeatures;
using System;
using StardewValley.Extensions;
using Object = StardewValley.Object;
using Microsoft.Xna.Framework.Input;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ArchipelagoBundle : BundleRemake
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static ArchipelagoClient _archipelago;
        private static ArchipelagoStateDto _state;
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

        public static void InitializeArchipelago(ILogger logger, IModHelper modHelper, ArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundlesManager bundlesManager)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _state = state;
            _wallet = _state.Wallet;
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

        public override bool CanAcceptThisItem(Item item, ClickableTextureComponent slot, bool ignoreStackCount)
        {
            if (name == MemeBundleNames.BUREAUCRACY)
            {
                var currentIngredient = Ingredients[ArchipelagoJunimoNoteMenu.BureaucracyIndex];
                return item.QualifiedItemId == currentIngredient.id;
            }

            return base.CanAcceptThisItem(item, slot, ignoreStackCount);
        }

        public override bool IsValidItemForThisIngredientDescription(Item item, BundleIngredientDescription ingredient)
        {
            if (name == MemeBundleNames.BUREAUCRACY)
            {
                var currentIngredient = Ingredients[ArchipelagoJunimoNoteMenu.BureaucracyIndex];
                return item.QualifiedItemId == currentIngredient.id;
            }

            return base.IsValidItemForThisIngredientDescription(item, ingredient);
        }

        public override Item TryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName, JunimoNoteMenuRemake parentMenu)
        {
            if (name == MemeBundleNames.HONORABLE)
            {
                if (item.QualifiedItemId == QualifiedItemIds.STONE)
                {
                    _state.NumberTimesCursed++;
                    LightningStrikeOnce();
                    Game1.weatherForTomorrow = "Storm";
                    Game1.isRaining = true;
                    Game1.isSnowing = false;
                    Game1.isLightning = true;
                    Game1.isDebrisWeather = false;
                    Game1.isGreenRain = false;
                    Game1.updateWeather(Game1.currentGameTime);
                }
            }

            var itemAfterDeposit = base.TryToDepositThisItem(item, slot, noteTextureName, parentMenu);
            return itemAfterDeposit;
        }



        public static void LightningStrikeOnce()
        {
            Game1.flashAlpha = (float)(0.5 + Game1.random.NextDouble());
            Game1.playSound("thunder");
        }
    }
}
