using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Buildings;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Archipelago.SlotData.SlotEnums;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class CarpenterBuildingsModifier
    {
        protected static ILogger _logger;
        protected static IModHelper _helper;
        protected static StardewArchipelagoClient _archipelago;

        public CarpenterBuildingsModifier(ILogger logger, IModHelper helper, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _helper = helper;
            _archipelago = archipelago;
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
                    ChangePrices(buildingsData);
                    AddFreeBuildings(buildingsData);
                },
                AssetEditPriority.Late
            );
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
