using System;
using System.Collections.Generic;
using System.Linq;
using Force.DeepCloner;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Buildings;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class CarpenterBuildingsModifier
    {
        private static readonly string[] _progressiveBuildings = new[] { "Coop", "Barn", "Shed" };

        protected static IMonitor _monitor;
        protected static IModHelper _helper;
        protected static ArchipelagoClient _archipelago;

        public CarpenterBuildingsModifier(IMonitor monitor, IModHelper helper, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
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
            if (!_archipelago.SlotData.BuildingProgression.HasFlag(BuildingProgression.Progressive))
            {
                return;
            }

            foreach (var buildingName in buildingsData.Keys.ToArray())
            {
                var buildingData = buildingsData[buildingName];

                var freebuildingData = buildingData.DeepClone();
                var archipelagoCondition = GetReceivedBuildingCondition(buildingName);
                var hasBuildingCondition = GameStateConditionProvider.CreateHasBuildingAnywhereCondition(buildingName, true);
                var doesNotHaveBuildingCondition = GameStateConditionProvider.CreateHasBuildingAnywhereCondition(buildingName, false);

                freebuildingData.BuildCost = 0;
                freebuildingData.BuildMaterials?.Clear();
                freebuildingData.Description = $"A gift from a friend. {freebuildingData.Description}";
                freebuildingData.BuildCondition = $"{archipelagoCondition}, {doesNotHaveBuildingCondition}";
                buildingData.BuildCondition = $"{archipelagoCondition}, {hasBuildingCondition}";

                buildingsData.Add($"Free {buildingName}", freebuildingData);
            }
        }

        private static string GetReceivedBuildingCondition(string buildingName)
        {
            var itemName = buildingName;
            const string bigPrefix = "Big ";
            const string deluxePrefix = "Deluxe ";
            var amount = 1;
            if (buildingName.StartsWith(bigPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                amount = 2;
                itemName = $"Progressive {itemName[bigPrefix.Length..]}";
            }
            else if (buildingName.StartsWith(deluxePrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                amount = 3;
                itemName = $"Progressive {itemName[deluxePrefix.Length..]}";
            }
            else if (_progressiveBuildings.Contains(buildingName))
            {
                itemName = $"Progressive {itemName}";
            }
            return GameStateConditionProvider.CreateHasReceivedItemCondition(itemName, amount);
        }
    }
}
