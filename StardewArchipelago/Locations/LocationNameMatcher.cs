using System;
using System.Collections.Generic;
using System.Linq;
using StardewArchipelago.Constants.Locations;

namespace StardewArchipelago.Locations
{
    public class LocationNameMatcher
    {
        private readonly Dictionary<string, string[]> _wordFilterCache;

        public LocationNameMatcher()
        {
            _wordFilterCache = new Dictionary<string, string[]>();
        }

        public IEnumerable<string> GetAllLocationsMatching(IEnumerable<string> allLocations, string filter)
        {
            return allLocations.Where(x => x.Contains(filter, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> GetAllLocationsStartingWith(IEnumerable<string> allLocations, string prefix)
        {
            return allLocations.Where(x => x.StartsWith(prefix, StringComparison.InvariantCultureIgnoreCase));
        }

        public IEnumerable<string> GetAllLocationsEndingWith(IEnumerable<string> allLocations, string suffix)
        {
            return allLocations.Where(x => x.EndsWith(suffix, StringComparison.InvariantCultureIgnoreCase));
        }

        public string[] GetAllLocationsContainingWord(IEnumerable<string> allLocations, string wordFilter)
        {
            if (_wordFilterCache.ContainsKey(wordFilter))
            {
                return _wordFilterCache[wordFilter];
            }

            var filteredLocations = FilterForWord(GetAllLocationsMatching(allLocations, wordFilter), wordFilter);

            if (NameMatching.Exceptions.ContainsKey(wordFilter))
            {
                foreach (var exceptionName in NameMatching.Exceptions[wordFilter])
                {
                    filteredLocations = filteredLocations.Where(x =>
                        !x.Contains(exceptionName, StringComparison.InvariantCultureIgnoreCase));
                }
            }

            var filteredArray = filteredLocations.ToArray();
            _wordFilterCache.Add(wordFilter, filteredArray);
            return filteredArray;
        }

        public bool IsAnyLocationMatching(IEnumerable<string> allLocations, string filter)
        {
            return GetAllLocationsMatching(allLocations, filter).Any();
        }

        public bool IsAnyLocationStartingWith(IEnumerable<string> allLocations, string prefix)
        {
            return GetAllLocationsStartingWith(allLocations, prefix).Any();
        }

        public bool IsAnyLocationEndingWith(IEnumerable<string> allLocations, string suffix)
        {
            return GetAllLocationsEndingWith(allLocations, suffix).Any();
        }

        private static IEnumerable<string> FilterForWord(IEnumerable<string> locations, string filterWord)
        {
            foreach (var location in locations)
            {
                if (ItemIsRelevant(filterWord, location))
                {
                    yield return location;
                }
            }
        }

        private static bool ItemIsRelevant(string itemName, string locationName)
        {
            var startOfItemName = locationName.IndexOf(itemName, StringComparison.InvariantCultureIgnoreCase);
            if (startOfItemName == -1)
            {
                return false;
            }

            var charBefore = startOfItemName == 0 ? ' ' : locationName[startOfItemName - 1];
            var charAfter = locationName.Length <= startOfItemName + itemName.Length ? ' ' : locationName[startOfItemName + itemName.Length];
            return charBefore == ' ' && charAfter == ' ';
        }

        public void ClearCache()
        {
            _wordFilterCache.Clear();
        }
    }
}
