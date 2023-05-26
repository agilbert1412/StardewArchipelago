using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using Microsoft.Xna.Framework;

namespace StardewArchipelago.GameModifications.Buildings
{   
    public abstract class BuildingMenuArchipelago : CarpenterMenu
    {
        private List<string> ExcludedBuildings = new List<string>{
            "Stone Cabin", "Plank Cabin", "Log Cabin", "Greenhouse", "Mine Elevator"
        };
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

        public Dictionary<string, BluePrint> FullBlueprintData()
        {
            var fullBlueprintData = new Dictionary<string, BluePrint>();
            Dictionary<string, string> rawBlueprintData = Game1.content.Load<Dictionary<string, string>>("Data\\blueprints");
            foreach (var blueprintPair in rawBlueprintData)
            {
                if (ExcludedBuildings.Contains(blueprintPair.Key))
                {
                    continue;
                }
                var blueprintDataArray = blueprintPair.Value.Split('/');
                if (blueprintDataArray[0] == "animal")
                {
                    continue;
                }
                fullBlueprintData.Add(blueprintPair.Key, new BluePrint(blueprintPair.Key));
            }
            
            return fullBlueprintData;
        }

        protected virtual void AddBuildingBlueprint(List<BluePrint> blueprints, string buildingName, string sendingPlayer, bool onlyOne = false, string requiredBuilding = null)
        {
            var farm = Game1.getFarm();
            var isConstructedAlready = farm.isBuildingConstructed(buildingName);
            if (onlyOne && isConstructedAlready)
            {
                return;
            }

            if (requiredBuilding != null)
            {
                var requiredBuildingExists = farm.isBuildingConstructed(requiredBuilding);
                if (!requiredBuildingExists)
                {
                    return;
                }
            }

            var shouldBePaid = isConstructedAlready;
            if (!shouldBePaid && (buildingName.EndsWith("Coop") || buildingName.EndsWith("Barn") || buildingName.EndsWith("Shed")))
            {
                if (buildingName.StartsWith("Big"))
                {
                    shouldBePaid |= farm.isBuildingConstructed(buildingName.Replace("Big", "Deluxe"));
                }
                else
                {
                    shouldBePaid |= farm.isBuildingConstructed($"Big {buildingName}");
                    shouldBePaid |= farm.isBuildingConstructed($"Deluxe {buildingName}");
                }
            }
            if (shouldBePaid)
            {
                //Tractor Mod implementation utilizes an odd building type...
                if (buildingName == "Tractor Garage")
                {
                    blueprints.Add(CreateTractorGarageBlueprint(false));

                }
                else
                {
                    blueprints.Add(new BluePrint(buildingName));
                }

            }
            else
            {
                if (buildingName == "Tractor Garage")
                {
                    blueprints.Add(CreateTractorGarageBlueprint(true, sendingPlayer));
                }
                else
                {
                    blueprints.Add(new FreeBlueprint(buildingName, sendingPlayer));
                }

            }
        }

        private static BluePrint CreateTractorGarageBlueprint(bool free, string sendingPlayerName = null)
        {
            const string blueprintName = "Tractor Garage";
            const string tractorDescription = "A garage to store your tractor. Tractor included!";
            var garageBlueprint = new BluePrint("Stable")
            {
                displayName = blueprintName,
                description = tractorDescription,
                moneyRequired = 150000,
                itemsRequired = new Dictionary<int, int>() { { 355, 20 }, { 337, 5 }, { 787, 5 } },
            };

            if (free)
            {
                var freeGarageBlueprint = new FreeBlueprint("Stable", sendingPlayerName);
                freeGarageBlueprint.SetDisplayFields(blueprintName, tractorDescription, sendingPlayerName);
                garageBlueprint = freeGarageBlueprint;
            }

            garageBlueprint.maxOccupants = -794739;
            garageBlueprint.tilesWidth = 4;
            garageBlueprint.tilesHeight = 2;
            garageBlueprint.sourceRectForMenuView = new Rectangle(0, 0, 64, 96);
            garageBlueprint.magical = false;
            return garageBlueprint;
        }
    }
}
