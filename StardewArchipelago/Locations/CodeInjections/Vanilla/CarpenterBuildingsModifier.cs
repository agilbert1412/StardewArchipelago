using Force.DeepCloner;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;
using StardewArchipelago.Constants;
using StardewArchipelago.Stardew;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago.SlotData.SlotEnums.SlotDataRandomization;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class CarpenterBuildingsModifier
    {
        protected static ILogger _logger;
        protected static IModHelper _helper;
        protected static StardewArchipelagoClient _archipelago;
        private readonly StardewItemManager _itemManager;

        public CarpenterBuildingsModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago, StardewItemManager itemManager)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
            _itemManager = itemManager;
        }

        public void OnBuildingsRequested(object sender, AssetRequestedEventArgs e)
        {
            if (!e.NameWithoutLocale.IsEquivalentTo("Data/Buildings"))
            {
                return;
            }

            e.Edit(asset =>
                {
                    var buildingsData = asset.AsDictionary<string, BuildingData>().Data;
                    ChangeCostsBasedOnDataRandomization(buildingsData);
                    ChangePrices(buildingsData);
                    AddFreeBuildings(buildingsData);
                },
                AssetEditPriority.Late
            );
        }

        private void ChangeCostsBasedOnDataRandomization(IDictionary<string, BuildingData> buildingsData)
        {
            var shopsData = _archipelago.SlotData.DataRandomization.ShopsData;
            if (shopsData == null)
            {
                return;
            }
            foreach (var (buildingName, buildingData) in buildingsData)
            {
                if (!TryChangeCostBasedOnDataRandomization(shopsData, buildingName, buildingData))
                {

                }
            }
        }

        private bool TryChangeCostBasedOnDataRandomization(Dictionary<string, Dictionary<string, RandomizedShopItemData>> shopsData, string buildingName, BuildingData buildingData)
        {
            foreach (var (shopName, shopItems) in shopsData)
            {
                foreach (var (itemName, itemData) in shopItems)
                {
                    if (itemName.Equals(buildingName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (itemData.Currency != null && itemData.Currency != "Money")
                        {
                            buildingData.BuildCost = 0;
                        }
                        else if (itemData.Price != null)
                        {
                            buildingData.BuildCost = itemData.Price.Value;
                        }
                        if (itemData.Materials != null)
                        {
                            buildingData.BuildMaterials.Clear();
                            foreach (var (materialName, materialAmount) in itemData.Materials)
                            {
                                var materialItem = _itemManager.GetItemByName(materialName);
                                var material = new BuildingMaterial()
                                {
                                    ItemId = materialItem.GetQualifiedId(),
                                    Amount = materialAmount,
                                };
                                buildingData.BuildMaterials.Add(material);
                            }
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        private void ChangePrices(IDictionary<string, BuildingData> buildingsData)
        {
            var priceMultiplier = _archipelago.SlotData.BuildingPriceMultiplier;
            if (Math.Abs(priceMultiplier - 1.0) < 0.01)
            {
                return;
            }

            foreach (var (_, buildingData) in buildingsData)
            {
                var finalCost = (int)Math.Round(buildingData.BuildCost * priceMultiplier);
                buildingData.BuildCost = finalCost;
                if (buildingData.BuildMaterials == null)
                {
                    continue;
                }

                foreach (var buildingMaterial in buildingData.BuildMaterials)
                {
                    var amount = Math.Max(1, (int)Math.Round(buildingMaterial.Amount * priceMultiplier));
                    buildingMaterial.Amount = amount;
                }
            }
        }

        private void AddFreeBuildings(IDictionary<string, BuildingData> buildingsData)
        {
            foreach (var buildingName in buildingsData.Keys.ToArray())
            {
                var buildingData = buildingsData[buildingName];

                if (!_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive) && !buildingData.MagicalConstruction)
                {
                    continue;
                }

                var freebuildingData = buildingData.DeepClone();
                var archipelagoCondition = GameStateConditionProvider.GetReceivedBuildingCondition(buildingName);
                var hasBuildingCondition = GameStateConditionProvider.CreateHasBuildingOrHigherCondition(buildingName, true);
                var doesNotHaveBuildingCondition = GameStateConditionProvider.CreateHasBuildingOrHigherCondition(buildingName, false);

                freebuildingData.BuildCost = 0;
                freebuildingData.BuildMaterials?.Clear();
                freebuildingData.BuildingType = buildingData.BuildingType;
                freebuildingData.Description = $"A gift from a friend. {freebuildingData.Description}";
                freebuildingData.BuildCondition = $"{archipelagoCondition}, {doesNotHaveBuildingCondition}";
                buildingData.BuildCondition = $"{archipelagoCondition}, {hasBuildingCondition}";

                buildingsData.Add($"Free {buildingName}", freebuildingData);
            }
        }
    }
}
