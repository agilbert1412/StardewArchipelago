using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.GameModifications.Modded
{
    public class ModifiedVillagerEventChecker
    {
        private const string JUNA_MOD = "Juna - Roommate NPC";
        private const string JUNA_MAIL_ONE = "JunaWizardletter2";
        private const string JUNA_MAIL_TWO = "JunaWizardletter4";

        public void CheckJunaHearts(ArchipelagoClient _archipelago)
        {
            if (!_archipelago.SlotData.Mods.HasMod(JUNA_MOD))
            {
                return;
            }

            var junaHearts = Game1.player.getFriendshipHeartLevelForNPC("Juna");
            if (junaHearts >= 3 && !Game1.player.hasOrWillReceiveMail(JUNA_MAIL_ONE))
            {
                Game1.player.mailForTomorrow.Add(JUNA_MAIL_ONE);
            }
            if (junaHearts >= 6 && !Game1.player.hasOrWillReceiveMail(JUNA_MAIL_TWO))
            {
                Game1.player.mailForTomorrow.Add(JUNA_MAIL_TWO);
            }
        }
    }
}