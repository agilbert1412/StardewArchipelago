using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewValley;

namespace StardewArchipelago.GameModifications.Modded
{
    public class ModifiedVillagerEventChecker
    {
        private const string JUNA_MAIL_ONE = "JunaWizardletter2";
        private const string JUNA_MAIL_TWO = "JunaWizardletter4";

        public void CheckJunaHearts(StardewArchipelagoClient archipelago)
        {
            if (archipelago == null || !archipelago.SlotData.Mods.HasMod(ModNames.JUNA))
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
