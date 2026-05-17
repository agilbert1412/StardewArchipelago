using KaitoKid.Utilities.Interfaces;
using Newtonsoft.Json;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.ApworldData;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;
using StardewArchipelago.Constants;
using StardewArchipelago.Constants.Locations;
using StardewArchipelago.Constants.Vanilla;
using StardewArchipelago.GameModifications.Shops;
using StardewArchipelago.Items;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Shops;
using StardewValley.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewValley.GameData.FarmAnimals;

namespace StardewArchipelago.GameModifications.RandomizedData
{
    public class AnimalsDataModifier
    {
        private static ILogger _logger;
        private static IModHelper _modHelper;
        private static StardewArchipelagoClient _archipelago;
        private static StardewItemManager _itemManager;
        private static DataRandomization _dataRandomization;

        public AnimalsDataModifier(ILogger logger, IModHelper modHelper, StardewArchipelagoClient archipelago, StardewItemManager itemManager, DataRandomization dataRandomization)
        {
            _logger = logger;
            _modHelper = modHelper;
            _archipelago = archipelago;
            _itemManager = itemManager;
            _dataRandomization = dataRandomization;
        }

        public void OnAnimalsDataRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/FarmAnimals"))
            {
                return;
            }

            e.Edit(asset =>
            {
                    if (!_dataRandomization.ShopsData.ContainsKey(ShopNames.MARNIES_RANCH))
                    {
                        return;
                    }

                    var animalsData = asset.AsDictionary<string, FarmAnimalData>().Data;

                    foreach (var animalId in animalsData.Keys.ToArray())
                    {
                        ModifyAnimalData(animalsData, animalId);
                    }
            },
            AssetEditPriority.Late + 1
            );
        }

        private void ModifyAnimalData(IDictionary<string, FarmAnimalData> animalsData, string animalId)
        {
            var animalData = animalsData[animalId];
            if (animalData.PurchasePrice <= 0)
            {
                return;
            }

            ModifyAnimalData(animalData, animalId);
        }

        private void ModifyAnimalData(FarmAnimalData animalData, string animalId)
        {
            var randomizedAnimalsData = _dataRandomization.ShopsData[ShopNames.MARNIES_RANCH];

            var animalName = animalId;

            if (!randomizedAnimalsData.ContainsKey(animalName))
            {
                if (!animalId.StartsWith("White "))
                {
                    _logger.LogWarning($"Animal ID [{animalId}] is purchaseable but does not have randomized data.");
                    return;
                }

                animalName = animalId.Substring("White ".Length);

                if (!randomizedAnimalsData.ContainsKey(animalName))
                {
                    _logger.LogWarning($"Animal Name [{animalName}] is purchaseable but does not have randomized data.");
                    return;
                }
            }

            var randomizedAnimalData = randomizedAnimalsData[animalName];
            ModifyAnimalData(animalData, randomizedAnimalData);
        }

        private void ModifyAnimalData(FarmAnimalData animalData, RandomizedShopItemData randomizedAnimalData)
        {
            animalData.CustomFields ??= new Dictionary<string, string>();
            ModifyAnimalCurrency(animalData, randomizedAnimalData);
            ModifyAnimalPrice(animalData, randomizedAnimalData);
            ModifyAnimalMaterials(animalData, randomizedAnimalData);
        }

        private static void ModifyAnimalCurrency(FarmAnimalData animalData, RandomizedShopItemData randomizedAnimalData)
        {
            if (!string.IsNullOrWhiteSpace(randomizedAnimalData.Currency))
            {
                animalData.CustomFields.TryAdd(ShopMenuInjections.CURRENCY_KEY, randomizedAnimalData.Currency);
                animalData.CustomFields[ShopMenuInjections.CURRENCY_KEY] = randomizedAnimalData.Currency;
            }
        }

        private static void ModifyAnimalPrice(FarmAnimalData animalData, RandomizedShopItemData randomizedAnimalData)
        {
            if (randomizedAnimalData.Price.HasValue)
            {
                animalData.PurchasePrice = randomizedAnimalData.Price.Value / 2;
            }
        }

        private static void ModifyAnimalMaterials(FarmAnimalData animalData, RandomizedShopItemData randomizedAnimalData)
        {
            if (randomizedAnimalData.Materials != null)
            {
                animalData.CustomFields.TryAdd(ShopMenuInjections.MATERIALS_KEY, "");
                var materialsDict = randomizedAnimalData.Materials.ToDictionary(x => _itemManager.GetItemByName(x.Key).GetQualifiedId(), x => Math.Max(1, x.Value));
                animalData.CustomFields[ShopMenuInjections.MATERIALS_KEY] = JsonConvert.SerializeObject(materialsDict);
            }
        }
    }
}
