using System;
using System.Collections.Generic;

namespace StardewArchipelago.Archipelago.ConnectionResults
{
    public class MissingModRequirementsConnectionResult : ModErrorConnectionResult
    {
        public MissingModRequirementsConnectionResult(List<MissingMod> missingModRequirements) : base(missingModRequirements, GetErrorMessage(missingModRequirements))
        {
        }

        private static string GetErrorMessage(List<MissingMod> missingModRequirements)
        {
            var errorMessage = $"The slot you are connecting to requires a content patcher,\r\n mod, but not all expected mods are installed and active.";
            foreach (var missingModRequirement in missingModRequirements)
            {
                errorMessage += $"{Environment.NewLine}\tMod: {missingModRequirement.ModName}, expected version: {missingModRequirement.ModExpectedVersion}, current Version: {missingModRequirement.ModCurrentVersion}";
            }

            return errorMessage;
        }
    }
}
