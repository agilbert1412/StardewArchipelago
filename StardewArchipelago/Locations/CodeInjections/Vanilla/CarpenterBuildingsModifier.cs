using System;
using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley.GameData.Buildings;

namespace StardewArchipelago.Locations.CodeInjections.Vanilla
{
    public class CarpenterBuildingsModifier
    {
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
                },
                AssetEditPriority.Late
            );
        }

        private void ChangePrices(IDictionary<string, BuildingData> buildingsData)
        {
            var priceMultiplier = _archipelago.SlotData.BuildingPriceMultiplier;
            if (priceMultiplier - 1.0 < double.Epsilon)
            {
                return;
            }

            foreach (var (_, buildingData) in buildingsData)
            {
                var finalCost = (int)(buildingData.BuildCost * priceMultiplier);
                buildingData.BuildCost = finalCost;
                foreach (var buildingMaterial in buildingData.BuildMaterials)
                {
                    var amount = Math.Max(1, (int)(buildingMaterial.Amount * priceMultiplier));
                    buildingMaterial.Amount = amount;
                }
            }
        }
    }
}
