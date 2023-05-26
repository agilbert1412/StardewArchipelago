using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Locations.CodeInjections.Vanilla;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class CarpenterMenuArchipelago : BuildingMenuArchipelago
    {   
        
        public CarpenterMenuArchipelago(ArchipelagoClient archipelago) : base(archipelago)
        {
        }

        public CarpenterMenuArchipelago(IModHelper modHelper, ArchipelagoClient archipelago) : base(modHelper, archipelago, false)
        {
        }

        public override List<BluePrint> GetAvailableBlueprints()
        {
            var blueprints = new List<BluePrint>();
            var blueprintData = FullBlueprintData();
            foreach (var blueprint in blueprintData)
            {
                var blueprintMagical = blueprint.Value.magical;
                var blueprintUpgrade = blueprint.Value.blueprintType;

                if (blueprintMagical)
                {
                    if (blueprint.Key == "Stable")
                    {
                        AddBuildingBlueprintIfReceived(blueprints, blueprint.Key, true);
                        continue;
                    }
                    if (blueprintUpgrade == "none")
                    {
                        AddBuildingBlueprintIfReceived(blueprints, blueprint.Key, requiredBuilding: null);
                        continue;
                    }
                    AddBuildingBlueprintIfReceived(blueprints, blueprint.Key, requiredBuilding: blueprintUpgrade);
                }
            }
            AddBuildingBlueprintIfReceived(blueprints, CarpenterInjections.BUILDING_TRACTOR_GARAGE, true);
            return blueprints;
        }

        private void AddBuildingBlueprintIfReceived(List<BluePrint> blueprints, string buildingName, bool onlyOne = false, string requiredBuilding = null)
        {
            var hasReceivedBuilding = CarpenterInjections.HasReceivedBuilding(buildingName, out var sendingPlayer);
            if (!hasReceivedBuilding)
            {
                return;
            }

            AddBuildingBlueprint(blueprints, buildingName, sendingPlayer, onlyOne, requiredBuilding);
        }
    }
}
