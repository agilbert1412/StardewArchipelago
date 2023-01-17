using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

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

            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_COOP);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_BARN);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_WELL);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_SILO);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_MILL);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_SHED);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_FISH_POND);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_STABLE, true);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_SLIME_HUTCH);

            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_BIG_COOP, requiredBuilding: CarpenterInjections.BUILDING_COOP);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_DELUXE_COOP, requiredBuilding: CarpenterInjections.BUILDING_BIG_COOP);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_BIG_BARN, requiredBuilding: CarpenterInjections.BUILDING_BARN);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_DELUXE_BARN, requiredBuilding: CarpenterInjections.BUILDING_BIG_BARN);
            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_BIG_SHED, requiredBuilding: CarpenterInjections.BUILDING_SHED);

            AddBuildingBlueprint(blueprints, CarpenterInjections.BUILDING_SHIPPING_BIN);
            return blueprints;
        }
    }
}
