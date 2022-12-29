using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Netcode;
using StardewValley;
using StardewValley.Objects;

namespace StardewArchipelago.Items
{
    public class UnlockManager
    {
        private Dictionary<string, Action<int>> _unlockables;

        public UnlockManager()
        {
            _unlockables = new Dictionary<string, Action<int>>();
            RegisterCommunityCenterRepairs();
            RegisterPlayerImprovement();
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

        private void RegisterPlayerImprovement()
        {
            _unlockables.Add("Progressive Backpack", (numberReceived) => SetBackPackLevel(numberReceived));
        }

        private void SetBackPackLevel(int level)
        {
            var previousMaxItems = Game1.player.MaxItems;
            var backpack1Name = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8708");
            var backpack2Name = Game1.content.LoadString("Strings\\StringsFromCSFiles:GameLocation.cs.8709");
            switch (level)
            {
                case 0:
                    Game1.player.MaxItems = 12;
                    break;
                case 1:
                    Game1.player.MaxItems = 24;
                    break;
                case 2:
                    Game1.player.MaxItems = 36;
                    break;
            }

            if (previousMaxItems < Game1.player.MaxItems)
            {
                while (Game1.player.Items.Count <= Game1.player.MaxItems)
                {
                    Game1.player.Items.Add(null);
                }
                Game1.player.holdUpItemThenMessage(new SpecialItem(99, level == 1 ? backpack1Name : backpack2Name));
            }
        }
    }
}
