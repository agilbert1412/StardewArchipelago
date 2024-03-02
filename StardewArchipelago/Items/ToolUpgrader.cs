using System.Linq;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace StardewArchipelago.Items
{
    public class ToolUpgrader
    {

        public Tool UpgradeToolInEntireWorld(string toolGenericName)
        {
            var player = Game1.player;
            var toolName = toolGenericName.Replace(" ", "_");
            if (TryUpgradeToolInInventory(player, toolName, out var upgradedTool))
            {
                return upgradedTool;
            }

            if (TryUpgradeToolInChests(toolName, out upgradedTool))
            {
                return upgradedTool;
            }

            if (TryUpgradeToolInLostAndFoundBox(player, toolName, out upgradedTool))
            {
                return upgradedTool;
            }

            return null;
        }

        private static bool TryUpgradeToolInInventory(Farmer player, string toolName, out Tool upgradedTool)
        {
            foreach (var playerItem in player.Items)
            {
                if (TryUpgradeCorrectTool(toolName, playerItem, out upgradedTool))
                {
                    return true;
                }
            }

            upgradedTool = null;
            return false;
        }

        private static bool TryUpgradeToolInChests(string toolName, out Tool upgradedTool)
        {
            var locations = Game1.locations.ToList();

            foreach (var building in Game1.getFarm().buildings)
            {
                if (building?.indoors.Value == null)
                {
                    continue;
                }
                locations.Add(building.indoors.Value);
            }

            foreach (var gameLocation in locations)
            {
                foreach (var (tile, gameObject) in gameLocation.Objects.Pairs)
                {
                    if (gameObject is not Chest chest)
                    {
                        continue;
                    }

                    foreach (var chestItem in chest.Items)
                    {
                        if (TryUpgradeCorrectTool(toolName, chestItem, out upgradedTool))
                        {
                            return true;
                        }
                    }
                }
            }


            if (Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse)
            {
                foreach (var fridgeItem in farmHouse.GetFridge(false).Items)
                {
                    if (TryUpgradeCorrectTool(toolName, fridgeItem, out upgradedTool))
                    {
                        return true;
                    }
                }
            }

            if (Game1.getLocationFromName("IslandFarmHouse") is IslandFarmHouse islandHouse)
            {
                foreach (var fridgeItem in islandHouse.GetFridge(false).Items)
                {
                    if (TryUpgradeCorrectTool(toolName, fridgeItem, out upgradedTool))
                    {
                        return true;
                    }
                }
            }

            upgradedTool = null;
            return false;
        }

        private static bool TryUpgradeToolInLostAndFoundBox(Farmer player, string toolName, out Tool upgradedTool)
        {
            foreach (var lostAndFoundItem in player.team.returnedDonations)
            {
                if (TryUpgradeCorrectTool(toolName, lostAndFoundItem, out upgradedTool))
                {
                    return true;
                }
            }

            upgradedTool = null;
            return false;
        }

        private static bool TryUpgradeCorrectTool(string toolName, Item item, out Tool upgradedTool)
        {
            if (item is not Tool toolToUpgrade || !toolToUpgrade.Name.Replace(" ", "_").Contains(toolName))
            {
                upgradedTool = null;
                return false;
            }

            if (toolToUpgrade.UpgradeLevel < 4)
            {
                toolToUpgrade.UpgradeLevel++;
            }

            {
                upgradedTool = toolToUpgrade;
                return true;
            }
        }
    }
}
