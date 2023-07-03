using System;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class ModifiedVillagerEventChecker
    {
        private const string JUNA_MOD = "Juna - Roommate NPC";
        private const string JUNA_MAIL_ONE = "JunaWizardletter2";
        private const string JUNA_MAIL_TWO = "JunaWizardletter4";
        private const int HEART_SIZE = 250;
        private static bool SENT_MAIL_ONE = false;
        private static bool SENT_MAIL_TWO = false;

        public void CheckJunaHearts(ArchipelagoClient _archipelago)
        {
            if (!_archipelago.SlotData.Mods.HasMod(JUNA_MOD) || !Game1.player.friendshipData.ContainsKey("Juna"))
            {
                return;
            }
            var juna = Game1.player.friendshipData["Juna"];
            if (juna.Points >= 3*HEART_SIZE && SENT_MAIL_ONE == false)
            {
                Game1.player.mailForTomorrow.Add(JUNA_MAIL_ONE);
                SENT_MAIL_ONE = true;
            }
            if (juna.Points >= 6*HEART_SIZE && SENT_MAIL_TWO == false)
            {
                Game1.player.mailForTomorrow.Add(JUNA_MAIL_TWO);
                SENT_MAIL_TWO = true;
            }
        }
    }
}