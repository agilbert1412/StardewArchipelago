using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Items;
using StardewArchipelago.Locations.CodeInjections;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.Buildings
{
    public abstract class BuildingMenuArchipelago : CarpenterMenu
    {
        protected IModHelper _modHelper;
        protected ArchipelagoClient _archipelago;

        public BuildingMenuArchipelago(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
        }

        protected BuildingMenuArchipelago(IModHelper modHelper, ArchipelagoClient archipelago, bool magicalConstruction) : base(magicalConstruction)
        {
            _modHelper = modHelper;
            _archipelago = archipelago;

            var blueprintsField = _modHelper.Reflection.GetField<List<BluePrint>>(this, "blueprints");
            var blueprints = GetAvailableBlueprints();

            if (blueprints == null || !blueprints.Any())
            {
                return;
            }

            blueprintsField.SetValue(blueprints);

            setNewActiveBlueprint();
            if (!Game1.options.SnappyMenus)
                return;
            populateClickableComponentList();
            snapToDefaultClickableComponent();
        }

        public abstract List<BluePrint> GetAvailableBlueprints();

        protected virtual void AddBuildingBlueprint(List<BluePrint> blueprints, string buildingName, string sendingPlayer, bool onlyOne = false, string requiredBuilding = null)
        {
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
    }
}
