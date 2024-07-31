using System;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class KentInjections
    {
        private static ILogger _logger;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(ILogger logger, ArchipelagoClient archipelago)
        {
            _logger = logger;
            _archipelago = archipelago;
        }

        // public static bool AddCharacterIfNecessary(string characterId, bool bypassConditions = false)
        public static bool AddCharacterIfNecessary_ConsiderSeasonsRandomizerForKent_Prefix(string characterId, ref bool bypassConditions)
        {
            try
            {
                if (characterId != "Kent")
                {
                    return true; // run original logic
                }

                if (Game1.Date.TotalDays < 112)
                {
                    return false; // don't run original logic
                }

                bypassConditions = true;
                return true; // don't run original logic
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed in {nameof(AddCharacterIfNecessary_ConsiderSeasonsRandomizerForKent_Prefix)}:\n{ex}");
                return true; // run original logic
            }
        }
    }
}
