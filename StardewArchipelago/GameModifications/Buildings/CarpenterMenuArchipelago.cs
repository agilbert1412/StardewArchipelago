using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class CarpenterMenuArchipelago : CarpenterMenu
    {
        private IModHelper _modHelper;
        private ArchipelagoClient _archipelago;

        public CarpenterMenuArchipelago(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
        }

        public CarpenterMenuArchipelago(IModHelper modHelper, ArchipelagoClient archipelago) : base(false)
        {
            _modHelper = modHelper;
            _archipelago = archipelago;

            var blueprintsField = _modHelper.Reflection.GetField<List<BluePrint>>(this, "blueprints");
            var blueprints = GetAvailableBlueprints();

            blueprintsField.SetValue(blueprints);

            setNewActiveBlueprint();
            if (!Game1.options.SnappyMenus)
                return;
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public List<BluePrint> GetAvailableBlueprints()
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

        private void AddBuildingBlueprint(List<BluePrint> blueprints, string buildingName, bool onlyOne = false, string requiredBuilding = null)
        {
            var hasReceivedCoop = _archipelago.HasReceivedItem(GetBuildingArchipelagoName(buildingName), out var sendingPlayer);
            sendingPlayer = "ArchipelagoPlayer";
            hasReceivedCoop = true;
            if (!hasReceivedCoop)
            {
                return;
            }

            var isConstructedAlready = Game1.getFarm().isBuildingConstructed(buildingName);
            if (onlyOne && isConstructedAlready)
            {
                return;
            }

            if (requiredBuilding != null)
            {
                var requiredBuildingExists = Game1.getFarm().isBuildingConstructed(requiredBuilding);
                if (!requiredBuildingExists)
                {
                    return;
                }
            }

            if (isConstructedAlready)
            {
                blueprints.Add(new BluePrint(buildingName));
            }
            else
            {
                blueprints.Add(new FreeBlueprint(buildingName, sendingPlayer));
            }
        }

        private string GetBuildingArchipelagoName(string buildingName)
        {
            return $"{ItemParser.BUILDING_PREFIX}{buildingName}";
        }
    }
}
