using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StardewArchipelago.Archipelago.ConnectionResults
{
    internal class IncompatibleModsConnectionResult : ModErrorConnectionResult
    {
        public IncompatibleModsConnectionResult(List<IncompatibleMod> incompatibleMods) : base(incompatibleMods, GetErrorMessage(incompatibleMods))
        {
        }

        private static string GetErrorMessage(List<IncompatibleMod> incompatibleMods)
        {
            var errorMessage = $"You cannot play Archipelago with the following unsupported mods:";
            foreach (var incompatibleMod in incompatibleMods)
            {
                errorMessage += $"{Environment.NewLine}\t - {incompatibleMod.ModName}: {incompatibleMod.Justification}";
            }

            return errorMessage;
        }
    }
}
