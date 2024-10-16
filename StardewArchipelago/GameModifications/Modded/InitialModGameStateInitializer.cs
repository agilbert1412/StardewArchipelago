using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Constants.Modded;
using StardewValley;

namespace StardewArchipelago.GameModifications.Modded
{
    public class InitialModGameStateInitializer
    {
        private static ILogger _logger;
        private static StardewArchipelagoClient _archipelago;

        public InitialModGameStateInitializer(ILogger logger, StardewArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
            GuntherInitializer();
        }

        public void GuntherInitializer()
        {
            if (_archipelago.SlotData.Mods.HasMod(ModNames.SVE) && !Game1.player.mailReceived.Contains("GuntherUnlocked"))
            {
                Game1.player.mailReceived.Add("GuntherUnlocked");
                Game1.player.eventsSeen.Add("103042015"); // Gunther says hi
                Game1.player.eventsSeen.Add("1579125"); // Marlon entering sewer immediately after; just annoying to see day one
            }
        }
    }
}
