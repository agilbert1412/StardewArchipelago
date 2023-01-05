using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Netcode;
using StardewArchipelago.Locations;
using StardewValley;
using StardewValley.Objects;
using StardewValley.Tools;

namespace StardewArchipelago.Items
{
    public class UnlockManager
    {
        private Dictionary<string, Action<int>> _unlockables;
        public bool ShouldGiveItemsWithUnlocks { get; set; }

        public UnlockManager()
        {
            _unlockables = new Dictionary<string, Action<int>>();
            ShouldGiveItemsWithUnlocks = true;
            RegisterCommunityCenterRepairs();
            RegisterPlayerImprovement();
            RegisterProgressiveTools();
        }

        public bool IsUnlock(string unlockName)
        {
            return _unlockables.ContainsKey(unlockName);
        }

        public void PerformUnlock(string unlockName, int numberReceived)
        {
            _unlockables[unlockName](numberReceived);
        }

        private void RegisterCommunityCenterRepairs()
        {
            _unlockables.Add("Bridge Repair", (_) => RepairBridge());
            _unlockables.Add("Greenhouse", (_) => RepairGreenHouse());
            _unlockables.Add("Glittering Boulder Removed", (_) => RemoveGlitteringBoulder());
            _unlockables.Add("Minecarts Repair", (_) => RepairMinecarts());
            _unlockables.Add("Bus Repair", (_) => RepairBus());
            // _unlockables.Add("Movie Theater", BuildMovieTheater);
        }

        private void RegisterPlayerImprovement()
        {
            _unlockables.Add("Progressive Backpack", SetBackPackLevel);
        }

        private void RegisterProgressiveTools()
        {
            _unlockables.Add("Progressive Axe", ReceiveProgressiveAxe);
            _unlockables.Add("Progressive Pickaxe", ReceiveProgressivePickaxe);
            _unlockables.Add("Progressive Hoe", ReceiveProgressiveHoe);
            _unlockables.Add("Progressive Watering Can", ReceiveProgressiveWateringCan);
            _unlockables.Add("Progressive Trash Can", ReceiveProgressiveTrashCan);
            _unlockables.Add("Progressive Fishing Rod", ReceiveProgressiveFishingRod);
        }

        private void RepairBridge()
        {
            SendCommunityRepairMail("ccCraftsRoom");
        }

        private void RepairGreenHouse()
        {
            SendCommunityRepairMail("ccPantry");
        }

        private void RemoveGlitteringBoulder()
        {
            SendCommunityRepairMail("ccFishTank");
        }

        private void RepairMinecarts()
        {
            SendCommunityRepairMail("ccBoilerRoom");
        }

        private void RepairBus()
        {
            SendCommunityRepairMail("ccVault");
        }

        private void SendCommunityRepairMail(string mailTitle)
        {
            if (Game1.player.mailReceived.Contains(mailTitle))
            {
                return;
            }

            Game1.player.mailForTomorrow.Add(mailTitle + "%&NL&%");
        }

        private void SetBackPackLevel(int level)
        {
            var previousMaxItems = Game1.player.MaxItems;
            var backpack1Name = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
            var backpack2Name = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
            switch (level)
            {
                case <= 0:
                    Game1.player.MaxItems = 12;
                    break;
                case 1:
                    Game1.player.MaxItems = 24;
                    break;
                case >= 2:
                    Game1.player.MaxItems = 36;
                    break;
            }

            if (previousMaxItems >= Game1.player.MaxItems)
            {
                return;
            }

            while (Game1.player.Items.Count < Game1.player.MaxItems)
            {
                Game1.player.Items.Add(null);
            }
            Game1.player.holdUpItemThenMessage(new SpecialItem(99, level == 1 ? backpack1Name : backpack2Name));
        }

        private void ReceiveProgressiveAxe(int numberReceived)
        {
            ReceiveProgressiveTool(numberReceived, () => new Axe { UpgradeLevel = numberReceived }, "Axe");
        }

        private void ReceiveProgressivePickaxe(int numberReceived)
        {
            ReceiveProgressiveTool(numberReceived, () => new Pickaxe { UpgradeLevel = numberReceived }, "Pickaxe");
        }

        private void ReceiveProgressiveHoe(int numberReceived)
        {
            ReceiveProgressiveTool(numberReceived, () => new Hoe { UpgradeLevel = numberReceived }, "Hoe");
        }

        private void ReceiveProgressiveWateringCan(int numberReceived)
        {
            ReceiveProgressiveTool(numberReceived, () => new WateringCan {UpgradeLevel = numberReceived}, "Watering Can");
        }

        private void ReceiveProgressiveTrashCan(int numberReceived)
        {
            ReceiveProgressiveTool(numberReceived, () => new GenericTool { UpgradeLevel = numberReceived }, "Trash Can");
        }

        private void ReceiveProgressiveFishingRod(int numberReceived)
        {
            var modData = Game1.getFarm().modData;
            LocationsCodeInjection.InitializeFishingRodsModDataValues();
            LocationsCodeInjection.SetModDataValue(LocationsCodeInjection.RECEIVED_FISHING_ROD_LEVEL_KEY, numberReceived.ToString());

            if (!ShouldGiveItemsWithUnlocks || numberReceived < 1)
            {
                return;
            }

            var itemToAdd = new FishingRod(numberReceived - 1);

            Game1.player.holdUpItemThenMessage(itemToAdd);
            Game1.player.addItemByMenuIfNecessary(itemToAdd);
        }

        private void ReceiveProgressiveTool(int numberReceived, Func<Tool> toolCreationFunction, string toolGenericName = null)
        {
            var itemHasBeenRemoved = false;
            var player = Game1.player;
            if (!string.IsNullOrWhiteSpace(toolGenericName))
            {
                foreach (Item playerItem in player.Items)
                {
                    if (playerItem != null && playerItem is Tool && playerItem.Name.Contains(toolGenericName))
                    {
                        itemHasBeenRemoved = true;
                        Game1.player.removeItemFromInventory(playerItem);
                    }
                }
            }

            var newTool = toolCreationFunction();

            if (newTool == null)
            {
                return;
            }

            if (ShouldGiveItemsWithUnlocks)
            {
                Game1.player.holdUpItemThenMessage(newTool);
            }

            if (newTool is GenericTool)
            {
                Game1.player.trashCanLevel = numberReceived;
            }
            else
            {
                if (itemHasBeenRemoved || ShouldGiveItemsWithUnlocks)
                {
                    Game1.player.addItemByMenuIfNecessary(newTool);
                }
            }
        }
    }
}
