using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace StardewArchipelago
{
    internal class UnlockManager
    {
        private Dictionary<string, Action> _unlockables;

        public UnlockManager()
        {
            _unlockables = new Dictionary<string, Action>();
            RegisterCommunityCenterRepairs();
        }

        public void DoUnlock(string unlockName)
        {
            _unlockables[unlockName]();
        }

        private void RegisterCommunityCenterRepairs()
        {
            _unlockables.Add("Bridge Repair", RepairBridge);
            _unlockables.Add("Greenhouse", RepairGreenHouse);
            _unlockables.Add("Glittering Boulder Removed", RemoveGlitteringBoulder);
            _unlockables.Add("Minecarts Repair", RepairMinecarts);
            _unlockables.Add("Bus Repair", RepairBus);
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

        private static void SendCommunityRepairMail(string mailTitle)
        {
            if (Game1.player.mailReceived.Contains(mailTitle))
            {
                return;
            }

            Game1.player.mailForTomorrow.Add(mailTitle + "%&NL&%");
        }
    }
}
