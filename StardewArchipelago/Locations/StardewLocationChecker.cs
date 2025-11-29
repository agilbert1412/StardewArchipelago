using System.Collections.Generic;
using KaitoKid.ArchipelagoUtilities.Net;
using KaitoKid.ArchipelagoUtilities.Net.Client;
using KaitoKid.Utilities.Interfaces;
using StardewArchipelago.Goals;

namespace StardewArchipelago.Locations
{

    public class StardewLocationChecker : LocationChecker, ILocationChecker
    {
        private const bool PREVENT_SENDING_CHECKS = false;

        private readonly LocationNameMatcher _locationNameMatcher;

        public StardewLocationChecker(ILogger logger, ArchipelagoClient archipelago, List<string> locationsAlreadyChecked) : base(logger, archipelago, locationsAlreadyChecked)
        {
            _locationNameMatcher = new LocationNameMatcher();
        }

        public override void SendAllLocationChecks()
        {
#pragma warning disable CS0162 // Unreachable code detected
            if (PREVENT_SENDING_CHECKS)
            {
                return;
            }
#pragma warning restore CS0162 // Unreachable code detected

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

        public IEnumerable<string> GetAllLocationsNotCheckedMatchingExactly(string filter)
        {
            return _locationNameMatcher.GetAllLocationsMatchingExactly(GetAllLocationsNotChecked(), filter);
        }

        public IEnumerable<string> GetAllLocationsNotCheckedStartingWith(string prefix)
        {
            return _locationNameMatcher.GetAllLocationsStartingWith(GetAllLocationsNotChecked(), prefix);
        }

        public IEnumerable<string> GetAllLocationsStartingWith(string prefix)
        {
            return _locationNameMatcher.GetAllLocationsStartingWith(GetAllLocations(), prefix);
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

        public bool IsAnyLocationNotCheckedEndingWith(string prefix)
        {
            return _locationNameMatcher.IsAnyLocationEndingWith(GetAllLocationsNotChecked(), prefix);
        }
    }
}
