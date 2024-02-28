using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewArchipelago.Archipelago;
using StardewModdingAPI;
using StardewValley;

namespace StardewArchipelago.GameModifications.CodeInjections
{
    internal class KentInjections
    {
        private static IMonitor _monitor;
        private static ArchipelagoClient _archipelago;

        public static void Initialize(IMonitor monitor, ArchipelagoClient archipelago)
        {
            _monitor = monitor;
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
                _monitor.Log($"Failed in {nameof(AddCharacterIfNecessary_ConsiderSeasonsRandomizerForKent_Prefix)}:\n{ex}", LogLevel.Error);
                return true; // run original logic
            }
        }
    }
}
