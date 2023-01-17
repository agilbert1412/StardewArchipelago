using System.Collections.Generic;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

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

            AddBuildingBlueprint(blueprints, WizardInjections.BUILDING_EARTH_OBELISK);
            AddBuildingBlueprint(blueprints, WizardInjections.BUILDING_WATER_OBELISK);
            AddBuildingBlueprint(blueprints, WizardInjections.BUILDING_DESERT_OBELISK);
            AddBuildingBlueprint(blueprints, WizardInjections.BUILDING_ISLAND_OBELISK);
            AddBuildingBlueprint(blueprints, WizardInjections.BUILDING_JUNIMO_HUT);
            AddBuildingBlueprint(blueprints, WizardInjections.BUILDING_GOLD_CLOCK);

            return blueprints;
        }
    }
}
