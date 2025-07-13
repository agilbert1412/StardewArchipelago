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
using StardewArchipelago.Constants.Vanilla;
using StardewValley;
using System;
using StardewArchipelago.Constants;
using StardewValley.Locations;
using StardewValley.Objects;
using System.Linq;
using StardewArchipelago.Archipelago;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla.Bundles
{
    public class ArchipelagoBundle : BundleRemake
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
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

        public static void InitializeArchipelago(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, ArchipelagoStateDto state, LocationChecker locationChecker, BundlesManager bundlesManager)
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
            if (!_bundlesManager.BundleRooms.BundlesByName.ContainsKey(bundleDisplayName))
            {
                bundleDisplayName = bundleName;
                if (!_bundlesManager.BundleRooms.BundlesByName.ContainsKey(bundleDisplayName))
                {
                    throw new Exception($"Unrecognized bundle name: {bundleName}");
                }
            }
            var bundleFromArchipelago = _bundlesManager.BundleRooms.BundlesByName[bundleDisplayName];

            // BundleColor = bundleFromArchipelago.ColorIndex;

            if (name == MemeBundleNames.COMMUNISM)
            {
                Ingredients[0] = new BundleIngredientDescription(Ingredients[0].id, Game1.player.Money, Ingredients[0].quality, Ingredients[0].completed, Ingredients[0].preservesId);
            }

            InitializeNameAndLabel(bundleName);

            if (bundleFromArchipelago is not ItemBundle itemBundle)
            {
                return;
            }

            RewardDescription = string.Empty;
            Complete = IsBundleComplete(completedIngredientsList, itemBundle, out var ingredients);

            Ingredients = ingredients;
            NumberOfIngredientSlots = itemBundle.NumberRequired;
        }

        private static bool IsBundleComplete(bool[] completedIngredientsList, ItemBundle itemBundle, out List<BundleIngredientDescription> ingredients)
        {
            var numberAlreadyDonated = 0;
            ingredients = new List<BundleIngredientDescription>();
            var isComplete = true;
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
                    isComplete = false;
                }
            }

            if (numberAlreadyDonated >= itemBundle.NumberRequired)
            {
                isComplete = true;
            }
            return isComplete;
        }

        private void InitializeNameAndLabel(string bundleName)
        {
            var nameForPlayer = GetBundleNameForPlayer(bundleName);
            name = nameForPlayer;
            label = nameForPlayer;
        }
        private string GetBundleNameForPlayer(string bundleName)
        {
            if (bundleName == MemeBundleNames.SCAM)
            {
                return MemeBundleNames.INVESTMENT;
            }
            return bundleName;
        }

        public override bool CanAcceptThisItem(Item item, ClickableTextureComponent slot, bool ignoreStackCount, ArchipelagoJunimoNoteMenu parentMenu)
        {
            if (name == MemeBundleNames.PERMIT_A38)
            {
                var currentIngredient = Ingredients[ArchipelagoJunimoNoteMenu.BureaucracyIndex];
                return item.QualifiedItemId == currentIngredient.id;
            }

            return base.CanAcceptThisItem(item, slot, ignoreStackCount, parentMenu);
        }

        public override bool IsValidItemForThisIngredientDescription(Item item, BundleIngredientDescription ingredient, int ingredientIndex, ArchipelagoJunimoNoteMenu parentMenu)
        {
            if (name == MemeBundleNames.PERMIT_A38 && item is not null)
            {
                var currentIngredient = Ingredients[ArchipelagoJunimoNoteMenu.BureaucracyIndex];
                return item.QualifiedItemId == currentIngredient.id;
            }
            if (name == MemeBundleNames.OFF_YOUR_BACK)
            {
                return IsValidItemForOffYourBackIngredientDescription(item, ingredient);
            }
            if (name == MemeBundleNames.COMMITMENT)
            {
                var baseValid = base.IsValidItemForThisIngredientDescription(item, ingredient, ingredientIndex, parentMenu);
                if (!baseValid)
                {
                    return baseValid;
                }
                return IsValidItemForCommitmentIngredientDescription(item, ingredient);
            }
            if (name == MemeBundleNames.IKEA && ingredientIndex >= Ingredients.Count)
            {
                return false;
            }
            if (name == MemeBundleNames.MERMAID)
            {
                var sequence = new[] { 1, 5, 4, 2, 3 };
                var numberIngredientsDonated = Ingredients.Count(x => x.completed);
                var onlyValidIngredient = sequence[numberIngredientsDonated] - 1;
                if (ingredientIndex != onlyValidIngredient)
                {
                    return false;
                }
            }
            if (name == MemeBundleNames.LOSER_CLUB && ingredient.id == QualifiedItemIds.TUNA)
            {
                if (item == null || item.QualifiedItemId != QualifiedItemIds.TUNA || item.modData == null || !item.modData.ContainsKey(GarbageInjections.FROM_TRASH_KEY))
                {
                    return false;
                }

                return bool.TryParse(item.modData[GarbageInjections.FROM_TRASH_KEY], out var fromTrash) && fromTrash;
            }
            if (name == MemeBundleNames.COOPERATION)
            {
                return false;
            }
            if (name == MemeBundleNames.ANIMAL_WELL && item.QualifiedItemId == QualifiedItemIds.ADVANCED_TV_REMOTE && !_archipelago.SlotData.ExcludeGingerIsland)
            {
                return false;
            }

            return base.IsValidItemForThisIngredientDescription(item, ingredient, ingredientIndex, parentMenu);
        }

        private static bool IsValidItemForOffYourBackIngredientDescription(Item item, BundleIngredientDescription ingredient)
        {
            if (item == null || ingredient.completed)
            {
                return false;
            }
            if (item is Hat && ingredient.id == MemeIDProvider.WORN_HAT)
            {
                return true;
            }
            if (item is Boots && ingredient.id == MemeIDProvider.WORN_BOOTS)
            {
                return true;
            }
            if (item is Clothing pants && pants.clothesType.Value == Clothing.ClothesType.PANTS && ingredient.id == MemeIDProvider.WORN_PANTS)
            {
                return true;
            }
            if (item is Clothing shirt && shirt.clothesType.Value == Clothing.ClothesType.SHIRT && ingredient.id == MemeIDProvider.WORN_SHIRT)
            {
                return true;
            }
            if (item is Ring ring && (ingredient.id == MemeIDProvider.WORN_LEFT_RING || ingredient.id == MemeIDProvider.WORN_RIGHT_RING))
            {
                return true;
            }

            return false;
        }

        private static bool IsValidItemForCommitmentIngredientDescription(Item item, BundleIngredientDescription ingredient)
        {
            if (item == null || ingredient.completed)
            {
                return false;
            }

            if (item.QualifiedItemId == QualifiedItemIds.BOUQUET)
            {
                return Game1.player.friendshipData.Values.Any(x => x.IsDating());
            }

            if (item.QualifiedItemId == QualifiedItemIds.MERMAID_PENDANT)
            {
                return Game1.player.friendshipData.Values.Any(x => x.IsMarried());
            }

            if (item.QualifiedItemId == QualifiedItemIds.WILTED_BOUQUET)
            {
                return Game1.player.friendshipData.Values.Any(x => x.IsDivorced());
            }

            if (item.QualifiedItemId == QualifiedItemIds.ANCIENT_DOLL)
            {
                return Game1.stats.Get("childrenTurnedToDoves") >= ingredient.stack;
            }

            return false;
        }

        public override Item TryToDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName, ArchipelagoJunimoNoteMenu parentMenu)
        {
            if (name == MemeBundleNames.BUNDLE_BUNDLE)
            {
                if (parentMenu._bundleBundleIndex >= 0)
                {
                    var doneBefore = parentMenu.SubBundleComplete(parentMenu._bundleBundleIndex);
                    var result = base.TryToDepositThisItem(item, slot, noteTextureName, parentMenu);
                    var doneAfter = parentMenu.SubBundleComplete(parentMenu._bundleBundleIndex);
                    if (!doneBefore && doneAfter)
                    {
                        var completeBefore = Complete;
                        parentMenu.HeldItem = result;
                        CompletionAnimation();
                        Complete = completeBefore;
                    }
                    return result;
                }
            }
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

        protected override Item SuccessfullyDepositThisItem(Item item, ClickableTextureComponent slot, string noteTextureName, ArchipelagoJunimoNoteMenu parentMenu, BundleIngredientDescription ingredientDescription1, int index1, CommunityCenter communityCenter)
        {
            var result = base.SuccessfullyDepositThisItem(item, slot, noteTextureName, parentMenu, ingredientDescription1, index1, communityCenter);
            if (name == MemeBundleNames.OFF_YOUR_BACK)
            {
                _state.QualifiedIdsClothesDonated.Add(item.QualifiedItemId);
            }
            if (name == MemeBundleNames.POOL)
            {
                if (ingredientDescription1.stack != 8)
                {
                    return result;
                }

                if (parentMenu.CheckIfAllIngredientsAreDeposited())
                {
                    return result;
                }

                Ingredients = Ingredients.Select(x => new BundleIngredientDescription(x, false)).ToList();
                communityCenter.bundleRewards[BundleIndex] = true;
                for (var i = 0; i < communityCenter.bundles.FieldDict[BundleIndex].Length; i++)
                {
                    communityCenter.bundles.FieldDict[BundleIndex][i] = false;
                }
                foreach (var ingredientSlot in parentMenu.IngredientSlots)
                {
                    ingredientSlot.item = null;
                }
            }
            if (name == MemeBundleNames.SISYPHUS)
            {
                ArchipelagoJunimoNoteMenu.SisyphusStoneNeedsToFall = true;
            }
            return result;
        }
    }
}
