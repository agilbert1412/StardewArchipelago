using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.ArchipelagoUtilities.Net.Interfaces;
using StardewArchipelago.Archipelago;
using StardewArchipelago.Goals;

namespace StardewArchipelago.Locations
{
    public class StardewLocationChecker : LocationChecker
    {
        private readonly LocationNameMatcher _locationNameMatcher;

        public StardewLocationChecker(ILogger logger, StardewArchipelagoClient archipelago, List<string> locationsAlreadyChecked) : base(logger, archipelago, locationsAlreadyChecked)
        {
            _locationNameMatcher = new LocationNameMatcher();
        }

        public override void SendAllLocationChecks()
        {
            base.SendAllLocationChecks();
            GoalCodeInjection.CheckAllsanityGoalCompletion();
        }

        public override void ClearCache()
        {
            base.ClearCache();
            _locationNameMatcher.ClearCache();
        }

        public IEnumerable<string> GetAllLocationsNotChecked(string filter)
        {
            return _locationNameMatcher.GetAllLocationsMatching(GetAllLocationsNotChecked(), filter);
        }

        public IEnumerable<string> GetAllLocationsNotCheckedStartingWith(string prefix)
        {
            return _locationNameMatcher.GetAllLocationsStartingWith(GetAllLocationsNotChecked(), prefix);
        }

        public string[] GetAllLocationsNotCheckedContainingWord(string wordFilter)
        {
            return _locationNameMatcher.GetAllLocationsContainingWord(GetAllLocationsNotChecked(), wordFilter);
        }

        public bool IsAnyLocationNotChecked(string filter)
        {
            return _locationNameMatcher.IsAnyLocationMatching(GetAllLocationsNotChecked(), filter);
        }

        public bool IsAnyLocationNotCheckedStartingWith(string prefix)
        {
            return _locationNameMatcher.IsAnyLocationStartingWith(GetAllLocationsNotChecked(), prefix);
        }
    }
}
