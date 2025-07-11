using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults;
using StardewArchipelago.Constants.Modded;

namespace StardewArchipelago.Archipelago.ConnectionResults
{
    internal class MissingModsConnectionResult : ModErrorConnectionResult
    {
        public MissingModsConnectionResult(List<MissingMod> missingMods) : base(missingMods, GetErrorMessage(missingMods))
        {
        }

        private static string GetErrorMessage(List<MissingMod> missingMods)
        {
            var errorMessage = $"The slot you are connecting to has been created expecting modded content,\r\nbut not all expected mods are installed and active.";
            foreach (var missingMod in missingMods)
            {
                errorMessage += $"{Environment.NewLine}\tMod: {missingMod.ModName}, expected version: {missingMod.ModExpectedVersion}, current Version: {missingMod.ModCurrentVersion}";
            }

            return errorMessage;
        }
    }
}
