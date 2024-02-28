using Force.DeepCloner;
using StardewValley;
using StardewValley.GameData.Buildings;
using StardewValley.Menus;

namespace StardewArchipelago.GameModifications.Buildings
{
    public class FreeBlueprint : CarpenterMenu.BlueprintEntry
    {
        public FreeBlueprint(int index, string id, BuildingData data, string skinId, string sendingPlayerName) : base(index, id, data, skinId)
        {

            var freeBuildingData = data.DeepClone();
            freeBuildingData.BuildCost = 0;
            freeBuildingData.BuildMaterials.Clear();
            freeBuildingData.Name = $"Free {data.Name}";
            freeBuildingData.Description = $"A gift from {sendingPlayerName}. {data.Description}";
        }
    }
}
