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

        public Dictionary<string, List<string>> BlueprintDict()
        {
            var blueprintDict = new Dictionary<string, List<string>>();
            Dictionary<string, string> blueprintData = Game1.content.Load<Dictionary<string, string>>("Data\\blueprints");
            foreach (var blueprintKey in blueprintData)
            {
                string[] strArray = blueprintKey.Value.Split('/');
                //Blueprints also have animals for some reason; ignore those.
                if (strArray.Count() < 18)
                {
                    continue;
                }
                var strArray2 = new List<string>(){strArray[10], strArray[11], strArray[18]};
                blueprintDict.Add(blueprintKey.Key, strArray2);
            }
            
            return blueprintDict;
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
                //Tractor Mod implementation utilizes an extremely oddball building type...
                if (buildingName == "Tractor Garage")
                {
                    blueprints.Add(new BluePrint("Stable"){
                    displayName = "Tractor Garage",
                    description = "A garage to store your tractor. Tractor included!",
                    maxOccupants = -794739,
                    moneyRequired = 150000,
                    tilesWidth = 4,
                    tilesHeight = 2,
                    sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                    itemsRequired = new Dictionary<int,int>(){{355, 20}, {337, 5}, {787, 5}},
                    magical = false
                    });

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
                    //Tractor Mod implementation utilizes an extremely oddball building type.  Using FreeBlueprint in case its needed in future.
                    blueprints.Add(new FreeBlueprint("Stable", sendingPlayer){
                    displayName = "Free Tractor Garage",
                    description = $"A gift from {sendingPlayer}. A garage to store your tractor. Tractor included!",
                    maxOccupants = -794739,
                    tilesWidth = 4,
                    tilesHeight = 2,
                    sourceRectForMenuView = new Rectangle(0, 0, 64, 96),
                    itemsRequired = new Dictionary<int, int>(),
                    magical = false
                    });
                }
                else
                {
                    blueprints.Add(new FreeBlueprint(buildingName, sendingPlayer));
                }
                
            }
        }
    }
}
