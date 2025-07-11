using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
