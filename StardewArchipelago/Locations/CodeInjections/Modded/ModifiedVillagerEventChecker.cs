using System;
using StardewArchipelago.Locations.CodeInjections.Vanilla.Relationship;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.Locations.CodeInjections.Modded
{
    public class ModifiedVillagerEventChecker
    {
        private static ArchipelagoClient _archipelago;
        private const string JUNA_MOD = "Juna - Roommate NPC";
        private const string JUNA_MAIL_ONE = "JunaWizardletter2";
        private const string JUNA_MAIL_TWO = "JunaWizardletter4";
        private const int HEART_SIZE = 250;
        private static bool SENT_MAIL_ONE = false;
        private static bool SENT_MAIL_TWO = false;

        public static void Initialize(ArchipelagoClient archipelago)
        {
            _archipelago = archipelago;
        }

        public void CheckJunaHearts()
        {
            if (!_archipelago.SlotData.Mods.HasMod(JUNA_MOD))
            {
                return;
            }
            var juna = Game1.player.friendshipData["Juna"];
            if (juna.Points >= 3*HEART_SIZE && SENT_MAIL_ONE == false)
            {
                Game1.player.mailReceived.Add(JUNA_MAIL_ONE);
                SENT_MAIL_ONE = true;
            }
            if (juna.Points >= 6*HEART_SIZE && SENT_MAIL_TWO == false)
            {
                Game1.player.mailReceived.Add(JUNA_MAIL_TWO);
                SENT_MAIL_TWO = true;
            }
        }
    }
}