using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net.Client.ConnectionResults;

namespace StardewArchipelago.Archipelago.ConnectionResults
{
    public class ModErrorConnectionResult : FailedConnectionResult
    {
        public IEnumerable<ProblemMod> ProblemModRequirements { get; }

        public ModErrorConnectionResult(IEnumerable<ProblemMod> problemModRequirements, string errorMessage) : base(errorMessage)
        {
            ProblemModRequirements = problemModRequirements;
        }
    }
}
