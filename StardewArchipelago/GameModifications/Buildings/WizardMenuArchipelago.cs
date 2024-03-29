﻿using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class WizardMenuArchipelago : BuildingMenuArchipelago
    {
        public WizardMenuArchipelago(IModHelper modHelper, ArchipelagoClient archipelago) : base(modHelper, archipelago, true)
        {
        }

        public override List<BluePrint> GetAvailableBlueprints()
        {
            var blueprints = new List<BluePrint>();
            var blueprintData = FullBlueprintData();
            foreach (var blueprint in blueprintData)
            {
                var blueprintMagical = blueprint.magical;

                if (blueprintMagical)
                {
                    AddBuildingBlueprintIfReceived(blueprints, blueprint.name);
                }
            }
            return blueprints;
        }

        private void AddBuildingBlueprintIfReceived(List<BluePrint> blueprints, string buildingName, bool onlyOne = false, string requiredBuilding = null)
        {
            var hasReceivedBuilding = _archipelago.HasReceivedItem(buildingName,  out var sendingPlayer);
            if (!hasReceivedBuilding)
            {
                return;
            }

            AddBuildingBlueprint(blueprints, buildingName, sendingPlayer, onlyOne, requiredBuilding);
        }
    }
}
